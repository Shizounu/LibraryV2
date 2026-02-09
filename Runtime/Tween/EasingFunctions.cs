using System;
using UnityEngine;

namespace Shizounu.Library.Tweening
{
    /// <summary>
    /// Collection of easing functions for smooth tweening animations.
    /// </summary>
    public static class EasingFunctions
    {
        /// <summary>
        /// Linear interpolation (no easing).
        /// </summary>
        public static float Linear(float t)
        {
            return t;
        }

        #region Quadratic (t^2)

        public static float QuadIn(float t)
        {
            return t * t;
        }

        public static float QuadOut(float t)
        {
            return 1f - (1f - t) * (1f - t);
        }

        public static float QuadInOut(float t)
        {
            return t < 0.5f ? 2f * t * t : 1f - (-2f * t + 2f) * (-2f * t + 2f) / 2f;
        }

        #endregion

        #region Cubic (t^3)

        public static float CubicIn(float t)
        {
            return t * t * t;
        }

        public static float CubicOut(float t)
        {
            return 1f - (1f - t) * (1f - t) * (1f - t);
        }

        public static float CubicInOut(float t)
        {
            return t < 0.5f ? 4f * t * t * t : 1f - (-2f * t + 2f) * (-2f * t + 2f) * (-2f * t + 2f) / 2f;
        }

        #endregion

        #region Quartic (t^4)

        public static float QuartIn(float t)
        {
            return t * t * t * t;
        }

        public static float QuartOut(float t)
        {
            return 1f - (1f - t) * (1f - t) * (1f - t) * (1f - t);
        }

        public static float QuartInOut(float t)
        {
            return t < 0.5f ? 8f * t * t * t * t : 1f - (-2f * t + 2f) * (-2f * t + 2f) * (-2f * t + 2f) * (-2f * t + 2f) / 2f;
        }

        #endregion

        #region Quintic (t^5)

        public static float QuintIn(float t)
        {
            return t * t * t * t * t;
        }

        public static float QuintOut(float t)
        {
            return 1f - (1f - t) * (1f - t) * (1f - t) * (1f - t) * (1f - t);
        }

        public static float QuintInOut(float t)
        {
            return t < 0.5f ? 16f * t * t * t * t * t : 1f - (-2f * t + 2f) * (-2f * t + 2f) * (-2f * t + 2f) * (-2f * t + 2f) * (-2f * t + 2f) / 2f;
        }

        #endregion

        #region Sine

        public static float SineIn(float t)
        {
            return 1f - Mathf.Cos((t * Mathf.PI) / 2f);
        }

        public static float SineOut(float t)
        {
            return Mathf.Sin((t * Mathf.PI) / 2f);
        }

        public static float SineInOut(float t)
        {
            return -(Mathf.Cos(Mathf.PI * t) - 1f) / 2f;
        }

        #endregion

        #region Exponential

        public static float ExpoIn(float t)
        {
            return t == 0f ? 0f : Mathf.Pow(2f, 10f * t - 10f);
        }

        public static float ExpoOut(float t)
        {
            return t == 1f ? 1f : 1f - Mathf.Pow(2f, -10f * t);
        }

        public static float ExpoInOut(float t)
        {
            if (t == 0f) return 0f;
            if (t == 1f) return 1f;
            return t < 0.5f ? Mathf.Pow(2f, 20f * t - 10f) / 2f : (2f - Mathf.Pow(2f, -20f * t + 10f)) / 2f;
        }

        #endregion

        #region Circular

        public static float CircIn(float t)
        {
            return 1f - Mathf.Sqrt(1f - t * t);
        }

        public static float CircOut(float t)
        {
            return Mathf.Sqrt(1f - (t - 1f) * (t - 1f));
        }

        public static float CircInOut(float t)
        {
            return t < 0.5f ? (1f - Mathf.Sqrt(1f - (2f * t) * (2f * t))) / 2f : (Mathf.Sqrt(1f - (-2f * t + 2f) * (-2f * t + 2f)) + 1f) / 2f;
        }

        #endregion

        #region Elastic

        public static float ElasticIn(float t)
        {
            const float c4 = (2f * Mathf.PI) / 3f;
            return t == 0f ? 0f : (t == 1f ? 1f : -Mathf.Pow(2f, 10f * t - 10f) * Mathf.Sin((t * 10f - 10.75f) * c4));
        }

        public static float ElasticOut(float t)
        {
            const float c4 = (2f * Mathf.PI) / 3f;
            return t == 0f ? 0f : (t == 1f ? 1f : Mathf.Pow(2f, -10f * t) * Mathf.Sin((t * 10f - 0.75f) * c4) + 1f);
        }

        public static float ElasticInOut(float t)
        {
            const float c5 = (2f * Mathf.PI) / 4.5f;
            if (t == 0f) return 0f;
            if (t == 1f) return 1f;
            return t < 0.5f
                ? -(Mathf.Pow(2f, 20f * t - 10f) * Mathf.Sin((20f * t - 11.125f) * c5)) / 2f
                : (Mathf.Pow(2f, -20f * t + 10f) * Mathf.Sin((20f * t - 11.125f) * c5)) / 2f + 1f;
        }

        #endregion

        #region Back

        public static float BackIn(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            return c3 * t * t * t - c1 * t * t;
        }

        public static float BackOut(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            return 1f + c3 * (t - 1f) * (t - 1f) * (t - 1f) + c1 * (t - 1f) * (t - 1f);
        }

        public static float BackInOut(float t)
        {
            const float c1 = 1.70158f;
            const float c2 = c1 * 1.525f;
            return t < 0.5f
                ? ((2f * t) * (2f * t) * ((c2 + 1f) * 2f * t - c2)) / 2f
                : ((2f * t - 2f) * (2f * t - 2f) * ((c2 + 1f) * (t * 2f - 2f) + c2) + 2f) / 2f;
        }

        #endregion

        #region Bounce

        public static float BounceIn(float t)
        {
            return 1f - BounceOut(1f - t);
        }

        public static float BounceOut(float t)
        {
            const float n1 = 7.5625f;
            const float d1 = 2.75f;

            if (t < 1f / d1)
                return n1 * t * t;
            if (t < 2f / d1)
                return n1 * (t -= 1.5f / d1) * t + 0.75f;
            if (t < 2.5f / d1)
                return n1 * (t -= 2.25f / d1) * t + 0.9375f;
            return n1 * (t -= 2.625f / d1) * t + 0.984375f;
        }

        public static float BounceInOut(float t)
        {
            return t < 0.5f
                ? (1f - BounceOut(1f - 2f * t)) / 2f
                : (1f + BounceOut(2f * t - 1f)) / 2f;
        }

        #endregion

        #region Delegate Support

        /// <summary>
        /// Delegate type for custom easing functions.
        /// </summary>
        public delegate float EasingFunction(float t);

        /// <summary>
        /// Gets an easing function by name.
        /// </summary>
        public static EasingFunction GetEasingFunction(EasingType type)
        {
            return type switch
            {
                EasingType.Linear => Linear,
                EasingType.QuadIn => QuadIn,
                EasingType.QuadOut => QuadOut,
                EasingType.QuadInOut => QuadInOut,
                EasingType.CubicIn => CubicIn,
                EasingType.CubicOut => CubicOut,
                EasingType.CubicInOut => CubicInOut,
                EasingType.QuartIn => QuartIn,
                EasingType.QuartOut => QuartOut,
                EasingType.QuartInOut => QuartInOut,
                EasingType.QuintIn => QuintIn,
                EasingType.QuintOut => QuintOut,
                EasingType.QuintInOut => QuintInOut,
                EasingType.SineIn => SineIn,
                EasingType.SineOut => SineOut,
                EasingType.SineInOut => SineInOut,
                EasingType.ExpoIn => ExpoIn,
                EasingType.ExpoOut => ExpoOut,
                EasingType.ExpoInOut => ExpoInOut,
                EasingType.CircIn => CircIn,
                EasingType.CircOut => CircOut,
                EasingType.CircInOut => CircInOut,
                EasingType.ElasticIn => ElasticIn,
                EasingType.ElasticOut => ElasticOut,
                EasingType.ElasticInOut => ElasticInOut,
                EasingType.BackIn => BackIn,
                EasingType.BackOut => BackOut,
                EasingType.BackInOut => BackInOut,
                EasingType.BounceIn => BounceIn,
                EasingType.BounceOut => BounceOut,
                EasingType.BounceInOut => BounceInOut,
                _ => Linear,
            };
        }

        #endregion
    }

    /// <summary>
    /// Enumeration of common easing types.
    /// </summary>
    public enum EasingType
    {
        Linear,
        QuadIn, QuadOut, QuadInOut,
        CubicIn, CubicOut, CubicInOut,
        QuartIn, QuartOut, QuartInOut,
        QuintIn, QuintOut, QuintInOut,
        SineIn, SineOut, SineInOut,
        ExpoIn, ExpoOut, ExpoInOut,
        CircIn, CircOut, CircInOut,
        ElasticIn, ElasticOut, ElasticInOut,
        BackIn, BackOut, BackInOut,
        BounceIn, BounceOut, BounceInOut,
    }
}
