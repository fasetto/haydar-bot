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
using Haydar.Services;

namespace Haydar.Modules
{
    public class DynastModule : ModuleBase<SocketCommandContext>
    {
        private readonly DynastApi _api;
        private readonly Config _config;
        private readonly PaginationService _paginator;

        public DynastModule(DynastApi api, Config config, PaginationService paginator)
        {
            _api = api;
            _config = config;
            _paginator = paginator;
        }

        [Command("toplist")]
        [Summary("Prints top10 players based on their score")]
        public async Task TopList(string region = null)
        {
            var toplist = await _api.FetchToplistAsync(region);

            if (toplist is null || toplist.Count == 0)
                throw new NullReferenceException("Can't fetch the toplist.");

            var result = Tabularize(toplist);
            await ReplyAsync(result);
        }

        [Command("find")]
        [Summary("To find a specific player in toplist")]
        public async Task FindPlayers(string player)
        {
            var players = await _api.FindPlayersAsync(player);

            if (players is null || players.Count() == 0)
                throw new NullReferenceException("Can't find that player.");

            var result = Tabularize(players);
            await ReplyAsync(result);
        }

        [Command("server")]
        [Summary("Prints informations about selected server")]
        public async Task FindServers(string label, string identifider = null)
        {
            string result;

            if (identifider == null)
            {
                var servers = await _api.FindAllServersAsync(label);

                if (servers is null || servers.Count() == 0)
                    throw new NullReferenceException("Can't find that server.");

                result = Tabularize(servers);
                await ReplyAsync(result);
                return;
            }

            var server = await _api.FindServerAsync(label, identifider);

            if (server is null)
                throw new NullReferenceException("Can't find that server.");

            result = Tabularize(new List<ServerInfo>() { server });
            await ReplyAsync(result);
        }

        [Command("dead")]
        [Summary("Prints servers with less player count")]
        public async Task DeadServers(string region = null)
        {
            var servers = await _api.DeadServersAsync(region);

            if (servers is null || servers.Count() == 0)
                throw new NullReferenceException("Can't find that server.");

            var result = Tabularize(servers);
            await ReplyAsync(result);
        }

        [Command("item")]
        [Summary("To check properties of choosen item")]
        public async Task GetItemInformations(string name)
        {
            var item = _api.Items.Where(i => i.Name.ToLower()
                .Contains(name.ToLower()))
                .FirstOrDefault();

            if (item is null)
                throw new NullReferenceException("Can't find that item.");

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

            embed.WithFooter(footer => footer.Text = "If you didn't find the item you are looking for, just use the itemlist command to see the all items in the game..");
            await ReplyAsync(embed: embed.Build());
        }

        [Command("itemlist")]
        [Summary("Prints the item list currently in game.")]
        [RequireBotPermission(ChannelPermission.ManageMessages | ChannelPermission.AddReactions)]
        public async Task ItemList()
        {
            var items = _api.Items;
            var pageCount = int.Parse(Math.Ceiling(items.Count / 12m).ToString());
            var pages = new List<Page>();

            int counter = 0;
            for (int i = 0; i < pageCount; i++)
            {
                var fields = new List<EmbedFieldBuilder>()
                {
                    new EmbedFieldBuilder()
                    .WithName(":pushpin:")
                    .WithIsInline(true),

                    new EmbedFieldBuilder()
                    .WithName(":pushpin:")
                    .WithIsInline(true),
                };

                string leftCol = "", rightCol = "";

                for (int j = 0; j < 6; j++)
                {
                    try
                    {
                        leftCol += $"{items[counter].Name}\n";
                        rightCol += $"{items[counter + 6].Name}\n";
                        counter++;
                    }
                    catch (ArgumentOutOfRangeException) { break; }
                }

                fields[0] = fields[0].WithValue(leftCol);
                fields[1] = fields[1].WithValue(rightCol);

                pages.Add(new Page() { Fields = fields });
                counter += 6;
            }

            var msg = new PaginatedMessage(pages, ":closed_book: Item List", _config.AuthorBlog, new Color(0x01D484), Context.User);
            await _paginator.SendPaginatedMessageAsync(Context.Channel, msg);
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
