using App.Scripts.VContainerExtensions.LifeTimeScopes;
using App.SubDomains.Game.SubDomains.Environment.Scripts.Manager;
using App.SubDomains.Game.SubDomains.Environment.Scripts.Provider;

using UnityEngine;

using VContainer;

namespace App.SubDomains.Game.SubDomains.Environment.Scripts.Installer
{
    public class EnvironmentLifeTimeScope : SubLifeTimeScope
    {
        [SerializeField] private EnvironmentParentProvider environmentParent;

        public override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(environmentParent).AsImplementedInterfaces();
            builder.Register<EnvironmentManager>(Lifetime.Singleton).AsImplementedInterfaces();

        }
    }
}