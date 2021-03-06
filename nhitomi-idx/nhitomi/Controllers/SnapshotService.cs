using System;
using System.Threading;
using System.Threading.Tasks;
using MessagePack;
using Microsoft.Extensions.Options;
using Microsoft.IO;
using nhitomi.Database;
using nhitomi.Models;
using nhitomi.Models.Queries;
using nhitomi.Storage;
using OneOf;
using OneOf.Types;

namespace nhitomi.Controllers
{
    public class SnapshotServiceOptions
    {
        /// <summary>
        /// Maximum number of characters for serialized data embedded in snapshots.
        /// If this is exceeded, data is stored in storage instead.
        /// </summary>
        public int MaxEmbeddedDataChars { get; set; } = 1024;
    }

    public class SnapshotArgs
    {
        /// <summary>
        /// Creation time of the snapshot.
        /// If null, the update or creation time of the snapshot value will be used.
        /// If default, database added time will be used.
        /// </summary>
        public DateTime? Time { get; set; }

        public SnapshotSource Source { get; set; }
        public SnapshotEvent Event { get; set; }
        public OneOf<DbUser, string>? Committer { get; set; }
        public DbSnapshot Rollback { get; set; }
        public string Reason { get; set; }

        public void Validate()
        {
            if (Source == SnapshotSource.System && Committer != null)
                throw new ArgumentException("Committer must be null when snapshot source is the system.");

            if (Source != SnapshotSource.System && Committer == null)
                throw new ArgumentException("Snapshot source must not be the system when the committer is specified.");
        }
    }

    public interface ISnapshotService
    {
        /// <summary>
        /// Retrieves a snapshot object.
        /// Note that <paramref name="id"/> refers to the ID of the snapshot itself, whereas <paramref name="type"/> is the type of the snapshot's value.
        /// </summary>
        Task<OneOf<DbSnapshot, NotFound>> GetAsync(ObjectType type, string id, CancellationToken cancellationToken = default); // not using nhitomiObject for semantic correctness

        /// <summary>
        /// Retrieves the serialized value of a snapshot.
        /// </summary>
        Task<OneOf<T, NotFound>> GetValueAsync<T>(DbSnapshot snapshot, CancellationToken cancellationToken = default) where T : class, IDbObject, IDbHasType;

        Task<SearchResult<DbSnapshot>> SearchAsync(ObjectType type, SnapshotQuery query, CancellationToken cancellationToken = default);
        Task<int> CountAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Reverts an object to the given snapshot's value. Snapshot representing the rollback is created after indexing.
        /// Returned is new value of the reverted object and the snapshot representing rollback.
        /// </summary>
        Task<OneOf<(T, DbSnapshot), NotFound>> RollbackAsync<T>(DbSnapshot snapshot, SnapshotArgs snapshotRepresentingRollback, CancellationToken cancellationToken = default) where T : class, IDbObject, IDbHasType;

        /// <summary>
        /// Creates a snapshot of an object.
        /// </summary>
        Task<DbSnapshot> CreateAsync<T>(T obj, SnapshotArgs args, CancellationToken cancellationToken = default) where T : class, IDbObject, IDbHasType;
    }

    public class SnapshotService : ISnapshotService
    {
        static readonly MessagePackSerializerOptions _serializerOptions =
            MessagePackSerializerOptions
               .Standard
               .WithCompression(MessagePackCompression.Lz4Block);

        readonly IElasticClient _client;
        readonly IStorage _storage;
        readonly IOptionsMonitor<SnapshotServiceOptions> _options;
        readonly RecyclableMemoryStreamManager _memory;

        public SnapshotService(IElasticClient client, IStorage storage, IOptionsMonitor<SnapshotServiceOptions> options, RecyclableMemoryStreamManager memory)
        {
            _client  = client;
            _storage = storage;
            _options = options;
            _memory  = memory;
        }

        public async Task<OneOf<DbSnapshot, NotFound>> GetAsync(ObjectType type, string id, CancellationToken cancellationToken = default)
        {
            var snapshot = await _client.GetAsync<DbSnapshot>(id, cancellationToken);

            if (snapshot?.Target != type)
                return new NotFound();

            return snapshot;
        }

        public async Task<OneOf<T, NotFound>> GetValueAsync<T>(DbSnapshot snapshot, CancellationToken cancellationToken = default) where T : class, IDbObject, IDbHasType
        {
            // data embedded in snapshot
            if (snapshot.Data != null)
            {
                var value = MessagePackSerializer.Deserialize<T>(MessagePackSerializer.ConvertFromJson(snapshot.Data, _serializerOptions), _serializerOptions);

                if (value != null && value.Type == snapshot.Target && value.Id == snapshot.TargetId)
                    return value;

                //throw new SnapshotMismatchException($"Snapshot {snapshot.Id} of {snapshot.Target} {snapshot.TargetId} does not match with actual value {value.SnapshotTarget} {value.Id}");
            }

            // read serialized snapshot value
            var readResult = await _storage.ReadAsync($"snapshots/{snapshot.Id}", cancellationToken);

            if (readResult.TryPickT0(out var file, out _))
                await using (file)
                {
                    var value = MessagePackSerializer.Deserialize<T>(await file.Stream.ToArrayAsync(cancellationToken), _serializerOptions);

                    if (value != null && value.Type == snapshot.Target && value.Id == snapshot.TargetId)
                        return value;

                    //throw new SnapshotMismatchException($"Snapshot {snapshot.Id} of {snapshot.Target} {snapshot.TargetId} does not match with actual value {value.SnapshotTarget} {value.Id}");
                }

            // if rollback is specified, get value of rollback
            if (snapshot.RollbackId != null)
            {
                var rollback = await GetAsync(snapshot.Target, snapshot.RollbackId, cancellationToken);

                if (rollback.IsT0)
                {
                    var value = await GetValueAsync<T>(rollback.AsT0, cancellationToken);

                    if (value.IsT0)
                        return value.AsT0;
                }

                //throw new SnapshotMismatchException($"Rollback snapshot {snapshot.Id} references invalid target snapshot: {rollback?.Id ?? "<null>"}");
            }

            return new NotFound();
        }

        public Task<SearchResult<DbSnapshot>> SearchAsync(ObjectType type, SnapshotQuery query, CancellationToken cancellationToken = default)
            => _client.SearchAsync(new DbSnapshotQueryProcessor(type, query), cancellationToken);

        public Task<int> CountAsync(CancellationToken cancellationToken = default)
            => _client.CountAsync<DbSnapshot>(cancellationToken);

        public async Task<OneOf<(T, DbSnapshot), NotFound>> RollbackAsync<T>(DbSnapshot snapshot, SnapshotArgs snapshotRepresentingRollback, CancellationToken cancellationToken = default) where T : class, IDbObject, IDbHasType
        {
            // get snapshot value
            var result = await GetValueAsync<T>(snapshot, cancellationToken);

            if (!result.TryPickT0(out var value, out var error))
                return error;

            // update entry
            var entry = await _client.GetEntryAsync<T>(snapshot.TargetId, cancellationToken);

            do
            {
                entry.Value = value;
            }
            while (!await entry.TryUpdateAsync(cancellationToken)); // this will upsert

            var rollback = null as DbSnapshot;

            // create snapshot representing rollback
            if (snapshotRepresentingRollback != null)
                rollback = await CreateAsync(entry.Value, snapshotRepresentingRollback, cancellationToken);

            return (value, rollback);
        }

        public async Task<DbSnapshot> CreateAsync<T>(T obj, SnapshotArgs args, CancellationToken cancellationToken = default) where T : class, IDbObject, IDbHasType
        {
            args.Validate();

            if (obj.Id == null)
                throw new ArgumentException($"Cannot create snapshot of {typeof(T).Name} with uninitialized ID: {obj}");

            var options = _options.CurrentValue;

            var entry = _client.Entry(new DbSnapshot
            {
                // ensure snapshot time is in sync with object time
                CreatedTime = args.Time ?? obj switch
                {
                    IHasUpdatedTime u => u.UpdatedTime,
                    IHasCreatedTime c => c.CreatedTime,

                    _ => default
                },

                Source      = args.Source,
                Event       = args.Event,
                RollbackId  = args.Rollback?.Id,
                CommitterId = args.Committer?.Match(u => u.Id, s => s),
                Target      = obj.Type,
                TargetId    = obj.Id,
                Reason      = args.Reason
            });

            var data = MessagePackSerializer.Serialize(obj, _serializerOptions);

            // try embedding serialized data in string form into snapshot
            var dataStr = MessagePackSerializer.ConvertToJson(data, _serializerOptions);

            if (dataStr.Length <= options.MaxEmbeddedDataChars)
            {
                entry.Value.Data = dataStr;
            }

            // otherwise use storage
            else
            {
                await using var file = new StorageFile
                {
                    Name   = $"snapshots/{entry.Id}",
                    Stream = _memory.GetStream(data)
                };

                await _storage.WriteAsync(file, cancellationToken);
            }

            return await entry.CreateAsync(cancellationToken);
        }
    }
}