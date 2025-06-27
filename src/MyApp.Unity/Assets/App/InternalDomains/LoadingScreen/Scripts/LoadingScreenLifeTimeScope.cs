using App.Scripts.View;

using UnityEngine;

using VContainer;
using VContainer.Unity;

namespace App.InternalDomains.LoadingScreen.Scripts
{
    public class LoadingScreenLifeTimeScope : LifetimeScope
    {
        [Space, SerializeField] private LoadingScreenBar loadingScreenBar;
        
        protected override void Configure(IContainerBuilder builder)
        {
            RegisterViews(builder);
        }
        
        private void RegisterViews(IContainerBuilder builder)
        {
            builder.RegisterComponent(loadingScreenBar);
        }
    }
}