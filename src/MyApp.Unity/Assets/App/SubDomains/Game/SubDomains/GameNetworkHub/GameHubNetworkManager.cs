using System;
using System.Threading.Tasks;

using App.InternalDomains.DebugService;
using App.InternalDomains.NetworkService;
using App.SubDomains.Game.SubDomains.PlayersManager;

using Cysharp.Threading.Tasks;

using Shared.Data;
using Shared.Hubs;
using MyApp.Shared.Data;

using Quaternion = System.Numerics.Quaternion;
using Vector3 = System.Numerics.Vector3;

namespace App.SubDomains.Game.SubDomains.GameNetworkHub
{
    public class GameHubNetworkManager : IGameHubNetworkManager, IGameHubReceiver, IDisposable
    {
        private readonly IDebugService _debugService;
        private readonly INetworkService _networkService;
        private readonly IPlayersManager _playersManager;
        
        private IGameHub _client;
        private string _localPlayerId;

        public GameHubNetworkManager(IDebugService debugService,
                                     INetworkService networkService,
                                     IPlayersManager playersManager)
        {
            _debugService = debugService;
            _networkService = networkService;
            _playersManager = playersManager;
        }

        public async UniTask ConnectAsync(string roomName, string playerId)
        {
            _debugService.Log("Connecting to GameHub...");
            
            _client = await _networkService.ConnectHub<IGameHub, IGameHubReceiver>(this);
            
            _debugService.Log("Connected to GameHub successfully.");
            
            _localPlayerId = playerId;
            
            var roomPlayers = await _client.JoinAsync(roomName, playerId, Vector3.Zero, Quaternion.Identity);
            
            _debugService.Log("Joined room: " + roomName);
            foreach (var player in roomPlayers)
            {
                if (player.Id == playerId)
                {
                    continue;
                }
                
                _debugService.Log("Join Player:" + player.Id);
                _playersManager.OnPlayerJoined(player, true);
            }
        }
        
        public UniTask LeaveAsync()
        {
            _playersManager.Dispose();
            return _client.LeaveAsync().AsUniTask();
        }
        
        public UniTask MoveAsync(Vector3 position, Quaternion rotation)
        {
            return _client.MoveAsync(position, rotation).AsUniTask();
        }

        public UniTask TargetChangedAsync(string targetId)
        {
            if (! string.IsNullOrEmpty(targetId))
            {
                return _client.TargetChangedAsync(targetId).AsUniTask();
            }

            _debugService.LogWarning("Target ID is null or empty. Cannot change target.");
            return UniTask.CompletedTask;
        }

        public UniTask WaitForDisconnect()
        {
            return _client.WaitForDisconnect().AsUniTask();
        }

        void IGameHubReceiver.OnJoin(TransformData transformData)
        {
            _debugService.Log("Join Player:" + transformData.Id);

            var isRemote = transformData.Id != _localPlayerId;
            _playersManager.OnPlayerJoined(transformData, isRemote);
        }

        void IGameHubReceiver.OnLeave(TransformData transformData)
        {
            _debugService.Log("Leave Player:" + transformData.Id);

            _playersManager.OnPlayerLeft(transformData.Id);
        }

        void IGameHubReceiver.OnMove(TransformData transformData)
        {
            _playersManager.OnPlayerMoved(transformData.Id, transformData.Position, transformData.Rotation);
        }

        void IGameHubReceiver.OnTargetChanged(string playerId, string targetId)
        {
            _playersManager.OnPlayerTargetChanged(playerId, targetId);
        }

        void IGameHubReceiver.OnMatchExpired(MatchExpirationData expirationData)
        {
            _debugService.Log($"Match expired: {expirationData.MatchId} at {expirationData.ExpirationTime}");
            
            // TODO: Handle match expiration - show UI notification, disconnect players, etc.
            // For now, just log the event with empty data structure
            _debugService.Log($"Expiration type: {expirationData.ExpirationType}");
            _debugService.Log($"Additional data count: {expirationData.Data.Count}");
        }

        public void Dispose()
        {
            try
            {
                _client.DisposeAsync().ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _debugService.LogError(e.ToString());
            }
        }
    }
}