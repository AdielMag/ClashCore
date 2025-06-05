using Server.Mongo.Entity;

namespace Server.Mongo.Collection
{
    public interface IPlayersCollection
    {
        Task<Player> CreatePlayerAsync(string userId, string username);
        Task<Player> GetPlayerByUserIdAsync(string userId);
        Task UpdateLastLoginAsync(string userId);
    }
} 