using System;
using System.Runtime.CompilerServices;

namespace Shared.Data
{
    /// <summary>
    /// Keyframe with time, value, and the same in/out tangents used by Unity.
    /// </summary>
    public readonly struct Keyframe
    {
        public readonly float Time;
        public readonly float Value;
        public readonly float InTangent;
        public readonly float OutTangent;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Keyframe(float time, float value,
                        float inTangent  = 0f,
                        float outTangent = 0f)
        {
            Time       = time;
            Value      = value;
            InTangent  = inTangent;
            OutTangent = outTangent;
        }
    }
    
    public sealed class Curve
    {
        private readonly Keyframe[] _keys;

        public Curve(params Keyframe[] keys)
        {
            if (keys is null || keys.Length == 0)
                throw new ArgumentException("Curve needs at least one key.");

            _keys = (Keyframe[])keys.Clone();
            Array.Sort(_keys, (a, b) => a.Time.CompareTo(b.Time));
        }

        /// <summary>
        /// Evaluate using the Hermite basis (same as Unity’s AnimationCurve).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Evaluate(float t)
        {
            if (t <= _keys[0].Time)  return _keys[0].Value;
            if (t >= _keys[^1].Time) return _keys[^1].Value;

            // Find the first key whose time ≥ t  (keys are sorted)
            for (int i = 1; i < _keys.Length; ++i)
            {
                if (t <= _keys[i].Time)
                {
                    var a   = _keys[i - 1];
                    var b   = _keys[i];
                    float dt = b.Time - a.Time;

                    // Normalised parameter u in [0,1]
                    float u  = (t - a.Time) / dt;
                    float u2 = u * u;
                    float u3 = u2 * u;

                    // Hermite basis
                    float h00 =  2f * u3 - 3f * u2 + 1f;
                    float h10 =        u3 - 2f * u2 + u;
                    float h01 = -2f * u3 + 3f * u2;
                    float h11 =        u3 -       u2;

                    float m0  = a.OutTangent * dt;
                    float m1  = b.InTangent  * dt;

                    return h00 * a.Value + h10 * m0 + h01 * b.Value + h11 * m1;
                }
            }
            return _keys[^1].Value;                // fallback (shouldn’t reach)
        }

        /// <summary>Total span in seconds: lastKey.time − firstKey.time.</summary>
        public float Duration => _keys[^1].Time - _keys[0].Time;

        /// <summary>Linear 0→1 (tangents = 1).</summary>
        public static Curve Linear01 { get; } = new Curve(
            new Keyframe(0f, 0f, 1f, 1f),
            new Keyframe(1f, 1f, 1f, 1f));
    }
}