

using System.Linq;
using System.Numerics;

using MagicOnion.Server.Hubs;

using Microsoft.Extensions.Logging;

using Server.Common.Extensions;
using Server.Hubs.GamingHub.Data;
using Server.Hubs.GamingHub.Managers.ConnectionManager;
using Server.Hubs.GamingHub.Validators.MovementValidator;
using Server.Mongo.Collection;
using Server.Mongo.Entity;

using Shared.Data;
using Shared.Hubs;

using MyApp.Shared.Data;

namespace Server.Hubs.GamingHub
{
    public class GameHub : StreamingHubBase<IGameHub, IGameHubReceiver>, IGameHub, IDisposable
    {
        private const int _kReconnectTimeoutSeconds = 12;
        
        private readonly ILogger<GameHub> _logger;
        private readonly IConnectionManager _connectionManager;
        private readonly IMovementValidator _movementValidator;
        private readonly IMatchCollection _matchCollection;
        private readonly IMatchInstanceCollection _matchInstanceCollection;
        
        private PlayerConnection _playerConnection;
        private Room _currentRoom;
        private DateTime _lastMoveTime;
        private string _matchId;
        private Timer _matchExpirationTimer;

        public GameHub(ILogger<GameHub> logger, IMatchCollection matchCollection, IMatchInstanceCollection matchInstanceCollection)
        {
            _logger = logger;
            _matchCollection = matchCollection;
            _matchInstanceCollection = matchInstanceCollection;
            _connectionManager = new ConnectionManager(logger, _kReconnectTimeoutSeconds);
            _movementValidator = new MovementValidator();
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
                _currentRoom = new Room(roomName, group, storage);
                
                if (!_currentRoom.Players.TryAdd(id, _playerConnection))
                {
                    _logger.LogWarning($"Player {id} already exists in room {roomName}");
                }
                
                _matchId = roomName;
                
                await StartMatchExpirationMonitoring();
                
                _logger.LogInformation($"Player {id} josned room {roomName}");
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

        private async Task StartMatchExpirationMonitoring()
        {
            try
            {
                if (string.IsNullOrEmpty(_matchId))
                {
                    return;
                }

                // Only the first player in the room should create the timer
                if (_currentRoom.Players.Count > 1)
                {
                    _logger.LogInformation($"Match expiration timer already handled by first player in room {_matchId}, skipping");
                    return;
                }

                // Get match details to check if it has time limits
                var match = await _matchCollection.GetMatchByIdAsync(_matchId);

                // Validate limit type
                match.ValidateLimitType();

                // Check if match has time-based limits
                if (!match.LimitType.HasFlag(MatchLimitType.Time) || !match.Duration.HasValue)
                {
                    _logger.LogInformation($"Match {_matchId} has no time limits, skipping expiration monitoring");
                    return;
                }

                // Calculate time until expiration
                var expirationTime = match.GetExpirationTime();
                if (!expirationTime.HasValue)
                {
                    return;
                }

                var timeUntilExpiration = expirationTime.Value - DateTime.UtcNow;
                if (timeUntilExpiration <= TimeSpan.Zero)
                {
                    // Match is already expired
                    _logger.LogWarning($"Match {_matchId} is already expired");
                    await HandleMatchExpiration(match);
                    return;
                }

                _logger.LogInformation($"Starting match expiration timer for match {_matchId}, expires in {timeUntilExpiration}");
                
                // This instance becomes the timer owner
                _matchExpirationTimer = new Timer(async void (_) =>
                                                  {
                                                      await HandleMatchExpiration(match);
                                                  }, 
                    null, timeUntilExpiration, Timeout.InfiniteTimeSpan);
                
                _logger.LogInformation($"Successfully created expiration timer for match {_matchId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error starting match expiration monitoring for match {_matchId}");
            }
        }

        private async Task HandleMatchExpiration(Match match)
        {
            try
            {
                _logger.LogInformation($"Match {match.Id} has expired, notifying players and invalidating match");

                // Dispose our timer since we're the owner
                await _matchExpirationTimer.DisposeAsync();
                _matchExpirationTimer = null;
                
                // Create expiration data
                var expirationData = new MatchExpirationData
                {
                    MatchId = match.Id,
                    ExpirationType = match.LimitType,
                    ExpirationTime = match.GetExpirationTime() ?? DateTime.UtcNow,
                    Data = new Dictionary<string, object>() // Empty for now, can be populated later
                };

                // Notify all players in the current room
                Broadcast(_currentRoom.Group).OnMatchExpired(expirationData);
                _logger.LogInformation($"Match expiration notification sent to {_currentRoom.Players.Count} players in room {match.Id}");

                CleanupExpiredMatch(match.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error handling match expiration for match {match?.Id}");
            }
        }

        private void CleanupExpiredMatch(string matchId)
        {
            try
            {
                _logger.LogInformation($"Cleaning up expired match {matchId}");
                
                // Clear all players from the current room
                var playerIds = _currentRoom.Players.Keys.ToList();
                foreach (var playerId in playerIds)
                {
                    _currentRoom.Players.TryRemove(playerId, out _);
                }
                
                _logger.LogInformation($"Completed cleanup for expired match {matchId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error during match cleanup for {matchId}");
            }
        }

        public void Dispose()
        {
            _connectionManager.Cleanup(_playerConnection.Id);
            
            _matchExpirationTimer.Dispose();
            _matchExpirationTimer = null;
        }
    }
}