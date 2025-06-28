using App.SubDomains.Game.SubDomains.PlayerController.Scripts.@interface;
using App.SubDomains.Game.SubDomains.PlayerView.Scripts.Interface;

using Shared.Data;

using UnityEngine;

using VContainer;
using VContainer.Unity;

using Quaternion = System.Numerics.Quaternion;
using Vector3 = System.Numerics.Vector3;

namespace App.SubDomains.Game.SubDomains.PlayerController.Scripts.Controller
{
    public abstract class PlayerControllerBase : IPlayerController
    {
        public GameObject ViewGameObject => view.gameObject;
        
        [Inject] private readonly PlayerView.Scripts.View.PlayerView _prefab;
        [Inject] private readonly IPlayerViewParentProvider _playersViewParentProvider;
        [Inject] protected readonly IObjectResolver resolver;
        
        protected TransformData transform;
        protected PlayerView.Scripts.View.PlayerView view;

        public virtual void Create(TransformData transformData)
        {
            transform = transformData;
            view = resolver.Instantiate(_prefab, _playersViewParentProvider.PlayerViewParent);
            view.name = $"Player: {transform.Id}";
            
            UpdatePositionAndRotation(transform.Position, transform.Rotation);
        }

        public virtual void UpdatePositionAndRotation(Vector3 position, Quaternion rotation)
        {
            view.Position = position;
            view.Rotation = rotation;
        }
        
        public virtual void UpdateTarget(Transform target)
        {
            view.PlayerVisualController.UpdateLookAtTarget(target);
        }

        public abstract void LateTick();

        public virtual void Dispose()
        {
            Object.Destroy(view.gameObject);
        }
    }
}