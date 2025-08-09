using MongoDB.Driver;
using MyApp.Shared.Data;

using Server.Mongo.Entity;

using MatchType = Shared.Data.MatchType;

namespace Server.Mongo.Collection
{
    public interface IMatchCollection
    {
        Task<Match> CreateMatchAsync(List<string> players,
                                     MatchType matchType,
                                     string url,
                                     int port);
        
        Task<Match> CreateMatchAsync(List<string> players,
                                     MatchType matchType,
                                     string url,
                                     int port,
                                     TimeSpan? duration = null,
                                     MatchLimitType limitType = MatchLimitType.None);
        Task<Match> GetMatchByIdAsync(string matchId);
        Task<Match?> TryJoinOpenMatchAsync(
            MatchType matchType,
            int        maxPlayers,
            string     playerId,
            IClientSessionHandle session);
        Task DeleteMatchAsync(string matchId);
        Task InvalidateMatchAsync(string matchId);
        Task<long> InvalidateAllActiveMatchesAsync();
    }
} 