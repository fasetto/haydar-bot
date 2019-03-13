using System;
using System.Collections.Generic;
using LiteDB;

namespace Haydar.Models
{
    public class Contributor
    {
        [BsonId]
        public Guid Id { get; set; }
        public string Name { get; set; }
        public List<string> Descriptions { get; set; }
    }
}
