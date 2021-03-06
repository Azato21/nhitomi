using System;
using MessagePack;
using Nest;
using nhitomi.Models;

namespace nhitomi.Database
{
    /// <summary>
    /// Represents a restriction on a user.
    /// Restricted users are denied access to some endpoints.
    /// </summary>
    [MessagePackObject]
    public class DbUserRestriction : DbModelBase<UserRestriction>, IDbModelConvertible<DbUserRestriction, UserRestriction>
    {
        [Key("s"), Date(Name = "s")]
        public DateTime StartTime { get; set; }

        [Key("e"), Date(Name = "e")]
        public DateTime? EndTime { get; set; }

        [Key("m"), Keyword(Name = "m", DocValues = false)]
        public string ModeratorId { get; set; }

        [Key("r"), Text(Name = "r")]
        public string Reason { get; set; }

        public override void MapTo(UserRestriction model, IServiceProvider services)
        {
            base.MapTo(model, services);

            model.StartTime   = StartTime;
            model.EndTime     = EndTime;
            model.ModeratorId = ModeratorId;
            model.Reason      = Reason;
        }

        public override void MapFrom(UserRestriction model, IServiceProvider services)
        {
            base.MapFrom(model, services);

            StartTime   = model.StartTime;
            EndTime     = model.EndTime;
            ModeratorId = model.ModeratorId;
            Reason      = model.Reason;
        }
    }
}