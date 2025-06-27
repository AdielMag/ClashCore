using System;
using System.Threading.Tasks;
using App.InternalDomains.DebugService;
using App.InternalDomains.NetworkService;

using Cysharp.Threading.Tasks;

using UnityEngine;
using VContainer;

namespace App.InternalDomains.PlayersService
{
    public class PlayersService : IPlayersService,
                                  IPlayerIdProvider
    {
        private const string _kUserIdKey = "Player_UserId";
        private const int _kMaxRetryAttempts = 3;
        private const float _kRetryDelaySeconds = 1.0f;

        public string PlayerId
        {
            get;
            private set;
        }
        
        private readonly INetworkService _networkService;
        private readonly IDebugService _debugService;
        
        private Shared.Services.IPlayersService _playersService;
        
        public PlayersService(INetworkService networkService, IDebugService debugService)
        {
            _networkService = networkService;
            _debugService = debugService;
        }

        public async UniTask Login()
        {
            _playersService ??= await _networkService.GetService<Shared.Services.IPlayersService>();

            try
            {
                var userId = PlayerPrefs.GetString(_kUserIdKey, string.Empty);

                if (string.IsNullOrEmpty(userId))
                {
                    _debugService.Log("No existing userId found. Starting registration process...");
                    userId = await RetryAsync(RegisterNewPlayer, _kMaxRetryAttempts, _kRetryDelaySeconds);
                }
                else
                {
                    _debugService.Log($"Found existing userId: {userId}. Attempting to login...");
                    await RetryAsync(() => LoginExistingPlayer(userId), _kMaxRetryAttempts, _kRetryDelaySeconds);
                }

                PlayerId = userId;
            }
            catch (Exception ex)
            {
                _debugService.LogError($"Error during login process: {ex.Message}");
                throw;
            }

            _debugService.Log($"Login process completed. PlayerId: {PlayerId}");
        }

        private async Task<string> RegisterNewPlayer()
        {
            _debugService.Log("Registering new player...");
            var newUserId = await _playersService.RegisterAsync();

            if (string.IsNullOrEmpty(newUserId))
            {
                _debugService.LogError("Registration failed: Received empty userId");
                throw new Exception("Registration failed: Empty userId received");
            }

            _debugService.Log($"Registration successful. New userId: {newUserId}");
            SaveUserId(newUserId);
            await RetryAsync(() => LoginExistingPlayer(newUserId), _kMaxRetryAttempts, _kRetryDelaySeconds);
            return newUserId;
        }

        private async Task LoginExistingPlayer(string userId)
        {
            _debugService.Log($"Attempting to login with userId: {userId}");
            try
            {
                await _playersService.LoginAsync(userId);
            }
            catch (Exception ex)
            {
                _debugService.LogError($"Login failed for userId {userId}: {ex.Message}");
                if (ex.Message.Contains("Player not found"))
                {
                    _debugService.LogWarning("Player not found. Clearing stored userId and retrying with registration...");
                    ClearUserId();
                }
                throw;
            }
        }

        private void SaveUserId(string userId)
        {
            PlayerPrefs.SetString(_kUserIdKey, userId);
            PlayerPrefs.Save();
            _debugService.Log("UserId saved to PlayerPrefs");
        }

        private void ClearUserId()
        {
            PlayerPrefs.DeleteKey(_kUserIdKey);
            PlayerPrefs.Save();
            _debugService.Log("UserId cleared from PlayerPrefs");
        }

        private async Task<T> RetryAsync<T>(Func<Task<T>> action, int maxAttempts, float delaySeconds)
        {
            var attempt = 0;
            Exception lastException = null;
            while (attempt < maxAttempts)
            {
                try
                {
                    return await action();
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    attempt++;
                    if (attempt < maxAttempts)
                    {
                        _debugService.LogWarning($"Retrying ({attempt}/{maxAttempts}) after error: {ex.Message}");
                        await UniTask.Delay(TimeSpan.FromSeconds(delaySeconds));
                    }
                }
            }
            throw new Exception($"Operation failed after {maxAttempts} attempts", lastException);
        }

        private async Task RetryAsync(Func<Task> action, int maxAttempts, float delaySeconds)
        {
            await RetryAsync(async () =>
            {
                await action();
                return true;
            }, maxAttempts, delaySeconds);
        }
    }
}

