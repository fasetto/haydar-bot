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

        [BsonField("name")]
        public List<string> Descriptions { get; set; }
    }
}
