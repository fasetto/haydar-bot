using System;
using Xunit;
using Haydar.Api;
using Haydar.Models;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using System.Net;
using System.IO;
using Newtonsoft.Json;

namespace Haydar.Tests
{
    public class DynastApiTests
    {
        private readonly DynastApi api;
        private readonly Mock<DynastApi> mock;

        public DynastApiTests()
        {
            var config = new Config()
            {
                ApiUrl = "http://announcement-mirror.dynast.io/"
            };

            var data = File.ReadAllText("../../../sample_data.json");
            var sampleData = JsonConvert.DeserializeObject<ServerObj>(data).ServerInformations;

            mock = new Mock<DynastApi>(config);
            mock.Setup(x => x.DeserializeServerInformations(It.IsAny<string>()))
                .Returns(sampleData);

            api = mock.Object;
        }

        [Fact]
        public async void FetchToplistAsync_ReturnsRightInformations()
        {
            var toplist = await api.FetchToplistAsync();
            var first = toplist.FirstOrDefault();
            var last = toplist.LastOrDefault();

            Assert.NotEmpty(toplist);
            Assert.Equal(10, toplist.Count);
            Assert.True(first.TopPlayerScore > last.TopPlayerScore);
        }

        [Fact]
        public async void FetchToplistAsync_WithRegion_ReturnsRightInformations()
        {
            var region = "eu";
            var toplist = await api.FetchToplistAsync(region);

            Assert.NotEmpty(toplist);
            Assert.Equal(10, toplist.Count);
            Assert.Equal(10, toplist.Where(x => x.Region.ToLower().StartsWith(region)).Count());
        }

        [Fact]
        public async void FindPlayersAsync_SearchString_CanFindPlayers()
        {
            var player = "serkan";
            var result = await api.FindPlayersAsync(player);

            Assert.Equal(1, result.Count);
        }

        [Fact]
        public async void DeadServersAsync_ReturnsRightInformations()
        {
            var deadServers = await api.DeadServersAsync();
            var first = deadServers.FirstOrDefault();
            var last = deadServers.LastOrDefault();

            Assert.Equal(10, deadServers.Count);
            Assert.True(first.ClientCount < last.ClientCount);
        }

        [Fact]
        public async void DeadServersAsync_WithRegion_ReturnsRightInformations()
        {
            var region = "eu";
            var deadServers = await api.DeadServersAsync(region);
            var first = deadServers.FirstOrDefault();
            var last = deadServers.LastOrDefault();

            Assert.Equal(10, deadServers.Count);
            Assert.True(first.ClientCount < last.ClientCount);
            Assert.Equal(10, deadServers.Where(x => x.Region.ToLower().StartsWith(region)).Count());
        }

        [Theory]
        [InlineData("amsterdam-03x", "ams", "3x")]
        [InlineData("amsterdam-03", "ams", "03")]
        [InlineData("amsterdam-07x", "ams", "7x")]
        [InlineData("amsterdam-01x", "ams", "1x")]
        [InlineData("amsterdam-01", "ams", "1")]
        public async void FindServerAsync_CanFindServer(string expected, string label, string identifer)
        {
            var server = await api.FindServerAsync(label, identifer);
            Assert.Equal(expected, server.Label);
        }

        [Theory]
        [InlineData("amsterdam", "ams")]
        [InlineData("frankfurt", "fra")]
        [InlineData("sydney", "syd")]
        [InlineData("russia", "ru")]
        public async void FindAllServersAsync_CandFindAllServers(string expected, string label)
        {
            var servers = await api.FindAllServersAsync(label);
            servers.ForEach(x => Assert.Equal(expected, x.Label.Split('-')[0]));
        }

        [Fact]
        public void Items_LoadedCorrectly()
        {
            var items = api.Items;
            var pan = items.Where(x => x.Name == "frying pane").FirstOrDefault();

            Assert.Equal(59, items.Count);
            Assert.NotNull(pan);
        }
    }
}
