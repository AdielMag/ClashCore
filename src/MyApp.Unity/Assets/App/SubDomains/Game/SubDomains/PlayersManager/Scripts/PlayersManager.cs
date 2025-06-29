using System.Collections.Generic;

using App.SubDomains.Game.Scripts.Interface;
using App.SubDomains.Game.SubDomains.PlayerController.Scripts.Controller;
using App.SubDomains.Game.SubDomains.PlayerController.Scripts.@interface;

using Shared.Data;

using UnityEngine;

using VContainer;
using VContainer.Unity;

using Quaternion = System.Numerics.Quaternion;
using Vector3 = System.Numerics.Vector3;

namespace App.SubDomains.Game.SubDomains.PlayersManager
{
    public class PlayersManager : IPlayersManager, ILocalPlayerProvider , ILateTickable
    {
        public IPlayerController LocalPlayerController
        {
            get;
            private set;
        }
        
        private readonly IObjectResolver _resolver;
        private readonly Dictionary<string, IPlayerController> _playersControllers = new();
        private readonly IProximityService _proximityService;
        private Transform testTransform;

        public PlayersManager(IObjectResolver resolver, IProximityService proximityService)
        {
            _resolver = resolver;
            _proximityService = proximityService;
            
            testTransform = new GameObject("TestTarget").transform;
            testTransform.position = UnityEngine.Vector3.zero;
            testTransform.rotation = UnityEngine.Quaternion.identity;
            proximityService.RegisterTransform(testTransform);
        }

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
            _proximityService.RegisterTransform(controller.ViewGameObject.transform);
        }
        
        public void OnPlayerLeft(string id)
        {
            var controller = _playersControllers[id];
            controller.Dispose();
            _playersControllers.Remove(id);
            _proximityService.UnregisterTransform(controller.ViewGameObject.transform);
        }

        public void OnPlayerMoved(string id, Vector3 position, Quaternion rotation)
        {
            if (!_playersControllers.TryGetValue(id, out var controller))
            {
                return;
            }
            
            controller.UpdatePositionAndRotation(position, rotation);
        }
        
        public void OnPlayerTargetChanged(string playerId, string targetId)
        {
            if (! _playersControllers.TryGetValue(playerId, out var controller) ||
                ! _playersControllers.TryGetValue(targetId, out var targetController))
            {
                return;
            }

            controller.UpdateTarget(targetController.ViewGameObject.transform);
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