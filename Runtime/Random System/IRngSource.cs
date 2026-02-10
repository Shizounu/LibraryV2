using System;

namespace Shizounu.Library.RandomSystem
{
    /// <summary>
    /// Interface for different RNG algorithm implementations.
    /// Allows for pluggable random number generators with reproducible results.
    /// </summary>
    public interface IRngSource
    {
        /// <summary>
        /// Gets the current seed value.
        /// </summary>
        uint Seed { get; }

        /// <summary>
        /// Seeds the RNG with a specific value for reproducibility.
        /// </summary>
        void SetSeed(uint seed);

        /// <summary>
        /// Returns the next random uint32 value.
        /// </summary>
        uint Next();

        /// <summary>
        /// Returns the next random float in range [0, 1).
        /// </summary>
        float NextFloat();

        /// <summary>
        /// Returns the next random double in range [0, 1).
        /// </summary>
        double NextDouble();

        /// <summary>
        /// Returns the next random int in range [0, maxValue).
        /// </summary>
        int NextInt(int maxValue);

        /// <summary>
        /// Returns the next random int in range [minValue, maxValue).
        /// </summary>
        int NextInt(int minValue, int maxValue);

        /// <summary>
        /// Creates a copy of this RNG source with the same internal state.
        /// </summary>
        IRngSource Clone();
    }
}
