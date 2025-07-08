using System;

using UnityEngine.UIElements;

using VContainer;
using VContainer.Unity;

using Object = UnityEngine.Object;

namespace App.Scripts.Editor.VContainerExtensionsEditor
{
    public abstract class EditorDiContext : IDisposable
    {
        private readonly LifetimeScope _lifetimeScope;
        private readonly IObjectResolver _resolver;
        private readonly bool _isBuilt;

        protected EditorDiContext (VisualElement root)
        {
            if (_isBuilt)
            {
                return;
            }

            _lifetimeScope = LifetimeScope.Create(builder =>
            {
                builder.RegisterComponent(root).AsSelf().AsImplementedInterfaces();
                Configure(builder);
            });
            _lifetimeScope.gameObject.name = $"{GetType().Name}_LifetimeScope_DONT_DELETE!!!";
            _lifetimeScope.Build();

            _resolver = _lifetimeScope.Container;
            _isBuilt = true;

            OnContainerBuilt();
        }

        public void Dispose()
        {
            _lifetimeScope?.DisposeCore();
            Object.DestroyImmediate(_lifetimeScope?.gameObject);
            _resolver?.Dispose();
        }
        
        protected abstract void Configure(IContainerBuilder builder);

        protected virtual void OnContainerBuilt()
        {
        }

        protected T Resolve<T>()
        {
            return _resolver.Resolve<T>();
        }
    }
}