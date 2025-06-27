using UnityEngine;

using VContainer;

namespace App.Scripts.VContainerExtensions.LifeTimeScopes
{
    public abstract class SubLifeTimeScope : MonoBehaviour
    {
        public abstract void Configure(IContainerBuilder builder);
    }
}