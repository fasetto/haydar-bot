using System.Collections.Generic;
using Newtonsoft.Json;

namespace Haydar.Models
{
    public class ServerObj
    {
        [JsonProperty("status")]
        public bool Status { get; set; }

        [JsonProperty("servers")]
        public List<ServerInfo> ServerInformations { get; set; }
    }

    public class ServerInfo
    {
        [JsonProperty("client_count")]
        public long ClientCount { get; set; }

        [JsonProperty("top_player_name")]
        public string TopPlayerName { get; set; }

        [JsonProperty("top_player_score")]
        public long TopPlayerScore { get; set; }

        [JsonProperty("top_player_level")]
        public long TopPlayerLevel { get; set; }

        [JsonProperty("region")]
        public string Region { get; set; }

        [JsonProperty("label")]
        public string Label { get; set; }

        [JsonProperty("lifetime")]
        public long Lifetime { get; set; }
    }
}
