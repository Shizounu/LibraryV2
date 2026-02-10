namespace Shizounu.Library.RandomSystem
{
    /// <summary>
    /// Xorshift-based RNG implementation for fast, reproducible random number generation.
    /// Provides good distribution and performance for game-like applications.
    /// Based on Xorshift32 algorithm.
    /// </summary>
    public class XorshiftRng : IRngSource
    {
        private uint _state;

        /// <summary>
        /// Gets the current seed value.
        /// </summary>
        public uint Seed => _state;

        /// <summary>
        /// Initializes a new Xorshift RNG with a default seed.
        /// </summary>
        public XorshiftRng() : this(2463534242u)
        {
        }

        /// <summary>
        /// Initializes a new Xorshift RNG with a specific seed.
        /// </summary>
        public XorshiftRng(uint seed)
        {
            SetSeed(seed);
        }

        /// <summary>
        /// Seeds the RNG with a specific value for reproducibility.
        /// </summary>
        public void SetSeed(uint seed)
        {
            // Ensure seed is not zero (required for Xorshift)
            _state = seed != 0 ? seed : 2463534242u;
        }

        /// <summary>
        /// Returns the next random uint32 value using Xorshift algorithm.
        /// </summary>
        public uint Next()
        {
            _state ^= _state << 13;
            _state ^= _state >> 17;
            _state ^= _state << 5;
            return _state;
        }

        /// <summary>
        /// Returns the next random float in range [0, 1).
        /// </summary>
        public float NextFloat()
        {
            return (Next() >> 8) * (1.0f / 16777216.0f);
        }

        /// <summary>
        /// Returns the next random double in range [0, 1).
        /// </summary>
        public double NextDouble()
        {
            return (Next() >> 8) * (1.0 / 16777216.0);
        }

        /// <summary>
        /// Returns the next random int in range [0, maxValue).
        /// </summary>
        public int NextInt(int maxValue)
        {
            if (maxValue <= 0)
                throw new System.ArgumentException("maxValue must be greater than 0", nameof(maxValue));

            return (int)(NextFloat() * maxValue);
        }

        /// <summary>
        /// Returns the next random int in range [minValue, maxValue).
        /// </summary>
        public int NextInt(int minValue, int maxValue)
        {
            if (maxValue <= minValue)
                throw new System.ArgumentException("maxValue must be greater than minValue");

            return minValue + NextInt(maxValue - minValue);
        }

        /// <summary>
        /// Creates a copy of this RNG source with the same internal state.
        /// </summary>
        public IRngSource Clone()
        {
            return new XorshiftRng(_state);
        }
    }
}
