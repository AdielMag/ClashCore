

using System.Numerics;

using MagicOnion.Server.Hubs;

using Microsoft.Extensions.Logging;

using Server.Hubs.GamingHub.Data;
using Server.Hubs.GamingHub.Managers.ConnectionManager;
using Server.Hubs.GamingHub.Managers.RoomManager;
using Server.Hubs.GamingHub.Validators.MovementValidator;

using Shared.Data;
using Shared.Hubs;

namespace Server.Hubs.GamingHub
{
    public class GameHub : StreamingHubBase<IGameHub, IGameHubReceiver>, IGameHub, IDisposable
    {
        private const int _kReconnectTimeoutSeconds = 12;
        
        private readonly ILogger<GameHub> _logger;
        private readonly IConnectionManager _connectionManager;
        private readonly IMovementValidator _movementValidator;
        private readonly IRoomManager _roomManager;
        
        private PlayerConnection _playerConnection;
        private Room _currentRoom;
        private DateTime _lastMoveTime;

        public GameHub(ILogger<GameHub> logger)
        {
            _logger = logger;
            _connectionManager = new ConnectionManager(logger, _kReconnectTimeoutSeconds);
            _movementValidator = new MovementValidator();
            _roomManager = new RoomManager(logger);
            _lastMoveTime = DateTime.UtcNow;
        }

        public async ValueTask<TransformData[]> JoinAsync(string roomName, string id, Vector3 position, Quaternion rotation)
        {
            try
            {
                var transformData = new TransformData { Id = id, Position = position, Rotation = rotation };
                _playerConnection = new PlayerConnection(id, ConnectionId.ToString(), transformData);

                _connectionManager.CancelDisconnectionTimer(id);

                var (group, storage) = await Group.AddAsync(roomName, transformData);
                _currentRoom = await _roomManager.GetOrCreateRoom(roomName, group, storage);
                
                if (!_currentRoom.Players.TryAdd(id, _playerConnection))
                {
                    _logger.LogWarning($"Player {id} already exists in room {roomName}");
                }
                
                _logger.LogInformation($"Player {id} joined room {roomName}");
                Broadcast(_currentRoom.Group).OnJoin(transformData);

                return _currentRoom.Storage.AllValues.ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error during JoinAsync for player {id} in room {roomName}");
                throw;
            }
        }

        public async ValueTask LeaveAsync()
        {
            await HandlePlayerTimeout();
        }

        public ValueTask MoveAsync(Vector3 position, Quaternion rotation)
        {
            try
            {
                var deltaTime = (float)(DateTime.UtcNow - _lastMoveTime).TotalSeconds;

                var validationResult = _movementValidator.ValidateMovement(
                    _playerConnection.TransformData.Position, position, deltaTime);

                if (! validationResult.IsValid)
                {
                    Broadcast(_currentRoom.Group).OnMove(_playerConnection.TransformData);

                    _logger.LogWarning(
                        $"Invalid movement detected for player {_playerConnection.Id}: {validationResult.ErrorMessage}");

                    return ValueTask.CompletedTask;
                }

                _playerConnection.UpdateTransform(position, rotation);

                BroadcastExcept(_currentRoom.Group, ConnectionId).OnMove(_playerConnection.TransformData);

                _lastMoveTime = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error during MoveAsync for player {_playerConnection?.Id}");
                throw;
            }
            
            return ValueTask.CompletedTask;
        }
        

        public ValueTask TargetChangedAsync(string targetId)
        {
            try
            {
                var playerId = _playerConnection.Id;
                Broadcast(_currentRoom.Group).OnTargetChanged(playerId, targetId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error during TargetChangedAsync for player {_playerConnection?.Id}");
                throw;
            }
            return ValueTask.CompletedTask;
        }

        protected override async ValueTask OnDisconnected()
        {
            try
            {
                _logger.LogInformation($"Client disconnected. Starting reconnection timer for player {_playerConnection.Id}");
                await _connectionManager.StartDisconnectionTimer(_playerConnection.Id, HandlePlayerTimeout);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error during OnDisconnected for player {_playerConnection.Id}");
            }
        }

        private async Task HandlePlayerTimeout()
        {
            
            try
            {
                await _currentRoom.Group.RemoveAsync(Context);
                _connectionManager.CancelDisconnectionTimer(_playerConnection.Id);
                _currentRoom.Players.TryRemove(_playerConnection.Id, out _);
                Broadcast(_currentRoom.Group).OnLeave(_playerConnection.TransformData);
                
                _logger.LogInformation($"Player {_playerConnection.Id} removed due to reconnection timeout");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error removing timed-out player {_playerConnection.Id} from room");
            }
        }

        public void Dispose()
        {
            _connectionManager.Cleanup(_playerConnection.Id);
        }
    }
}