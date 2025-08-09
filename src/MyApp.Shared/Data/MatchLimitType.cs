using System;

namespace MyApp.Shared.Data
{
    [Flags]
    public enum MatchLimitType
    {
        None = 0,
        Time = 1,
        Condition = 2,  // Reserved for future implementation
        Custom = 4      // Reserved for future implementation
    }
}