using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Server.Mongo.Entity
{
    public class Player
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("userId")]
        public string UserId { get; set; }
        
        [BsonElement("username")]
        public string Username { get; set; }
        
        [BsonElement("lastLoginAt")]
        public DateTime LastLoginAt { get; set; }

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; }

        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; }
    }
}