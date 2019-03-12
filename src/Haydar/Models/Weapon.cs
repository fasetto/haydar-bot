using LiteDB;

namespace Haydar.Models
{
    public class Weapon: Item
    {
        [BsonField("damage")]
        public float Damage { get; set; }

        [BsonField("distance")]
        public float Distance { get; set; }

        [BsonField("attack_angle")]
        public float AttackAngle { get; set; }

        [BsonField("durability")]
        public float Durability { get; set; }

        [BsonField("dps")]
        public float Dps { get; set; }
    }
}
