using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Haydar.Models;

[assembly: InternalsVisibleTo("Haydar.Tests"), InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace Haydar.Api
{
    public class DynastApi
    {
        private readonly Config _config;

        public DynastApi(Config config)
        {
            _config = config;
        }

        public async Task<List<ServerInfo>> FetchToplistAsync(string region = null)
        {
            List<ServerInfo> toplist;

            if (region != null)
                toplist = await FetchServerInformationsAsync(x => x.Region.ToLower().StartsWith(region.ToLower()));

            else
                toplist = await FetchServerInformationsAsync();

            return toplist.OrderByDescending(player => player.TopPlayerScore).Take(10).ToList();
        }

        private Task<List<ServerInfo>> FetchServerInformationsAsync(Func<ServerInfo, bool> predicate = null)
        {
            string data;

            using (var client = new WebClient())
            {
                client.Headers.Add("User-Agent", "Haydar");
                data = client.DownloadString(_config.ApiUrl);
            }

            var serverInformations = DeserializeServerInformations(data);

            if (predicate != null)
                serverInformations = serverInformations.Where(predicate).ToList();

            serverInformations = serverInformations.ToList();

            return Task.FromResult(serverInformations);
        }

        internal virtual List<ServerInfo> DeserializeServerInformations(string data)
        {
            var serverInformations = JsonConvert.DeserializeObject<ServerObj>(data).ServerInformations;
            return serverInformations;
        }

        public async Task<List<ServerInfo>> FindAsync(string player)
        {
            var players = await FetchServerInformationsAsync(x => x.TopPlayerName.ToLower().Contains(player.ToLower()));

            return players.Take(3).ToList();
        }

        public async Task<List<ServerInfo>> DeadAsync(string region = null)
        {
            List<ServerInfo> deadServers;

            if (region != null)
                deadServers = await FetchServerInformationsAsync(x => x.Region.ToLower().StartsWith(region.ToLower()));

            else
                deadServers = await FetchServerInformationsAsync();

            return deadServers.OrderBy(x => x.ClientCount).Take(10).ToList();
        }

        public async Task<ServerInfo> ServerAsync(string label)
        {
            if (label.Split(' ').Length < 2)
                return null;

            string name = label.Split(' ')[0];
            string identifier = label.Split(' ')[1];

            identifier = identifier[0] == '0' ? identifier : identifier.Insert(0, "0");

            var servers = await FetchServerInformationsAsync(x => x.Label.Split('-')[0].ToLower().Contains(name.ToLower()) && x.Label.Split('-')[1] == identifier.ToLower());
            return servers.FirstOrDefault();
        }

        //TODO: Add Item command
        //TODO: Add ItemList command
    }
}
