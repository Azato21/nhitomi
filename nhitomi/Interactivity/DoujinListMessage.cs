using System.Collections.Generic;
using Discord;
using nhitomi.Core;
using nhitomi.Interactivity.Triggers;

namespace nhitomi.Interactivity
{
    public class DoujinListMessage : ListInteractiveMessage<Doujin>
    {
        public DoujinListMessage(EnumerableBrowser<Doujin> enumerable) : base(enumerable)
        {
        }

        protected override IEnumerable<ReactionTrigger> CreateTriggers()
        {
            yield return new FavoriteTrigger();
            yield return new DownloadTrigger();

            foreach (var trigger in base.CreateTriggers())
                yield return trigger;

            yield return new DeleteTrigger();
        }

        protected override Embed CreateEmbed(Doujin value) => DoujinMessage.CreateEmbed(value);
        protected override Embed CreateEmptyEmbed() => throw new System.NotImplementedException();
    }
}