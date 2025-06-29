using App.InternalDomains.AppLifetimeService;
using App.InternalDomains.DebugService;
using App.InternalDomains.LifeTimeScopesRegistry;
using App.InternalDomains.LoadingScreen.Scripts.Services;
using App.InternalDomains.NetworkService;
using App.InternalDomains.PlayersService;
using App.InternalDomains.SceneService;

using VContainer;
using VContainer.Unity;

namespace App.Scripts
{
    public class AppLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<AppStarter>(Lifetime.Singleton).AsImplementedInterfaces();
            
            RegisterServices(builder);
            
            DontDestroyOnLoad(this);
        }

        private void RegisterServices(IContainerBuilder builder)
        {
            builder.Register<NetworkService>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<DebugService>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<SceneService>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<AppLifetimeService>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<PlayersService>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<LifeTimeScopesRegistry>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<LoadingScreenService>(Lifetime.Singleton).AsImplementedInterfaces();
        }
    }
}