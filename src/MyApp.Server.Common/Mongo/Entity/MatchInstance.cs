using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Server.Mongo.Entity
{
    public class MatchInstance
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string Url { get; set; } = string.Empty;
        public int Port { get; set; }
        public int PlayerCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
