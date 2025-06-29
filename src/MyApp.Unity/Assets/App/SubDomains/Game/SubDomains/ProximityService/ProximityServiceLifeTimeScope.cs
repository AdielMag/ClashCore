using App.Scripts.VContainerExtensions.LifeTimeScopes;

using VContainer;

namespace App.SubDomains.Game.SubDomains.ProximityService
{
    public class ProximityServiceLifeTimeScope : SubLifeTimeScope
    {
        public override void Configure(IContainerBuilder builder)
        {
            builder.Register<ProximityService>(Lifetime.Singleton).AsImplementedInterfaces();
        }
    }
}