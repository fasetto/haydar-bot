using System.Threading.Tasks;
using Discord.Commands;
using Haydar.Api;
using Haydar.Models;

namespace Haydar.Modules
{
    public class DynastModule : ModuleBase<SocketCommandContext>
    {
        private readonly DynastApi _api;
        private readonly Config _config;

        public DynastModule(DynastApi api, Config config)
        {
            _api = api;
            _config = config;
        }

        [Command("toplist"), Summary("Prints top10 players based on their score")]
        public Task TopList(string region)
        {
            //TODO: Fetch the toplist from api and send it.
            return ReplyAsync("toplist command executed.");
        }

        //TODO: Add Help command
        //TODO: Add Invite command
        //TODO: Add Contributors command

    }
}
