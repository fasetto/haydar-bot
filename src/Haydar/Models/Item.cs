using Newtonsoft.Json;

namespace Haydar.Models
{
    public class Item
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("image")]
        public string Image { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("damage")]
        public string Damage { get; set; }

        [JsonProperty("distance")]
        public string Distance { get; set; }

        [JsonProperty("attack_angle")]
        public string AttackAngle { get; set; }

        [JsonProperty("durability")]
        public int Durability { get; set; }

        [JsonProperty("dps")]
        public float Dps { get; set; }

    }
}
