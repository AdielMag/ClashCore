using MyApp.Shared.Data;
using Server.Mongo.Entity;

namespace Server.Common.Extensions
{
    public static class MatchExtensions
    {
        public static void ValidateLimitType(this Match match)
        {
            if (match.LimitType != MatchLimitType.None && match.LimitType != MatchLimitType.Time)
            {
                throw new NotImplementedException("Only time-based limits are currently supported");
            }
        }
        
        public static bool IsExpired(this Match match)
        {
            if (match.LimitType.HasFlag(MatchLimitType.Time) && match.Duration.HasValue)
            {
                return DateTime.UtcNow > match.CreatedAt.Add(match.Duration.Value);
            }
            return false;
        }
        
        public static DateTime? GetExpirationTime(this Match match)
        {
            if (match.LimitType.HasFlag(MatchLimitType.Time) && match.Duration.HasValue)
            {
                return match.CreatedAt.Add(match.Duration.Value);
            }
            return null;
        }
    }
}