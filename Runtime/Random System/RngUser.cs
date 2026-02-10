using System;

namespace Shizounu.Library.RandomSystem
{
    /// <summary>
    /// Represents an individual user/consumer of random numbers from a specific RNG source.
    /// Each user pulls from their own RNG source, allowing different sequences
    /// while still being part of the same reproducible system.
    /// </summary>
    public class RngUser
    {
        /// <summary>
        /// Unique identifier for this RNG user.
        /// </summary>
        public int Id { get; private set; }

        /// <summary>
        /// Descriptive name for this user (e.g., "PlayerAttackRoll", "EnemyAI").
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// The RNG source this user pulls from.
        /// </summary>
        public IRngSource Source { get; private set; }

        /// <summary>
        /// Reference to the parent RNG context for history recording.
        /// </summary>
        private RngContext _context;

        /// <summary>
        /// Counter for tracking the number of values this user has generated.
        /// </summary>
        private int _generationCount = 0;

        /// <summary>
        /// Gets the number of random values this user has generated.
        /// </summary>
        public int GenerationCount => _generationCount;

        /// <summary>
        /// Creates a new RNG user with the specified ID, name, and RNG source.
        /// </summary>
        public RngUser(int id, string name, IRngSource source, RngContext context = null)
        {
            Id = id;
            Name = name;
            Source = source ?? throw new ArgumentNullException(nameof(source));
            _context = context;
        }

        /// <summary>
        /// Gets the next random uint32 value from this user's RNG source.
        /// </summary>
        public uint Next(string label = "")
        {
            uint stateBefore = Source.Seed;
            uint value = Source.Next();
            uint stateAfter = Source.Seed;

            Record(value, stateBefore, stateAfter, label);
            return value;
        }

        /// <summary>
        /// Gets the next random float in range [0, 1) from this user's RNG source.
        /// </summary>
        public float NextFloat(string label = "")
        {
            uint stateBefore = Source.Seed;
            float value = Source.NextFloat();
            uint stateAfter = Source.Seed;

            Record((uint)(value * uint.MaxValue), stateBefore, stateAfter, label);
            return value;
        }

        /// <summary>
        /// Gets the next random double in range [0, 1) from this user's RNG source.
        /// </summary>
        public double NextDouble(string label = "")
        {
            uint stateBefore = Source.Seed;
            double value = Source.NextDouble();
            uint stateAfter = Source.Seed;

            Record((uint)(value * uint.MaxValue), stateBefore, stateAfter, label);
            return value;
        }

        /// <summary>
        /// Gets the next random int in range [0, maxValue) from this user's RNG source.
        /// </summary>
        public int NextInt(int maxValue, string label = "")
        {
            uint stateBefore = Source.Seed;
            int value = Source.NextInt(maxValue);
            uint stateAfter = Source.Seed;

            Record((uint)value, stateBefore, stateAfter, label);
            return value;
        }

        /// <summary>
        /// Gets the next random int in range [minValue, maxValue) from this user's RNG source.
        /// </summary>
        public int NextInt(int minValue, int maxValue, string label = "")
        {
            uint stateBefore = Source.Seed;
            int value = Source.NextInt(minValue, maxValue);
            uint stateAfter = Source.Seed;

            Record((uint)value, stateBefore, stateAfter, label);
            return value;
        }

        /// <summary>
        /// Resets this user's RNG source to its initial state.
        /// </summary>
        public void ResetSource()
        {
            _generationCount = 0;
            // Note: We keep the source as-is; if you need to reset to original seed,
            // store that separately or create a new source.
        }

        /// <summary>
        /// Creates a snapshot of this user's current RNG state.
        /// </summary>
        public RngSnapshot CreateSnapshot(string label = "")
        {
            var snapshot = new RngSnapshot(label);
            snapshot.RecordUserState(Id, Source);
            return snapshot;
        }

        /// <summary>
        /// Restores this user's RNG state from a snapshot.
        /// </summary>
        public bool RestoreFromSnapshot(RngSnapshot snapshot)
        {
            if (snapshot.TryGetUserState(Id, out var savedSource))
            {
                Source = savedSource.Clone();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Gets a string representation of this RNG user.
        /// </summary>
        public override string ToString()
        {
            return $"RngUser({Id}, {Name}) - Generated: {_generationCount} values, Seed: {Source.Seed}";
        }

        private void Record(uint value, uint stateBefore, uint stateAfter, string label)
        {
            _generationCount++;
            _context?.RecordGeneration(Id, value, stateBefore, stateAfter, label ?? string.Empty);
        }
    }
}
