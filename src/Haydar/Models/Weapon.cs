using LiteDB;

namespace Haydar.Models
{
    public class Weapon: Item
    {
        public float Damage { get; set; }
        public float Distance { get; set; }
        public float AttackAngle { get; set; }
        public float Durability { get; set; }
        public float Dps { get; set; }
    }
}
