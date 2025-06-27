using App.Scripts.VContainerExtensions.LifeTimeScopes;

using VContainer;

namespace App.SubDomains.Game.SubDomains.InputManager.Scripts
{
    public class InputManagerLifeTimeScope : SubLifeTimeScope
    {
        public override void Configure(IContainerBuilder builder)
        {
            builder.Register<InputManager>(Lifetime.Singleton).AsImplementedInterfaces();
        }
    }
}