using UnityEngine;

namespace Shizounu.Library.Utility
{
    public static class NumericExtensions
    {
        /// <summary>
        /// Check if value is between min and max (inclusive).
        /// </summary>
        public static bool Between(this int value, int min, int max)
        {
            return value >= min && value <= max;
        }

        /// <summary>
        /// Check if value is between min and max (inclusive).
        /// </summary>
        public static bool Between(this float value, float min, float max)
        {
            return value >= min && value <= max;
        }

        /// <summary>
        /// Clamp value between min and max.
        /// </summary>
        public static int Clamp(this int value, int min, int max)
        {
            return Mathf.Clamp(value, min, max);
        }

        /// <summary>
        /// Clamp value between min and max.
        /// </summary>
        public static float Clamp(this float value, float min, float max)
        {
            return Mathf.Clamp(value, min, max);
        }

        /// <summary>
        /// Clamp value between 0 and 1.
        /// </summary>
        public static float Clamp01(this float value)
        {
            return Mathf.Clamp01(value);
        }

        /// <summary>
        /// Check if float is approximately zero.
        /// </summary>
        public static bool IsApproximatelyZero(this float value, float tolerance = 0.0001f)
        {
            return Mathf.Abs(value) < tolerance;
        }

        /// <summary>
        /// Check if float is approximately equal to another float.
        /// </summary>
        public static bool IsApproximately(this float value, float other, float tolerance = 0.0001f)
        {
            return Mathf.Abs(value - other) < tolerance;
        }

        /// <summary>
        /// Remap value from one range to another.
        /// </summary>
        public static float Remap(this float value, float fromMin, float fromMax, float toMin, float toMax)
        {
            return (value - fromMin) / (fromMax - fromMin) * (toMax - toMin) + toMin;
        }

        /// <summary>
        /// Get the sign of the value (-1, 0, or 1).
        /// </summary>
        public static int Sign(this float value)
        {
            if (value > 0) return 1;
            if (value < 0) return -1;
            return 0;
        }

        /// <summary>
        /// Convert degrees to radians.
        /// </summary>
        public static float ToRadians(this float degrees)
        {
            return degrees * Mathf.Deg2Rad;
        }

        /// <summary>
        /// Convert radians to degrees.
        /// </summary>
        public static float ToDegrees(this float radians)
        {
            return radians * Mathf.Rad2Deg;
        }

        /// <summary>
        /// Square the value.
        /// </summary>
        public static float Squared(this float value)
        {
            return value * value;
        }

        /// <summary>
        /// Square the value.
        /// </summary>
        public static int Squared(this int value)
        {
            return value * value;
        }
    }
}
