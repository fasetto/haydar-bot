using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Haydar.Models;
using LiteDB;
using Microsoft.Extensions.Configuration;

namespace Haydar.Services
{
    public class CommandHandlingService
    {
        private readonly DiscordSocketClient _discord;
        private readonly CommandService _commands;
        private IServiceProvider _provider;
        private readonly LiteRepository _repository;
        private readonly Config _config;

        public CommandHandlingService(IServiceProvider provider, DiscordSocketClient discord, CommandService commands, Config config, LiteRepository repository)
        {
            _discord = discord;
            _commands = commands;
            _provider = provider;
            _config = config;
            _repository = repository;

            _discord.MessageReceived += MessageReceived;
        }

        public async Task InitializeAsync(IServiceProvider provider)
        {
            _provider = provider;
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), provider);
            // Add additional initialization code here...
        }

        private async Task MessageReceived(SocketMessage rawMessage)
        {
            // Ignore system messages and messages from bots
            if (!(rawMessage is SocketUserMessage message)) return;
            if (message.Source != MessageSource.User) return;

            var context = new SocketCommandContext(_discord, message);

            var guild = _repository.Query<DiscordGuild>()
                .Where(g => g.Id == context.Guild.Id)
                .FirstOrDefault();

            var prefix = _config.BotConfig.Prefix;

            if (guild != null)
                prefix = guild.Prefix;

            int argPos = 0;
            // if (!message.HasMentionPrefix(_discord.CurrentUser, ref argPos)) return;
            if (!message.HasStringPrefix(prefix, ref argPos)) return;

            await _commands.ExecuteAsync(context, argPos, _provider);
        }

    }
}
