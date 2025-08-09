using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MyApp.Shared.Data;

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
        public bool IsValid { get; set; } = true;
        
        // Time-based limits
        public TimeSpan? Duration { get; set; }
        public MatchLimitType LimitType { get; set; } = MatchLimitType.None;
    }
} 