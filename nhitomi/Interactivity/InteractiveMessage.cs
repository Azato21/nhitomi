using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using nhitomi.Interactivity.Triggers;

namespace nhitomi.Interactivity
{
    public abstract class InteractiveMessage : EmbedMessage, IDisposable
    {
        public IReadOnlyDictionary<IEmote, ReactionTrigger> Triggers { get; private set; }

        protected InteractiveMessage()
        {
            Triggers = CreateTriggers().ToDictionary(t => t.Emote);
        }

        protected abstract IEnumerable<ReactionTrigger> CreateTriggers();

        public override async Task InitializeAsync(IServiceProvider services, ICommandContext context,
            CancellationToken cancellationToken = default)
        {
            await base.InitializeAsync(services, context, cancellationToken);

            // initialize reaction triggers
            Triggers = CreateTriggers().ToDictionary(t => t.Emote);

            foreach (var trigger in Triggers.Values)
                trigger.Initialize(this);

            await Message.AddReactionsAsync(Triggers.Keys.ToArray());
        }

        public virtual void Dispose()
        {
        }
    }
}