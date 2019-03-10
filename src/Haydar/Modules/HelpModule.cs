
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Haydar.Models;

namespace Haydar.Modules
{
    public class HelpModule: ModuleBase<SocketCommandContext>
    {
        private readonly CommandService _service;
        private readonly Config _config;

        public HelpModule(CommandService service, Config config)
        {
            _service = service;
            _config = config;
        }


        public async Task Help()
        {
            var prefix = _config.BotConfig.Prefix;
            var builder = new EmbedBuilder()
            {
                Color = new Color(0xFD3439),
                Description = ":closed_book: These are the commands you can use"
            };

            foreach (var module in _service.Modules)
            {
                string description = "";
                foreach (var cmd in module.Commands)
                {
                    var result = await cmd.CheckPreconditionsAsync(Context);
                    if (result.IsSuccess)
                    {
                        description += $"**{prefix}{cmd.Aliases.FirstOrDefault()}**\n";
                        description += cmd.Summary + "\n\n";
                    }
                }

                if (!string.IsNullOrWhiteSpace(description))
                {
                    builder.AddField(x =>
                    {
                        x.Name = module.Name;
                        x.Value = description;
                        x.IsInline = false;
                    });
                }
            }

            await ReplyAsync("", embed: builder.Build());
        }

        [Command("help"), Summary("Prints this help message.")]
        public async Task Help(string command = null)
        {
            if (command == null)
            {
                await Help();
                return;
            }

            var result = _service.Search(Context, command);

            if (!result.IsSuccess)
            {
                await ReplyAsync($":expressionless: I couldn't find that command.");
                return;
            }

            string prefix = _config.BotConfig.Prefix;
            var builder = new EmbedBuilder()
            {
                Color = new Color(0xFD3439),
                Description = $"Here are some commands like **{command}**"
            };

            foreach (var match in result.Commands)
            {
                var cmd = match.Command;

                builder.AddField(x =>
                {
                    x.Name = string.Join(", ", cmd.Aliases);
                    x.Value = $"Parameters: {string.Join(", ", cmd.Parameters.Select(p => p.Name))}\n" +
                              $"Summary: {cmd.Summary}";
                    x.IsInline = false;
                });
            }

            await ReplyAsync("", embed: builder.Build());
        }
    }
}
