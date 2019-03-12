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
    [Name("Dynast Module")]
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
        public async Task FindPlayers([Remainder] string player)
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
        public async Task GetItemInformations([Remainder] string name)
        {
            var item = await _api.GetItem(name);

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

            if (item.GetType() == typeof(Weapon))
                IncludeWeaponStats((Weapon) item, embed);

            embed.WithFooter(footer => footer.Text = "If you didn't find the item you are looking for, just use the itemlist command to see the all items in the game..");
            await ReplyAsync(embed: embed.Build());
        }

        [Command("itemlist")]
        [Summary("Prints the item list currently in game.")]
        [RequireBotPermission(ChannelPermission.ManageMessages | ChannelPermission.AddReactions)]
        public async Task ItemList()
        {
            var items = await _api.GetItemList();
            var pageCount = int.Parse(Math.Ceiling(items.Count / 12m).ToString());
            var pages = new List<Page>();

            int counter = 0;
            for (int i = 0; i < pageCount; i++)
            {
                var fields = new List<EmbedFieldBuilder>()
                {
                    new EmbedFieldBuilder()
                    {
                        Name = ":pushpin:",
                        IsInline = true
                    },
                    new EmbedFieldBuilder()
                    {
                        Name = ":pushpin:",
                        IsInline = true
                    },
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

                fields[0].Value = leftCol;
                fields[1].Value = rightCol;

                pages.Add(new Page() { Fields = fields });
                counter += 6;
            }

            var msg = new PaginatedMessage(pages, ":closed_book: Item List", _config.AuthorBlog, new Color(0x01D484), Context.User);
            await _paginator.SendPaginatedMessageAsync(Context.Channel, msg);
        }


        [Command("invite")]
        [Summary("Prints the invite code of bot.")]
        public async Task Invite(string beta = null)
        {
            var invite = "https://kutt.it/haydar";
            await ReplyAsync(invite);
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

        private void IncludeWeaponStats(Weapon weapon, EmbedBuilder embed)
        {
            const float DAMAGE_INC = 0.02f;
            float damageAt10 = weapon.Damage * 1.0f + DAMAGE_INC * 10f;
            float damageAt20 = weapon.Damage * 1.0f + DAMAGE_INC * 20f;
            float damageAt30 = weapon.Damage * 1.0f + DAMAGE_INC * 30f;
            float damageAt40 = weapon.Damage * 1.0f + DAMAGE_INC * 40f;
            float damageAt50 = weapon.Damage * 1.0f + DAMAGE_INC * 50f;

            var damageInfos  = "";
                damageInfos += $"**multiplier** `{weapon.Damage}`\n";
                damageInfos += $"**dps** `{weapon.Dps.ToString("0.00")}`\n";
                damageInfos += $"**level10** `{damageAt10.ToString("0.00")}`\n";
                damageInfos += $"**level20** `{damageAt20.ToString("0.00")}`\n";
                damageInfos += $"**level30** `{damageAt30.ToString("0.00")}`\n";
                damageInfos += $"**level40** `{damageAt40.ToString("0.00")}`\n";
                damageInfos += $"**level50** `{damageAt50.ToString("0.00")}`";

            embed.AddField(f =>
            {
                f.Name     = "__**Damage**__";
                f.Value    = damageInfos;
                f.IsInline = true;
            });

            var otherInfos  = "";
                otherInfos += $"**durability** `{weapon.Durability}`\n";
                otherInfos += $"**attack angle** `{weapon.AttackAngle}`\n";
                otherInfos += $"**attack distance** `{weapon.Distance}`";

            embed.AddField(f =>
            {
                f.Name     = "__**Other Informations**__";
                f.Value    = otherInfos;
                f.IsInline = true;
            });
        }

        //TODO: Add Contributors command
        //TODO: Add Market/Trade system

    }
}
