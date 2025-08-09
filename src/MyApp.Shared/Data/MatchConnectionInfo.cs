using MessagePack;
using System;

namespace Shared.Data
{
    [MessagePackObject]
    public class MatchConnectionData
    {
        [Key(0)] public string Url;
        [Key(1)] public int Port;
        [Key(2)] public string MatchId;
        [Key(3)] public DateTime? ExpirationTimeUtc;
    }
}