using System;
using Xunit;
using Haydar.Api;
using Haydar.Models;
using System.Linq;

namespace Haydar.Tests
{
    public class ApiTests
    {
        private readonly DynastApi _api;

        public ApiTests()
        {
            var config = new Config()
            {
                ApiUrl = "http://announcement-mirror.dynast.io/"
            };

            _api = new DynastApi(config);
        }

        [Fact]
        public async void DynastApi_FetchToplist()
        {
            var toplist = await _api.FetchToplistAsync();
            var first = toplist.FirstOrDefault();
            var last = toplist.LastOrDefault();

            Assert.NotEmpty(toplist);
            Assert.Equal(10, toplist.Count);
            Assert.True(first.TopPlayerScore > last.TopPlayerScore);
        }

        [Fact]
        public async void DynastApi_FetchToplist_WithRegion()
        {
            var region = "eu";
            var toplist = await _api.FetchToplistAsync(region);

            Assert.NotEmpty(toplist);
            Assert.Equal(10, toplist.Count);
            Assert.Equal(10, toplist.Where(x => x.Region.ToLower().StartsWith(region)).Count());
        }
    }
}
