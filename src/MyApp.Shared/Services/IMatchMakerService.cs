using MagicOnion;
using Shared.Data;

namespace Shared.Services
{
    public interface IMatchMakerService : IService<IMatchMakerService>
    {
        UnaryResult<string> Test();
        
        UnaryResult<MatchConnectionData> JoinMatchAsync(string playerId,
                                                        Data.MatchType matchType,
                                                        string matchId);
    }
}