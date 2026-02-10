using System;

namespace Shizounu.Library.RandomSystem
{
    /// <summary>
    /// Wrapper that implements System.Random using an RngUser for reproducible generation.
    /// This allows using the RNG system with APIs that expect System.Random.
    /// </summary>
    public class RngSystemRandom : Random
    {
        private readonly RngUser _rngUser;

        /// <summary>
        /// Creates a new Random wrapper using the specified RngUser.
        /// </summary>
        public RngSystemRandom(RngUser rngUser)
        {
            _rngUser = rngUser ?? throw new ArgumentNullException(nameof(rngUser));
        }

        /// <summary>
        /// Returns a non-negative random integer.
        /// </summary>
        public override int Next()
        {
            return (int)(_rngUser.Next() & 0x7FFFFFFF);
        }

        /// <summary>
        /// Returns a non-negative random integer that is less than the specified maximum.
        /// </summary>
        public override int Next(int maxValue)
        {
            if (maxValue < 0)
                throw new ArgumentOutOfRangeException(nameof(maxValue));

            if (maxValue == 0)
                return 0;

            return _rngUser.NextInt(0, maxValue);
        }

        /// <summary>
        /// Returns a random integer that is within a specified range.
        /// </summary>
        public override int Next(int minValue, int maxValue)
        {
            if (minValue > maxValue)
                throw new ArgumentOutOfRangeException(nameof(minValue));

            return _rngUser.NextInt(minValue, maxValue);
        }

        /// <summary>
        /// Returns a random floating-point number between 0.0 and 1.0.
        /// </summary>
        public override double NextDouble()
        {
            return _rngUser.NextFloat();
        }

        /// <summary>
        /// Fills the elements of a specified array of bytes with random numbers.
        /// </summary>
        public override void NextBytes(byte[] buffer)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));

            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = (byte)(_rngUser.Next() & 0xFF);
            }
        }
    }
}
