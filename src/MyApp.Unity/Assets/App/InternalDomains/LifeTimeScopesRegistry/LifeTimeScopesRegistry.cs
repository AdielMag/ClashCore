using System.Collections.Generic;

using App.InternalDomains.DebugService;

using VContainer;

namespace App.InternalDomains.LifeTimeScopesRegistry
{
    public class LifeTimeScopesRegistry : ILifeTimeScopeRegistry
    {
        private readonly IDebugService _debugService;
        private readonly Dictionary<LifeTimeScopeType, IObjectResolver> _scopes;
        
        public LifeTimeScopesRegistry(IDebugService debugService)
        {
            _debugService = debugService;
            
            var capacity = System.Enum.GetValues(typeof(LifeTimeScopeType)).Length;
            _scopes = new Dictionary<LifeTimeScopeType, IObjectResolver>(capacity: capacity);
        }
        
        public void Subscribe(LifeTimeScopeType type, IObjectResolver resolver)
        {
            var added = _scopes.TryAdd(type, resolver);
            if (! added)
            {
                _debugService.LogError(
                    $"Failed to register LifeTimeScope: {type}. It already exists in the registry.");
            }
        }

        public void UnSubscribe(LifeTimeScopeType type)
        {
            var removed = _scopes.Remove(type);
            if (! removed)
            {
                _debugService.LogError(
                    $"Failed to unregister LifeTimeScope: {type}. It does not exist in the registry.");
            }
        }

        public T Resolve<T>(LifeTimeScopeType type) where T : class
        {
            return _scopes.TryGetValue(type, out var resolver) ? resolver.Resolve<T>() : null;
        }
    }
}