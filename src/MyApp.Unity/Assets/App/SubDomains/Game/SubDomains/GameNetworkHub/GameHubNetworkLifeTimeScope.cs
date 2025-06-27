using App.Scripts.VContainerExtensions.LifeTimeScopes;

using VContainer;

namespace App.SubDomains.Game.SubDomains.GameNetworkHub
{
    public class GameHubNetworkLifeTimeScope : SubLifeTimeScope
    {
        public override void Configure(IContainerBuilder builder)
        {
            builder.Register<GameHubNetworkManager>(Lifetime.Singleton).AsImplementedInterfaces();
        }
    }
}