using System;
using Shizounu.Library.RandomSystem;

namespace Shizounu.Library.GenerationAlgorithms.Shared
{
    /// <summary>
    /// Helper utilities for integrating the Random System with generation algorithms.
    /// Provides convenience methods to create and configure RNG sources.
    /// </summary>
    public static class GenerationRng
    {
        /// <summary>
        /// Creates an RNG source from a seed parameter.
        /// Uses XorshiftRng for performance. If seed is negative, generates a time-based seed.
        /// </summary>
        /// <param name="seed">Seed value, or negative for auto-seed</param>
        /// <returns>Configured RNG source</returns>
        public static IRngSource Create(int seed = -1)
        {
            if (seed < 0)
            {
                seed = Environment.TickCount;
            }
            return new XorshiftRng((uint)seed);
        }

        /// <summary>
        /// Creates an RNG source from an unsigned seed.
        /// </summary>
        public static IRngSource Create(uint seed)
        {
            return new XorshiftRng(seed);
        }

        /// <summary>
        /// Creates an RNG source from a nullable seed.
        /// If null, generates a time-based seed.
        /// </summary>
        public static IRngSource Create(int? seed)
        {
            return Create(seed ?? -1);
        }

        /// <summary>
        /// Creates an RNG source, or returns the provided one if not null.
        /// </summary>
        public static IRngSource CreateOrUse(IRngSource existing, int seed = -1)
        {
            return existing ?? Create(seed);
        }
    }
}
