
using System;
using LiteDB;

namespace Haydar.Models
{
    public class Item
    {
        [BsonId]
        public Guid Id { get; set; }

        [BsonField("name")]
        public string Name { get; set; }

        [BsonField("image")]
        public string Image { get; set; }

        [BsonField("description")]
        public string Description { get; set; }
    }
}
