using System;
using MyApp.Shared.Data;

namespace Shared.Data
{
    public class MatchConfig
    {
        public const string MatchesConfigKey = "MatchesConfig";

        public int MaxPlayers { get; set; }
        public int NumberOfTeams { get; set; }
        public TimeSpan? Duration { get; set; }
        public MatchLimitType LimitType { get; set; } = MatchLimitType.None;
    }
}
