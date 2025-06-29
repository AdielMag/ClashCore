using Cysharp.Threading.Tasks;

using UnityEngine;

namespace App.SubDomains.Game.Scripts.Interface
{
    public interface IProximityService
    {
        void RegisterTransform(Transform transform);
        void UnregisterTransform(Transform transform);
        Transform GetNearbyTarget(Transform self, Vector3 position, float range);
        UniTask<Transform> GetNearbyTargetAsync(Transform self, Vector3 position, float range);
        UniTask<Transform[]> GetNearbyTargetsAsync(Transform self, Vector3 position, float range, int maxCount = 10);
    }
}