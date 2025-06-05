using System.Collections.Generic;
using System.Numerics;

using App.SubDomains.Game.SubDomains.PlayerController.Scripts.Controller;
using App.SubDomains.Game.SubDomains.PlayerController.Scripts.@interface;

using Shared.Data;


using VContainer;
using VContainer.Unity;

namespace App.SubDomains.Game.SubDomains.PlayersManager
{
    public class PlayersManager : IPlayersManager, ILocalPlayerProvider , ILateTickable
    {
        public IPlayerController LocalPlayerController
        {
            get;
            private set;
        }
        
        [Inject] private readonly IObjectResolver _resolver;
        
        private readonly Dictionary<string, IPlayerController> _playersControllers = new();
        
        public void OnPlayerJoined(TransformData transformData, bool isRemote)
        {
            IPlayerController controller = isRemote ?
                _resolver.Resolve<RemotePlayerController>() :
                _resolver.Resolve<LocalPlayerController>();

            controller.Create(transformData);

            if (! isRemote)
            {
                LocalPlayerController = controller;
            }

            _playersControllers[transformData.Id] = controller;
        }
        
        public void OnPlayerLeft(string id)
        {
            var controller = _playersControllers[id];
            controller.Dispose();
            _playersControllers.Remove(id);
        }

        public void OnPlayerMoved(string id, Vector3 position, Quaternion rotation)
        {
            if (!_playersControllers.TryGetValue(id, out var controller))
            {
                return;
            }
            
            controller.UpdatePositionAndRotation(position, rotation);
        }

        public void LateTick()
        {
            foreach (var controller in _playersControllers.Values)
            {
                controller.LateTick();
            }
        }
        
        public void Dispose()
        {
            foreach (var controller in _playersControllers.Values)
            {
                controller.Dispose();
            }
            
            _playersControllers.Clear();
        }
    }
}