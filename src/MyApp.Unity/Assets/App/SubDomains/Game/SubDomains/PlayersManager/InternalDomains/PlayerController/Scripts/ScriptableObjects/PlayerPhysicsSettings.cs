using UnityEngine;

namespace App.SubDomains.Game.SubDomains.PlayerController.Scripts.ScriptableObjects
{
    [CreateAssetMenu(fileName = "PlayerPhysicsSettings", menuName = "ScriptableObjects/PlayerPhysicsSettings", order = 1)]
    public class PlayerPhysicsSettings : ScriptableObject
    {
        [SerializeField] private float speed = 5f;
        [SerializeField] private float lookAtSpeed = 3f;
        [SerializeField] private float rotationSpeed = 720f;
        [SerializeField] private AnimationCurve accelerationCurve;
        [SerializeField] private AnimationCurve decelerationCurve;
        
        public float Speed => speed;
        public float LookAtSpeed => lookAtSpeed;
        public float RotationSpeed => rotationSpeed;
        public AnimationCurve AccelerationCurve => accelerationCurve;
        public AnimationCurve DecelerationCurve => decelerationCurve;
    }
}