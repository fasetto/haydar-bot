using System;
using System.Collections.Generic;
using LiteDB;

namespace Haydar.Models
{
    public class Contributor
    {
        [BsonId]
        public Guid Id { get; set; }

        [BsonField("name")]
        public string Name { get; set; }

        [BsonField("description")]
        public string Description { get; set; }
    }
}
