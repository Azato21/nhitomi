// Copyright (c) 2018-2019 chiya.dev
// 
// This software is released under the MIT License.
// https://opensource.org/licenses/MIT

using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Options;
using nhitomi.Globalization;

namespace nhitomi.Discord
{
    public interface IDiscordContext
    {
        IUserMessage Message { get; }
        IMessageChannel Channel { get; }

        IUser User { get; }
        Localization Localization { get; }
    }

    public class DiscordService : DiscordSocketClient
    {
        readonly AppSettings _settings;

        public DiscordService(IOptions<AppSettings> options) : base(options.Value.Discord)
        {
            _settings = options.Value;
        }

        public async Task ConnectAsync()
        {
            if (LoginState != LoginState.LoggedOut)
                return;

            // login
            await LoginAsync(TokenType.Bot, _settings.Discord.Token);
            await StartAsync();
        }
    }

    public static class DiscordContextExtensions
    {
        public static IDisposable BeginTyping(this IDiscordContext context) => context.Channel.EnterTypingState();

        public static Task ReplyAsync(this IDiscordContext context, string localizationKey, object variables = null) =>
            context.Channel.SendMessageAsync(new LocalizationPath(localizationKey)[context.Localization](variables));
    }
}