using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Haydar.Models;
using Newtonsoft.Json;

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

            return toplist.Take(10).ToList();
        }

        private Task<List<ServerInfo>> FetchServerInformationsAsync(Func<ServerInfo, bool> predicate = null)
        {
            string result;

            using (var client = new WebClient())
            {
                client.Headers.Add("User-Agent", "Haydar");
                result = client.DownloadString(_config.ApiUrl);
            }

            var serverInformations = JsonConvert.DeserializeObject<ServerObj>(result).ServerInformations;

            if (predicate != null)
                serverInformations = serverInformations.Where(predicate).ToList();

            serverInformations = serverInformations
                .OrderByDescending(player => player.TopPlayerScore)
                .ToList();

            return Task.FromResult(serverInformations);
        }
    }
}
