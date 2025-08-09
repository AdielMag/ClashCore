using System;
using System.Collections.Generic;
using MessagePack;

namespace MyApp.Shared.Data
{
    [MessagePackObject]
    public class MatchExpirationData
    {
        [Key(0)]
        public string MatchId { get; set; } = string.Empty;
        
        [Key(1)]
        public MatchLimitType ExpirationType { get; set; }
        
        [Key(2)]
        public DateTime ExpirationTime { get; set; }
        
        [Key(3)]
        public Dictionary<string, object> Data { get; set; } = new Dictionary<string, object>();
    }
}