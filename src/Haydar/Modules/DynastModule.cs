using System.Data;
using System.Threading.Tasks;
using ConsoleTableExt;
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
        public async Task TopList(string region = null)
        {
            var toplist = await _api.FetchToplistAsync(region);

            var scoreTable = new DataTable();
            scoreTable.Columns.Add("# SERVER", typeof(string));
            scoreTable.Columns.Add("SCORE", typeof(string));
            scoreTable.Columns.Add("LEVEL", typeof(string));
            scoreTable.Columns.Add("NICKNAME", typeof(string));
            scoreTable.Columns.Add("PLAYER COUNT", typeof(string));

            foreach (var server in toplist)
                scoreTable.Rows.Add(server.Label, server.TopPlayerScore, server.TopPlayerLevel, server.TopPlayerName, server.ClientCount);

            var result = "```md\n";

            result += ConsoleTableBuilder.From(scoreTable)
                .WithFormat(ConsoleTableBuilderFormat.Minimal)
                .Export()
                .ToString();

            result += "```";

            await ReplyAsync(result);
        }

        //TODO: Add Help command
        //TODO: Add Invite command
        //TODO: Add Contributors command

    }
}
