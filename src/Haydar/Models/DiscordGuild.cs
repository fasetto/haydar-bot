using System;
using LiteDB;

namespace Haydar.Models
{
    public class DiscordGuild
    {
        [BsonId]
        public ulong Id { get; set; }
        public string Prefix { get; set; }
    }
}
