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
            if (_playersService == null)
            {
                _playersService = await _networkService.GetService<Shared.Services.IPlayersService>();
            }
            
            try
            {
                var userId = PlayerPrefs.GetString(_kUserIdKey, string.Empty);
                
                if (string.IsNullOrEmpty(userId))
                {
                    _debugService.Log("No existing userId found. Starting registration process...");
                    userId = await RegisterNewPlayer();
                }
                else
                {
                    _debugService.Log($"Found existing userId: {userId}. Attempting to login...");
                    await LoginExistingPlayer(userId);
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
            try
            {
                _debugService.Log("Registering new player...");
                var newUserId = await _playersService.RegisterAsync();
                
                if (string.IsNullOrEmpty(newUserId))
                {
                    _debugService.LogError("Registration failed: Received empty userId");
                    throw new Exception("Registration failed: Empty userId received");
                }

                _debugService.Log($"Registration successful. New userId: {newUserId}");
                
                PlayerPrefs.SetString(_kUserIdKey, newUserId);
                PlayerPrefs.Save();
                _debugService.Log("UserId saved to PlayerPrefs");

                await LoginExistingPlayer(newUserId);

                return newUserId;
            }
            catch (Exception ex)
            {
                _debugService.LogError($"Registration failed: {ex.Message}");
                throw new Exception("Failed to register new player", ex);
            }
        }

        private async Task LoginExistingPlayer(string userId)
        {
            bool loginSuccesful = true;
            
            try
            {
                _debugService.Log($"Attempting to login with userId: {userId}");
                await _playersService.LoginAsync(userId);
            }
            catch (Exception ex)
            {
                _debugService.LogError($"Login failed for userId {userId}: {ex.Message}");
                
                if (ex.Message.Contains("Player not found"))
                {
                    _debugService.LogWarning(
                        "Player not found. Clearing stored userId and retrying with registration...");
                    PlayerPrefs.DeleteKey(_kUserIdKey);
                    PlayerPrefs.Save();
                }
            }
        }
    }
}