using System.Collections.Concurrent;

using Microsoft.Extensions.Logging;

namespace Server.Hubs.GamingHub.Managers.ConnectionManager
{
    public class ConnectionManager : IConnectionManager
    {
        private static readonly ConcurrentDictionary<string, CancellationTokenSource> _disconnectionTimers = new();
        private readonly ILogger _logger;
        private readonly int _reconnectTimeoutSeconds;

        public ConnectionManager(ILogger logger, int reconnectTimeoutSeconds)
        {
            _logger = logger;
            _reconnectTimeoutSeconds = reconnectTimeoutSeconds;
        }

        public void CancelDisconnectionTimer(string playerId)
        {
            if (! _disconnectionTimers.TryRemove(playerId, out var timer))
            {
                return;
            }

            try
            {
                timer.Cancel();
                timer.Dispose();
                _logger.LogInformation($"Cancelled disconnection timer for player {playerId}");
            }
            catch (ObjectDisposedException)
            {
                _logger.LogWarning($"Timer was already disposed for player {playerId}");
            }
        }

        public async Task StartDisconnectionTimer(string playerId, Func<Task> onTimeout)
        {
            var cts = new CancellationTokenSource();
            
            if (!_disconnectionTimers.TryAdd(playerId, cts))
            {
                cts.Dispose();
                return;
            }

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(_reconnectTimeoutSeconds), cts.Token);
                
                if (_disconnectionTimers.TryRemove(playerId, out cts))
                {
                    cts.Dispose();
                    await onTimeout();
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation($"Reconnection timer cancelled for player {playerId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in disconnection timer for player {playerId}");
            }
        }

        public void Leave()
        {
            
        }

        public void Cleanup(string playerId)
        {
            CancelDisconnectionTimer(playerId);
        }
    }
}