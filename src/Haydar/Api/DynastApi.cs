using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Haydar.Models;
using System.IO;
using LiteDB;

[assembly: InternalsVisibleTo("Haydar.Tests"), InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace Haydar.Api
{
    public class DynastApi
    {
        private readonly Config _config;
        private readonly LiteRepository _repository;

        public DynastApi(Config config, LiteRepository repository)
        {
            _config = config;
            _repository = repository;
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

        public async Task<List<ServerInfo>> FindPlayersAsync(string player)
        {
            var players = await FetchServerInformationsAsync(x => x.TopPlayerName.ToLower().Contains(player.ToLower()));

            return players.Take(3).ToList();
        }

        public async Task<List<ServerInfo>> DeadServersAsync(string region = null)
        {
            List<ServerInfo> deadServers;

            if (region != null)
                deadServers = await FetchServerInformationsAsync(x => x.Region.ToLower().StartsWith(region.ToLower()));

            else
                deadServers = await FetchServerInformationsAsync();

            return deadServers.OrderBy(x => x.ClientCount).Take(10).ToList();
        }

        public async Task<ServerInfo> FindServerAsync(string label, string identifier)
        {
            identifier = identifier[0] == '0' ? identifier : identifier.Insert(0, "0");

            var servers = await FetchServerInformationsAsync(x =>
            {
                var values = x.Label.Split('-');

                if (values.Length > 1)
                    return x.Label.Split('-')[0].ToLower().Contains(label.ToLower()) && x.Label.Split('-')[1] == identifier.ToLower();

                return false;
            });

            return servers.FirstOrDefault();
        }

        public async Task<List<ServerInfo>> FindAllServersAsync(string label)
        {
            var servers = await FetchServerInformationsAsync(x => x.Label.Split('-')[0].ToLower().Contains(label.ToLower()));
            return servers.OrderByDescending(x => x.TopPlayerScore).Take(10).ToList();
        }

        public async Task<List<Item>> GetItemList()
        {
            var items   = new List<Item>();
            var weapons = _repository.Query<Weapon>().ToList();
            var hats    = _repository.Query<Hat>().ToList();

            items.AddRange(weapons);
            items.AddRange(hats);

            return await Task.FromResult(items);
        }

        public Task<Item> GetItem(string name)
        {
            var weapon = _repository.Query<Weapon>()
                .Where(x => x.Name.ToLower().Contains(name.ToLower()))
                .FirstOrDefault();

            if (weapon != null)
                return Task.FromResult<Item>(weapon);

            var hat = _repository.Query<Hat>()
                .Where(x => x.Name.ToLower().Contains(name.ToLower()))
                .FirstOrDefault();

            return Task.FromResult<Item>(hat);
        }
    }
}
