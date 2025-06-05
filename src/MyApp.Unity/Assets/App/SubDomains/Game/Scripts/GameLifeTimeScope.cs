using App.SubDomains.Game.SubDomains.CameraManager.Scripts;
using App.SubDomains.Game.SubDomains.Environment.Scripts.Manager;
using App.SubDomains.Game.SubDomains.Environment.Scripts.Provider;
using App.SubDomains.Game.SubDomains.GameNetworkHub;
using App.SubDomains.Game.SubDomains.InputManager.Scripts;
using App.SubDomains.Game.SubDomains.PlayerController.Scripts.Controller;
using App.SubDomains.Game.SubDomains.PlayerController.Scripts.ScriptableObjects;
using App.SubDomains.Game.SubDomains.PlayersManager;
using App.SubDomains.Game.SubDomains.PlayerView.Scripts.Provider;
using App.SubDomains.Game.SubDomains.PlayerView.Scripts.View;

using Cinemachine;

using Shared.Controller.PhysicsController.Controller;

using UnityEngine;
using UnityEngine.InputSystem;

using VContainer;
using VContainer.Unity;

namespace App.SubDomains.Game.Scripts
{
    public class GameLifeTimeScope : LifetimeScope
    {
        [Space]
        [SerializeField] private PlayerView playerViewPrefab;
        [SerializeField] private PlayerViewParentProvider playersViewParent;
        [SerializeField] private CinemachineVirtualCamera virtualCamera;
        [SerializeField] private CinemachineBrain cameraBrain;
        [SerializeField] private InputActionAsset inputActionAsset;
        [SerializeField] private EnvironmentParentProvider environmentParent;
        [SerializeField] private PlayerPhysicsSettings playerPhysicsSettings;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<GameStarter>(Lifetime.Singleton).AsImplementedInterfaces();
            
            RegisterManagers(builder);
            RegisterComponents(builder);
            RegisterSharedControllers(builder);
            RegisterPlayerControllers(builder);
        }
        
        private void RegisterComponents(IContainerBuilder builder)
        {
            builder.RegisterInstance(playerViewPrefab);
            builder.RegisterInstance(playersViewParent).AsImplementedInterfaces();
            builder.RegisterInstance(virtualCamera);
            builder.RegisterInstance(cameraBrain);
            builder.RegisterInstance(inputActionAsset);
            builder.RegisterInstance(environmentParent).AsImplementedInterfaces();
            builder.RegisterInstance(playerPhysicsSettings);
        }
        
        private void RegisterManagers(IContainerBuilder builder)
        {
            builder.Register<GameHubNetworkManager>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<PlayersManager>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<CameraManager>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<InputManager>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<EnvironmentManager>(Lifetime.Singleton).AsImplementedInterfaces();
        }
        
        private void RegisterSharedControllers(IContainerBuilder builder)
        {
            builder.Register<PhysicsController>(Lifetime.Transient).AsImplementedInterfaces();
        }
        
        private void RegisterPlayerControllers(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<LocalPlayerController>(Lifetime.Transient).AsSelf();
            builder.RegisterEntryPoint<RemotePlayerController>(Lifetime.Transient).AsSelf();
        }
    }
}