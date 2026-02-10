using System;

namespace Shizounu.Library.RandomSystem
{
    /// <summary>
    /// Static helper class for quick one-off RNG operations without a full context.
    /// Warning: These use unpredictable seeds and are not reproducible.
    /// </summary>
    public static class QuickRng
    {
        private static readonly XorshiftRng _defaultRng;

        static QuickRng()
        {
            _defaultRng = new XorshiftRng((uint)DateTime.UtcNow.GetHashCode());
        }

        /// <summary>
        /// Gets a quick random uint32 value.
        /// </summary>
        public static uint Next()
        {
            return _defaultRng.Next();
        }

        /// <summary>
        /// Gets a quick random float [0, 1).
        /// </summary>
        public static float NextFloat()
        {
            return _defaultRng.NextFloat();
        }

        /// <summary>
        /// Gets a quick random int [min, max).
        /// </summary>
        public static int NextInt(int minValue, int maxValue)
        {
            return _defaultRng.NextInt(minValue, maxValue);
        }

        /// <summary>
        /// Gets a quick random int [0, max).
        /// </summary>
        public static int NextInt(int maxValue)
        {
            return _defaultRng.NextInt(maxValue);
        }

        /// <summary>
        /// Reseeds the quick RNG with a specific seed.
        /// </summary>
        public static void Seed(uint seed)
        {
            _defaultRng.SetSeed(seed);
        }
    }
}
