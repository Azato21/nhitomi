// Copyright (c) 2018-2019 chiya.dev
// 
// This software is released under the MIT License.
// https://opensource.org/licenses/MIT

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Discord;
using Microsoft.Extensions.Options;
using nhitomi.Core;
using nhitomi.Database;
using Newtonsoft.Json;

namespace nhitomi
{
    public class MessageFormatter
    {
        public static IEmote FloppyDiskEmote => new Emoji("\uD83D\uDCBE");
        public static IEmote TrashcanEmote => new Emoji("\uD83D\uDDD1");
        public static IEmote HeartEmote => new Emoji("\u2764");
        public static IEmote LeftArrowEmote => new Emoji("\u25c0");
        public static IEmote RightArrowEmote => new Emoji("\u25b6");

        readonly AppSettings _settings;
        readonly ISet<IDoujinClient> _clients;
        readonly JsonSerializer _json;

        public MessageFormatter(
            IOptions<AppSettings> options,
            ISet<IDoujinClient> clients,
            JsonSerializer json)
        {
            _settings = options.Value;
            _clients = clients;
            _json = json;
        }

        static string Join(IEnumerable<string> values)
        {
            var array = values?.ToArray();

            return array == null || array.Length == 0
                ? null
                : string.Join(", ", array);
        }

        public Embed CreateDoujinEmbed(IDoujin doujin)
        {
            var embed = new EmbedBuilder()
                .WithTitle(doujin.OriginalName ?? doujin.PrettyName)
                .WithDescription(doujin.OriginalName == doujin.PrettyName ? null : doujin.PrettyName)
                .WithAuthor(a => a
                    .WithName(Join(doujin.Artists) ?? doujin.Source.Name)
                    .WithIconUrl(doujin.Source.IconUrl))
                .WithUrl(doujin.SourceUrl)
                .WithImageUrl(doujin.Pages.First().Url)
                .WithColor(Color.Green)
                .WithFooter($"{doujin.Source.Name}/{doujin.Id}");

            embed.AddFieldSafe("Language", doujin.Language, true);
            embed.AddFieldSafe("Parody of", doujin.ParodyOf, true);
            embed.AddFieldSafe("Categories", Join(doujin.Categories), true);
            embed.AddFieldSafe("Characters", Join(doujin.Characters), true);
            embed.AddFieldSafe("Tags", Join(doujin.Tags), true);

            embed.AddField("Content", $"{doujin.PageCount} pages", true);

            return embed.Build();
        }

        public Task AddDoujinTriggersAsync(IUserMessage message) =>
            message.AddReactionsAsync(new[]
            {
                FloppyDiskEmote,
                TrashcanEmote
            });

        public Task AddFeedDoujinTriggersAsync(IUserMessage message) =>
            message.AddReactionAsync(HeartEmote);

        public Task AddListTriggersAsync(IUserMessage message) =>
            message.AddReactionsAsync(new[]
            {
                LeftArrowEmote,
                RightArrowEmote
            });

        public Embed CreateHelpEmbed() =>
            new EmbedBuilder()
                .WithTitle("**nhitomi**: Help")
                .WithDescription(
                    "nhitomi — a Discord bot for searching and downloading doujinshi, by __chiya.dev__.\n" +
                    $"Join our server: {_settings.Discord.Guild.GuildInvite}")
                .AddField("  — Commands —", $@"
Doujinshi:
- {_settings.Discord.Prefix}get __source__ __id__ — Retrieves doujin information from a source by its ID.
- {_settings.Discord.Prefix}all __source__ — Displays all doujins from a source uploaded recently.
- {_settings.Discord.Prefix}search __query__ — Searches for doujins by the title and tags that satisfy your query.
- {_settings.Discord.Prefix}download __source__ __id__ — Sends a download link for a doujin by its ID.

Tag subscriptions:
- {_settings.Discord.Prefix}subscription — Lists all tags you are subscribed to.
- {_settings.Discord.Prefix}subscription add|remove __tag__ — Adds or removes a tag subscription.
- {_settings.Discord.Prefix}subscription enable|disable — Enables or disables subscription notifications.
- {_settings.Discord.Prefix}subscription clear — Removes all tag subscriptions.

Collection management:
- {_settings.Discord.Prefix}collection — Lists all collections belonging to you.
- {_settings.Discord.Prefix}collection __name__ — Displays doujins belonging to a collection.
- {_settings.Discord.Prefix}collection __name__ add __source__ __id__ — Adds a doujin to a collection.
- {_settings.Discord.Prefix}collection __name__ list — Lists all doujins belonging to a collection.
- {_settings.Discord.Prefix}collection __name__ sort __attribute__ — Reorders doujins in a collection by their attribute.
- {_settings.Discord.Prefix}collection __name__ delete — Deletes a collection, removing all doujins belonging to it.

Miscellaneous:
- {_settings.Discord.Prefix}help — Shows this help message.
".Trim())
                .AddField("  — Sources —", @"
- nhentai — `https://nhentai.net/`
- hitomi — `https://hitomi.la/`
~~- tsumino — `https://tsumino.com/`~~
~~- pururin — `https://pururin.io/`~~
".Trim())
                .WithColor(Color.Purple)
                .WithCurrentTimestamp()
                .Build();

        public Embed CreateErrorEmbed() =>
            new EmbedBuilder()
                .WithTitle("**nhitomi**: Error")
                .WithDescription(
                    "Sorry, we encountered an unexpected error and have reported it to the developers! " +
                    $"Please join our official server for further assistance: {_settings.Discord.Guild.GuildInvite}")
                .WithColor(Color.Red)
                .WithCurrentTimestamp()
                .Build();

        public Embed CreateDownloadEmbed(IDoujin doujin)
        {
            var downloadToken = TokenGenerator.CreateToken(
                new TokenGenerator.ProxyDownloadPayload
                {
                    Source = doujin.Source.Name,
                    Id = doujin.Id,
                    RequestThrottle = doujin.Source.RequestThrottle,
                    Expires = TokenGenerator.GetExpirationFromNow(_settings.Doujin.DownloadValidLength)
                },
                _settings.Discord.Token,
                serializer: _json);

            return new EmbedBuilder()
                .WithTitle($"**{doujin.Source.Name}**: {doujin.OriginalName ?? doujin.PrettyName}")
                .WithUrl($"{_settings.Http.Url}/download?token={HttpUtility.UrlEncode(downloadToken)}")
                .WithDescription(
                    $"Click the link above to start downloading `{doujin.OriginalName ?? doujin.PrettyName}`.\n")
                .WithColor(Color.LightOrange)
                .WithCurrentTimestamp()
                .Build();
        }

        public string UnsupportedSource(string source) =>
            $"**nhitomi**: Source __{source}__ is not supported. " +
            $"Please see refer to the manual (**{_settings.Discord.Prefix}help**) for a full list of supported sources.";

        public string DoujinNotFound(string source = null) =>
            $"**{source ?? "nhitomi"}**: No such doujin!";

        public string InvalidQuery(string source = null) =>
            $"**{source ?? "nhitomi"}**: Please specify your query.";

        public string JoinGuildForDownload =>
            $"**nhitomi**: Please join our server to enable downloading! {_settings.Discord.Guild.GuildInvite}";

        public string BeginningOfList =>
            "**nhitomi**: Beginning of list!";

        public string EndOfList =>
            "**nhitomi**: End of list!";

        public string EmptyList(string source = null) =>
            $"**{source ?? "nhitomi"}**: No results!";

        public Embed CreateSubscriptionListEmbed(string[] tags) =>
            new EmbedBuilder()
                .WithTitle("**nhitomi**: Subscriptions")
                .WithDescription(tags.Length == 0
                    ? "You have no subscriptions."
                    : $"- {string.Join("\n- ", tags)}")
                .WithColor(Color.Gold)
                .WithCurrentTimestamp()
                .Build();

        public string AddedSubscription(string tag) =>
            $"**nhitomi**: Added tag subscription '{tag}'.";

        public string AlreadySubscribed(string tag) =>
            $"**nhitomi**: You are already subscribed to the tag '{tag}'.";

        public string RemovedSubscription(string tag) =>
            $"**nhitomi**: Removed tag subscription '{tag}'.";

        public string NotSubscribed(string tag) =>
            $"**nhitomi**: You are not subscribed to the tag '{tag}'.";

        public string ClearedSubscriptions =>
            "**nhitomi**: Cleared all tag subscriptions.";

        public Embed CreateCollectionListEmbed(string[] collectionNames) =>
            new EmbedBuilder()
                .WithTitle("**nhitomi**: Collections")
                .WithDescription(collectionNames.Length == 0
                    ? "You have no collections."
                    : $"- {string.Join("\n- ", collectionNames)}")
                .WithColor(Color.Teal)
                .WithCurrentTimestamp()
                .Build();

        public string AddedToCollection(string collectionName, IDoujin doujin) =>
            $"**nhitomi**: Added '{doujin.OriginalName ?? doujin.PrettyName}' to collection '{collectionName}'.";

        public string AlreadyInCollection(string collectionName, IDoujin doujin) =>
            $"**nhitomi**: '{doujin.OriginalName ?? doujin.PrettyName}' already exists in collection '{collectionName}'.";

        public Embed CreateCollectionEmbed(string collectionName, CollectionItemInfo[] items) =>
            new EmbedBuilder()
                .WithTitle($"**nhitomi**: Collection '{collectionName}'")
                .WithDescription(items.Length == 0
                    ? "There are no doujins in this collection."
                    : $"- {string.Join("\n- ", items.Select(i => $"{i.Source}/{i.Id}: {i.Artist} — {i.Name}"))}")
                .WithColor(Color.Magenta)
                .WithCurrentTimestamp()
                .Build();

        public string CollectionDeleted(string collectionName) =>
            $"**nhitomi**: Deleted collection '{collectionName}'.";

        public string CollectionNotFound =>
            $"**nhitomi**: No such collection!";
    }
}