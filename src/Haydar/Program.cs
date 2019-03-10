using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Haydar.Services;
using LiteDB;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Haydar.Models;
using Haydar.Api;

namespace Haydar
{
    class Program
    {
        static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();

        private DiscordSocketClient _client;
        private Config _config;

        public async Task MainAsync()
        {
            _client = new DiscordSocketClient();
            _config = ReadConfig();

            var services = ConfigureServices();
            services.GetRequiredService<LogService>();
            await services.GetRequiredService<CommandHandlingService>().InitializeAsync(services);

            if (_config.Debug)
                await _client.LoginAsync(TokenType.Bot, _config.BotConfig.TokenBeta);
            else
                await _client.LoginAsync(TokenType.Bot, _config.BotConfig.Token);

            await _client.StartAsync();
            await Task.Delay(-1);
        }

        private Config ReadConfig()
        {
            var configStr = File.ReadAllText("config.json");
            var config = JsonConvert.DeserializeObject<Config>(configStr);
            return config;
        }

        private IServiceProvider ConfigureServices()
        {
            return new ServiceCollection()
                // Base
                .AddSingleton(_client)
                .AddSingleton<CommandService>()
                .AddSingleton<CommandHandlingService>()
                .AddPaginator(_client)
                // Logging
                // .AddLogging(x => x.AddConsole())
                .AddSingleton<LogService>()
                // Extra
                .AddSingleton(_config)
                .AddSingleton(new LiteDatabase("haydar.db"))
                .AddSingleton<DynastApi>()
                // Add additional services here...
                .BuildServiceProvider();
        }
    }
}
