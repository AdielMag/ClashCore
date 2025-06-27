using UnityEngine;

using VContainer;
using VContainer.Unity;

namespace App.InternalDomains.LifeTimeScopesRegistry
{
    public class LifeTimeScopeSubscriber : MonoBehaviour
    {
        [SerializeField] private LifeTimeScopeType lifeTimeScopeType;
        [SerializeField] private LifetimeScope lifetimeScope;
        
        private ILifeTimeScopeRegistry _lifeTimeScopeRegistry;
        
        [Inject]
        public void Inject(ILifeTimeScopeRegistry lifeTimeScopeRegistry)
        {
            _lifeTimeScopeRegistry = lifeTimeScopeRegistry;
        }
        
        private void Awake()
        {
            var resolver = lifetimeScope.Container;
            _lifeTimeScopeRegistry.Subscribe(lifeTimeScopeType, resolver);
        }
        
        private void OnDestroy()
        {
            _lifeTimeScopeRegistry?.UnSubscribe(lifeTimeScopeType);
        }
    }
}