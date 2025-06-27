using VContainer;

namespace App.InternalDomains.LifeTimeScopesRegistry
{
    public interface ILifeTimeScopeRegistry
    {
        void Subscribe(LifeTimeScopeType type, IObjectResolver resolver);
        void UnSubscribe(LifeTimeScopeType type);

        T Resolve<T>(LifeTimeScopeType type) where T : class;
    }
}