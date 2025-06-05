using Cysharp.Threading.Tasks;

using Quaternion = System.Numerics.Quaternion;
using Vector3 = System.Numerics.Vector3;

namespace App.SubDomains.Game.SubDomains.GameNetworkHub
{
    public interface IGameHubNetworkManager
    {
        UniTask ConnectAsync(string roomName, string playerId);
        UniTask LeaveAsync();
        UniTask MoveAsync(Vector3 position, Quaternion rotation);
        UniTask WaitForDisconnect();
    }
}