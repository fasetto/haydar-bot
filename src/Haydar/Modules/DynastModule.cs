using System.Threading.Tasks;
using Discord.Commands;
using Haydar.Api;

namespace Haydar.Modules
{
    public class DynastModule : ModuleBase<SocketCommandContext>
    {
        private readonly DynastApi _api;
        public DynastModule(DynastApi api)
        {
            _api = api;
        }

        [Command("toplist"), Summary("Prints top10 players based on their score")]
        public Task TopList(string region)
        {
            //TODO: Fetch the toplist from api and send it.
            return ReplyAsync("toplist command executed.");
        }

    }
}
