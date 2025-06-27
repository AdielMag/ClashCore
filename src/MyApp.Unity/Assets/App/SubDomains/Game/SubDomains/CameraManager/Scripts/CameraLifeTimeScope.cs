using App.Scripts.VContainerExtensions.LifeTimeScopes;

using VContainer;

namespace App.SubDomains.Game.SubDomains.CameraManager.Scripts
{
    public class CameraLifeTimeScope : SubLifeTimeScope
    {
        public override void Configure(IContainerBuilder builder)
        {
            builder.Register<CameraManager>(Lifetime.Singleton).AsImplementedInterfaces();
        }
    }
}