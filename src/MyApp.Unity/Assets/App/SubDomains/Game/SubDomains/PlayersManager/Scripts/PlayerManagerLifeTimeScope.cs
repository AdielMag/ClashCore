using App.Scripts.VContainerExtensions.LifeTimeScopes;
using App.SubDomains.Game.SubDomains.PlayerController.Scripts.Controller;
using App.SubDomains.Game.SubDomains.PlayerController.Scripts.ScriptableObjects;

using UnityEngine;

using VContainer;
using VContainer.Unity;

namespace App.SubDomains.Game.SubDomains.PlayersManager
{
    public class PlayerManagerLifeTimeScope : SubLifeTimeScope
    {
        [SerializeField] private PlayerPhysicsSettings playerPhysicsSettings;
        
        public override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(playerPhysicsSettings);
            builder.Register<PlayersManager>(Lifetime.Singleton).AsImplementedInterfaces();
            
            RegisterPlayerControllers(builder);
        }
        
        private void RegisterPlayerControllers(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<LocalPlayerController>(Lifetime.Transient).AsSelf();
            builder.RegisterEntryPoint<RemotePlayerController>(Lifetime.Transient).AsSelf();
        }
    }
}