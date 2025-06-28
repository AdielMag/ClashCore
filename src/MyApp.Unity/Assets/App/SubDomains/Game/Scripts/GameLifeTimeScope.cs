using App.Scripts.Command;
using App.Scripts.VContainerExtensions.LifeTimeScopes;
using App.SubDomains.Game.Scripts.Command;

using Cinemachine;

using Shared.Controller.PhysicsController.Controller;

using UnityEngine;
using UnityEngine.InputSystem;

using VContainer;

namespace App.SubDomains.Game.Scripts
{
    public class GameLifeTimeScope : PrimaryLifeTimeScope
    {
        [Space]

        [SerializeField] private CinemachineVirtualCamera virtualCamera;
        [SerializeField] private CinemachineBrain cameraBrain;
        [SerializeField] private InputActionAsset inputActionAsset;

        protected override void InternalConfigure(IContainerBuilder builder)
        {
            builder.Register<GameStarter>(Lifetime.Singleton).AsImplementedInterfaces();
            
            RegisterComponents(builder);
            RegisterSharedControllers(builder);
            RegisterCommands(builder);
            
            builder.RegisterBuildCallback(resolver =>
            {
                var commandPool = resolver.Resolve<CommandPool<QuitGameCommand>>();
                commandPool.SetPoolSize(1);
            });
        }

        private void RegisterComponents(IContainerBuilder builder)
        {
            builder.RegisterInstance(virtualCamera);
            builder.RegisterInstance(cameraBrain);
            builder.RegisterInstance(inputActionAsset);
        }
        
        private void RegisterSharedControllers(IContainerBuilder builder)
        {
            builder.Register<PhysicsController>(Lifetime.Transient).AsImplementedInterfaces();
        }
        
        private void RegisterCommands(IContainerBuilder builder)
        {
            builder.Register<QuitGameCommand>(Lifetime.Scoped);
            builder.Register<CommandPool<QuitGameCommand>>(Lifetime.Singleton);
        }
    }
}