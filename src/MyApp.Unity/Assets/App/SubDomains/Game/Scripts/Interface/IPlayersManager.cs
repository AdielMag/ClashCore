using System.Numerics;

using Shared.Data;


namespace App.SubDomains.Game.SubDomains.PlayersManager
{
    public interface IPlayersManager
    {
        void OnPlayerJoined(TransformData transformData, bool isRemote);
        void OnPlayerLeft(string id);
        void OnPlayerMoved(string id, Vector3 position, Quaternion rotation);
        void OnPlayerTargetChanged(string playerId, string targetId);
        void Dispose();
    }
}