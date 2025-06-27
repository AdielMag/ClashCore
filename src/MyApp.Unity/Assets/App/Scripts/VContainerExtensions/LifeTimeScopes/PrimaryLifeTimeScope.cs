using System.Collections.Generic;

using UnityEngine;

using VContainer;
using VContainer.Unity;

namespace App.Scripts.VContainerExtensions.LifeTimeScopes
{
    public abstract class PrimaryLifeTimeScope : LifetimeScope
    {
        [SerializeField] private List<SubLifeTimeScope> subLifeTimeScopes;
        
        protected sealed override void Configure(IContainerBuilder builder)
        {
            foreach (var subLifeTimeScope in subLifeTimeScopes)
            {
                subLifeTimeScope.Configure(builder);
            }
            
            InternalConfigure(builder);
        }
        
        protected abstract void InternalConfigure(IContainerBuilder builder);
    }
}