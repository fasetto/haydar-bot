using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Haydar.Models;
using LiteDB;

namespace Haydar.Modules
{
    [Name("Config Module")]
    public class ConfigModule: ModuleBase<SocketCommandContext>
    {
        private readonly LiteRepository _repository;

        public ConfigModule(LiteRepository repository)
        {
            _repository = repository;
        }

        [Command("prefix")]
        [Summary("To Change current prefix")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task SetPrefix(string name)
        {
            var all = _repository.Query<DiscordGuild>().ToList();

            var guild = new DiscordGuild()
            {
                Id = Context.Guild.Id,
                Prefix = name
            };

            var result = _repository.Upsert(guild);
            await ReplyAsync($"Prefix changed succesfully.");
        }
    }
}
