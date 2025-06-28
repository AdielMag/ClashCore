using System.Numerics;
using System.Threading.Tasks;

using MagicOnion;

using Shared.Data;

namespace Shared.Hubs
{
    public interface IGameHub : IStreamingHub<IGameHub, IGameHubReceiver>
    {
        ValueTask<TransformData[]> JoinAsync(string roomName, string id, Vector3 position, Quaternion rotation);
        ValueTask LeaveAsync();
        ValueTask MoveAsync(Vector3 position, Quaternion rotation);
        ValueTask TargetChangedAsync(string targetId);
    }
}