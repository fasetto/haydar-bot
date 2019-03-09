using System.Collections.Generic;
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
            var result = Tabularize(toplist);
            await ReplyAsync(result);
        }


        [Command("find"), Summary("To find a specific player in toplist")]
        public async Task FindPlayers(string player)
        {
            var players = await _api.FindPlayersAsync(player);
            var result = Tabularize(players);
            await ReplyAsync(result);
        }

        [Command("server"), Summary("Prints informations about selected server")]
        public async Task FindServers(string label, string identifider = null)
        {
            string result;

            if (identifider == null)
            {
                var servers = await _api.FindAllServersAsync(label);
                result = Tabularize(servers);
                await ReplyAsync(result);
                return;
            }

            var server = await _api.FindServerAsync(label, identifider);
            result = Tabularize(new List<ServerInfo>() { server });
            await ReplyAsync(result);
        }

        private string Tabularize(List<ServerInfo> serverList)
        {
            var scoreTable = new DataTable();
            scoreTable.Columns.Add("# SERVER", typeof(string));
            scoreTable.Columns.Add("SCORE", typeof(string));
            scoreTable.Columns.Add("LEVEL", typeof(string));
            scoreTable.Columns.Add("NICKNAME", typeof(string));
            scoreTable.Columns.Add("PLAYER COUNT", typeof(string));

            foreach (var server in serverList)
                scoreTable.Rows.Add(server.Label, server.TopPlayerScore, server.TopPlayerLevel, server.TopPlayerName, server.ClientCount);

            var result = "```md\n";
            result += ConsoleTableBuilder.From(scoreTable)
                .WithFormat(ConsoleTableBuilderFormat.Minimal)
                .Export()
                .ToString();

            result += "```";
            return result;
        }

        //TODO: Add Help command
        //TODO: Add Invite command
        //TODO: Add Contributors command

    }
}
