using Shared.Data;

using UnityEngine;

using Quaternion = System.Numerics.Quaternion;
using Vector3 = System.Numerics.Vector3;

namespace App.SubDomains.Game.SubDomains.PlayerController.Scripts.@interface
{
    public interface IPlayerController
    {
        GameObject ViewGameObject { get; }
        
        void Create(TransformData transformData);
        void UpdatePositionAndRotation(Vector3 position, Quaternion rotation);
        void LateTick();
        void Dispose();
    }
}