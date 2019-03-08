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
    }
}
