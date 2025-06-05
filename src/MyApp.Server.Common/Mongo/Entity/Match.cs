using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Server.Mongo.Entity
{
    public class Match
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public List<string> Players { get; set; }
        public int PlayerCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public Shared.Data.MatchType Type { get; set; }
        public string Url { get; set; }
        public int Port { get; set; }
    }
} 