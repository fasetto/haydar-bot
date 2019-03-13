
using System;
using LiteDB;

namespace Haydar.Models
{
    public class Item
    {
        [BsonId]
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Image { get; set; }
        public string Description { get; set; }
    }
}
