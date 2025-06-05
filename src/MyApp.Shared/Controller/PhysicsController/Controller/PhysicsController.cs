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

        public event Action<PositionChangedEventArgs> ? OnPositionChanged;
        public event Action<RotationChangedEventArgs> ? OnRotationChanged;

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

        private readonly PositionChangedEventArgs _positionArgs = new PositionChangedEventArgs();
        private readonly RotationChangedEventArgs _rotationArgs = new RotationChangedEventArgs();

        /* ───────────────────────── INITIALISE ───────────────────────── */
        public void Setup(ITransformable transform,
                          float moveSpeed,
                          float rotationSpeedDeg,
                          Curve ? accelerationCurve = null,
                          Curve ? decelerationCurve = null)
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
        }

        /* ───────────────────────── PUBLIC API ───────────────────────── */
        public void Move(Vector2 input, float dt)
        {
            if (_transform is null) throw new InvalidOperationException("Call Setup() first.");

            float inputMagSq = input.LengthSquared();
            bool hasInput = inputMagSq > _kEpsilonSq;

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
                float t = MathF.Min(_curveTimer, _accelCurve.Duration);
                _speedFactor = _accelCurve.Evaluate(t) / _accelCurve.Evaluate(_accelCurve.Duration);
            }
            else
            {
                float t = MathF.Min(_curveTimer, _decelCurve.Duration);
                _speedFactor = 1f - (_decelCurve.Evaluate(t) / _decelCurve.Evaluate(_decelCurve.Duration));
            }

            _speedFactor = Math.Max(_speedFactor, 0f);

            // Desired horizontal velocity
            Vector3 desiredDir =
                hasInput ? Vector3.Normalize(new Vector3(input.X, 0f, input.Y)) : Vector3.Zero;

            Vector3 desiredHorVel = desiredDir * (_moveSpeed * _speedFactor);

            // Preserve vertical velocity / gravity
            _velocity.X = desiredHorVel.X;
            _velocity.Z = desiredHorVel.Z;
            _velocity.Y = _grounded ? 0f : _velocity.Y + _kGravity * dt;

            // Integrate position
            Vector3 oldPos = _transform.Position;
            Vector3 newPos = oldPos + _velocity * dt;

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
                _positionArgs.Update(oldPos, newPos);
                OnPositionChanged?.Invoke(_positionArgs);
            }
        }

        public void Rotate(Vector2 input, float dt)
        {
            if (_transform is null ||
                input.LengthSquared() <= _kEpsilonSq) return;

            float targetYaw = MathF.Atan2(input.X, input.Y);
            float currentYaw = GetYawRad(_transform.Rotation);
            float delta = Normalise(targetYaw - currentYaw);

            float maxStep = _rotSpeedRad * dt;
            float step = MathF.Abs(delta) <= maxStep ? delta : MathF.Sign(delta) * maxStep;
            if (MathF.Abs(step) <= 1e-6f) return;

            Quaternion oldRot = _transform.Rotation;
            Quaternion newRot = YawQuat(currentYaw + step);
            _transform.Rotation = newRot;
            _rotationArgs.Update(oldRot, newRot);
            OnRotationChanged?.Invoke(_rotationArgs);
        }

        /* ────────────────────────── HELPERS ────────────────────────── */
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float GetYawRad(in Quaternion q) =>
            MathF.Atan2(2f * (q.W * q.Y + q.X * q.Z), 1f - 2f * (q.Y * q.Y + q.X * q.X));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Quaternion YawQuat(float yawRad)
        {
            float half = yawRad * 0.5f;
            return new Quaternion(0f, MathF.Sin(half), 0f, MathF.Cos(half));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float Normalise(float a)
        {
            const float TwoPi = 2f * MathF.PI;
            a %= TwoPi;
            return a > MathF.PI ? a - TwoPi : a < -MathF.PI ? a + TwoPi : a;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool Moved(in Vector3 a, in Vector3 b)
        {
            float dx = a.X - b.X, dy = a.Y - b.Y, dz = a.Z - b.Z;
            return (dx * dx + dy * dy + dz * dz) > _kEpsilonSq;
        }

        public void Dispose() => _transform = null;
    }
}

/* ───────────────────── POOLABLE EVENT ARGUMENTS ───────────────────── */
    public sealed class PositionChangedEventArgs : EventArgs
    {
        public Vector3 OldPosition { get; private set; }
        public Vector3 NewPosition { get; private set; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public PositionChangedEventArgs Update(Vector3 oldPos, Vector3 newPos)
        {
            OldPosition = oldPos;
            NewPosition = newPos;
            return this;
        }
    }

    public sealed class RotationChangedEventArgs : EventArgs
    {
        public Quaternion OldRotation { get; private set; }
        public Quaternion NewRotation { get; private set; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RotationChangedEventArgs Update(Quaternion oldRot, Quaternion newRot)
        {
            OldRotation = oldRot;
            NewRotation = newRot;
            return this;
        }
    }

