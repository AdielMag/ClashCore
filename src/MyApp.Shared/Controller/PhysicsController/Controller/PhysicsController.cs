// ---------------------------------------------------------------------------------------------------------------------
//  PhysicsController.cs – now with configurable acceleration / deceleration curves
// ---------------------------------------------------------------------------------------------------------------------
#nullable enable
using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using Shared.Controller.PhysicsController.Interface;
using Shared.Data;

namespace Shared.Controller.PhysicsController.Controller
{
    public sealed class PhysicsController : IPhysicsController,
                                            IDisposable
    {
        /* ─────────────────────────── CONSTANTS ─────────────────────────── */
        private const float _kGravity = -9.81f;
        private const float _kEpsilon = 0.001f;
        private const float _kEpsilonSq = _kEpsilon * _kEpsilon;
        private const float _kDeg2Rad = MathF.PI / 180f;

        /* ───────────────────── CONFIGURATION / STATE ───────────────────── */
        private ITransformable ? _transform;
        private float _moveSpeed;
        private float _rotSpeedRad;

        // Curves
        private Curve _accelCurve = Curve.Linear01;
        private Curve _decelCurve = Curve.Linear01;

        // Curve state
        private float _curveTimer;
        private bool _accelerating; // true ⇒ in acceleration phase
        private float _speedFactor; // 0‑1 multiplier of _moveSpeed

        private Vector3 _velocity;
        private bool _grounded;

        private readonly PositionChangedEventData _positionData = new ();
        private readonly RotationChangedData _rotationArgs = new ();
        private float _cameraAngleOffsetRad = 0f;

        /* ───────────────────────── INITIALISE ───────────────────────── */
        public void Setup(ITransformable transform,
                          float moveSpeed,
                          float rotationSpeedDeg,
                          Curve ? accelerationCurve = null,
                          Curve ? decelerationCurve = null,
                          float cameraAngleOffsetDeg = 0f)
        {
            _transform = transform ?? throw new ArgumentNullException(nameof(transform));
            _moveSpeed = moveSpeed;
            _rotSpeedRad = rotationSpeedDeg * _kDeg2Rad;
            _accelCurve = accelerationCurve ?? Curve.Linear01;
            _decelCurve = decelerationCurve ?? Curve.Linear01;
            _velocity = Vector3.Zero;
            _grounded = true;
            _curveTimer = 0f;
            _speedFactor = 0f;
            _accelerating = false;
            _cameraAngleOffsetRad = cameraAngleOffsetDeg * _kDeg2Rad;
        }

        /* ───────────────────────── PUBLIC API ───────────────────────── */
        public PositionChangedEventData Move(Vector2 input, float dt)
        {
            var inputMagSq = input.LengthSquared();
            var hasInput = inputMagSq > _kEpsilonSq;

            // Transition between accel ↔ decel
            if (hasInput != _accelerating)
            {
                _accelerating = hasInput;
                _curveTimer = 0f;
            }

            // Advance along the current curve
            _curveTimer += dt;

            if (_accelerating)
            {
                var t = MathF.Min(_curveTimer, _accelCurve.Duration);
                _speedFactor = _accelCurve.Evaluate(t) / _accelCurve.Evaluate(_accelCurve.Duration);
            }
            else
            {
                var t = MathF.Min(_curveTimer, _decelCurve.Duration);
                _speedFactor = 1f - (_decelCurve.Evaluate(t) / _decelCurve.Evaluate(_decelCurve.Duration));
            }

            _speedFactor = Math.Max(_speedFactor, 0f);

            // Apply camera angle offset to input direction
            Vector2 rotatedInput = hasInput ? RotateVector2(input, _cameraAngleOffsetRad) : Vector2.Zero;
            var desiredDir = hasInput ? Vector3.Normalize(new Vector3(rotatedInput.X, 0f, rotatedInput.Y)) : Vector3.Zero;
            var desiredHorVel = desiredDir * (_moveSpeed * _speedFactor);

            // Preserve vertical velocity / gravity
            _velocity.X = desiredHorVel.X;
            _velocity.Z = desiredHorVel.Z;
            _velocity.Y = _grounded ? 0f : _velocity.Y + _kGravity * dt;

            // Integrate position
            var oldPos = _transform.Position;
            var newPos = oldPos + _velocity * dt;

            // Simple ground collision (plane Y=0)
            if (newPos.Y <= 0f)
            {
                newPos.Y = 0f;
                _grounded = true;
                _velocity.Y = 0f;
            }
            else
            {
                _grounded = false;
            }

            if (Moved(newPos, oldPos))
            {
                _transform.Position = newPos;
                _positionData.Update(oldPos, newPos);
            }

            return _positionData;
        }

        public RotationChangedData Rotate(Vector2 input, float dt)
        {
            var passedRotationThreshold = input.LengthSquared() > _kEpsilonSq;
            if (passedRotationThreshold)
            {
                // Apply camera angle offset to input direction
                Vector2 rotatedInput = RotateVector2(input, _cameraAngleOffsetRad);
                var targetYaw = MathF.Atan2(rotatedInput.X, rotatedInput.Y);
                var currentYaw = GetYawRad(_transform.Rotation);
                var delta = Normalise(targetYaw - currentYaw);

                var maxStep = _rotSpeedRad * dt;
                var step = MathF.Abs(delta) <= maxStep ? delta : MathF.Sign(delta) * maxStep;

                if (!(MathF.Abs(step) <= 1e-6f))
                {
                    var oldRot = _transform.Rotation;
                    var newRot = YawQuat(currentYaw + step);
                    _transform.Rotation = newRot;
                    _rotationArgs.Update(oldRot, newRot);
                }
            }

            return _rotationArgs;
        }

        public void SetMoveSpeed(float moveSpeed)
        {
            _moveSpeed = moveSpeed;
        }

        public void SetRotationSpeedDeg(float rotationSpeedDeg)
        {
            _rotSpeedRad = rotationSpeedDeg * _kDeg2Rad;
        }

        /* ────────────────────────── HELPERS ────────────────────────── */
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float GetYawRad(in Quaternion q)
        {
            return MathF.Atan2(2f * (q.W * q.Y + q.X * q.Z), 1f - 2f * (q.Y * q.Y + q.X * q.X));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Quaternion YawQuat(float yawRad)
        {
            var half = yawRad * 0.5f;
            return new Quaternion(0f, MathF.Sin(half), 0f, MathF.Cos(half));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float Normalise(float a)
        {
            const float twoPi = 2f * MathF.PI;
            a %= twoPi;
            return a > MathF.PI ? a - twoPi : a < -MathF.PI ? a + twoPi : a;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool Moved(in Vector3 a, in Vector3 b)
        {
            float dx = a.X - b.X, dy = a.Y - b.Y, dz = a.Z - b.Z;
            return (dx * dx + dy * dy + dz * dz) > _kEpsilonSq;
        }

        public void Dispose()
        {
            _transform = null;
        }
        
        private static Vector2 RotateVector2(Vector2 v, float angleRad)
        {
            var cos = MathF.Cos(angleRad);
            var sin = MathF.Sin(angleRad);
            return new Vector2(
                v.X * cos - v.Y * sin,
                v.X * sin + v.Y * cos
            );
        }
    }
}

/* ───────────────────── POOLABLE EVENT ARGUMENTS ───────────────────── */
public sealed class PositionChangedEventData
{
    public bool HasMoved => ! OldPosition.Equals(NewPosition);

    public Vector3 OldPosition
    {
        get;
        private set;
    }

    public Vector3 NewPosition
    {
        get;
        private set;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PositionChangedEventData Update(Vector3 oldPos, Vector3 newPos)
    {
        OldPosition = oldPos;
        NewPosition = newPos;
        return this;
    }
}

public sealed class RotationChangedData
{
    public bool HasRotated => ! OldRotation.Equals(NewRotation);
    public Quaternion OldRotation
    {
        get;
        private set;
    }

    public Quaternion NewRotation
    {
        get;
        private set;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public RotationChangedData Update(Quaternion oldRot, Quaternion newRot)
    {
        OldRotation = oldRot;
        NewRotation = newRot;
        return this;
    }
}