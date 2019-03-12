using System.Collections.Generic;
using Newtonsoft.Json;

namespace Haydar.Models
{
    public class Config
    {
        [JsonProperty("debug")]
        public bool Debug { get; set; }

        [JsonProperty("api")]
        public string ApiUrl { get; set; }

        [JsonProperty("author_blog")]
        public string AuthorBlog { get; set; }

        [JsonProperty("bot")]
        public BotConfig BotConfig { get; set; }

        [JsonProperty("contributors")]
        public List<Contributor> Contributors { get; set; }
    }

    public class BotConfig
    {
        [JsonProperty("token")]
        public string Token { get; set; }

        [JsonProperty("token_beta")]
        public string TokenBeta { get; set; }

        [JsonProperty("prefix")]
        public string Prefix { get; set; }

        [JsonProperty("game_status")]
        public string GameStatus { get; set; }

        [JsonProperty("owner_id")]
        public ulong OwnerId { get; set; }

        [JsonProperty("invite")]
        public string InviteUrl { get; set; }

        [JsonProperty("invite_beta")]
        public string InviteUrlBeta { get; set; }

    }
}
