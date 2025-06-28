using App.Scripts.VContainerExtensions.LifeTimeScopes;
using App.SubDomains.Game.SubDomains.PlayerController.Scripts.Controller;
using App.SubDomains.Game.SubDomains.PlayerController.Scripts.ScriptableObjects;
using App.SubDomains.Game.SubDomains.PlayerView.Scripts.Provider;

using UnityEngine;

using VContainer;
using VContainer.Unity;

namespace App.SubDomains.Game.SubDomains.PlayersManager
{
    public class PlayerManagerLifeTimeScope : SubLifeTimeScope
    {
        [SerializeField] private PlayerPhysicsSettings playerPhysicsSettings;
        [SerializeField] private PlayerView.Scripts.View.PlayerView playerViewPrefab;
        [SerializeField] private PlayerViewParentProvider playersViewParent;
        
        public override void Configure(IContainerBuilder builder)
        {
            RegisterReferences(builder);
            RegisterPlayerControllers(builder);
            
            builder.Register<PlayersManager>(Lifetime.Singleton).AsImplementedInterfaces();
        }

        private void RegisterReferences(IContainerBuilder builder)
        {
            builder.RegisterInstance(playerPhysicsSettings);
            builder.RegisterInstance(playerViewPrefab);
            builder.RegisterInstance(playersViewParent).AsImplementedInterfaces();
        }

        private void RegisterPlayerControllers(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<LocalPlayerController>(Lifetime.Transient).AsSelf();
            builder.RegisterEntryPoint<RemotePlayerController>(Lifetime.Transient).AsSelf();
        }
    }
}