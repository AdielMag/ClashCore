// ---------------------------------------------------------------------------------------------------------------------
//  CurveUtils.cs – convert UnityEngine.AnimationCurve ➜ Shared.Data.Curve with tangents
// ---------------------------------------------------------------------------------------------------------------------
#nullable enable

using System;

using Shared.Data;

using UnityEngine;

namespace App.SubDomains.Game.Scripts.Utils
{
#if UNITY_2019_4_OR_NEWER
    using PhysKeyframe = Shared.Data.Keyframe;

    public static class CurveUtils
    {
        /// <summary>
        /// Convert a Unity <see cref="AnimationCurve"/> (including tangents) to an
        /// engine-agnostic <see cref="Curve"/> that reproduces the same shape.
        /// </summary>
        public static Curve ToPhysicsCurve(this AnimationCurve unityCurve)
        {
            if (unityCurve == null) throw new ArgumentNullException(nameof(unityCurve));

            var src = unityCurve.keys;
            var dst = new PhysKeyframe[src.Length];

            for (int i = 0; i < src.Length; ++i)
            {
                dst[i] = new PhysKeyframe(
                    src[i].time,
                    src[i].value,
                    src[i].inTangent,
                    src[i].outTangent);
            }
            return new Curve(dst);
        }
    }
#endif
}