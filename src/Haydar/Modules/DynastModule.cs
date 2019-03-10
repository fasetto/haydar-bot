using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using ConsoleTableExt;
using Discord;
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

        [Command("dead"), Summary("Prints servers with less player count")]
        public async Task DeadServers(string region = null)
        {
            var servers = await _api.DeadServersAsync(region);
            var result = Tabularize(servers);
            await ReplyAsync(result);
        }


        [Command("item"), Summary("To check properties of choosen item")]
        public async Task GetItemInformations(string name)
        {
            var item = _api.Items.Where(i => i.Name.ToLower()
                .Contains(name.ToLower()))
                .FirstOrDefault();

            var embed = new EmbedBuilder()
            {
                Title        = $":bulb: {item.Name}",
                Description  = item.Description,
                Color        = new Color(0xDDB979),
                ThumbnailUrl = item.Image,
                Url          = _config.AuthorBlog,
            };

            if (item.Damage.Length > 0)
            {
                const decimal DAMAGE_INC = 0.02m;
                decimal damageAt10 = decimal.Parse(item.Damage) * Convert.ToDecimal(1.0m + DAMAGE_INC * 10m);
                decimal damageAt20 = decimal.Parse(item.Damage) * Convert.ToDecimal(1.0m + DAMAGE_INC * 20m);
                decimal damageAt30 = decimal.Parse(item.Damage) * Convert.ToDecimal(1.0m + DAMAGE_INC * 30m);
                decimal damageAt40 = decimal.Parse(item.Damage) * Convert.ToDecimal(1.0m + DAMAGE_INC * 40m);
                decimal damageAt50 = decimal.Parse(item.Damage) * Convert.ToDecimal(1.0m + DAMAGE_INC * 50m);

                embed.AddField("__**Damage**__", $@"
                    **multiplier** `{item.Damage}`
                    **dps** `{item.Dps?.ToString("0.00")}`
                    **level10** `{damageAt10.ToString("0.00")}`
                    **level20** `{damageAt20.ToString("0.00")}`
                    **level30** `{damageAt30.ToString("0.00")}`
                    **level40** `{damageAt40.ToString("0.00")}`
                    **level50** `{damageAt50.ToString("0.00")}`
                ", true);

                embed.AddField("__**Other Informations**__", $@"
                    **durability** `{item.Durability}`
                    **attack angle** `{item.AttackAngle}`
                    **attack distance** `{item.Distance}`
                ", true);
            }

            embed.WithFooter(footer => footer.Text = "If you didn\'t find the item you are looking for, just use the itemlist command to see the all items in the game..");
            await ReplyAsync(embed: embed.Build());
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
