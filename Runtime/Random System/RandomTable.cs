using System;
using System.Collections.Generic;

namespace Shizounu.Library.RandomSystem
{
    /// <summary>
    /// Helper class for weighted random selection from a table of options.
    /// </summary>
    public class RandomTable<T>
    {
        private readonly List<(T Value, float Weight)> _entries = new List<(T, float)>();
        private float _totalWeight = 0;
        private readonly RngUser _rng;

        /// <summary>
        /// Gets the number of entries in the table.
        /// </summary>
        public int Count => _entries.Count;

        /// <summary>
        /// Creates a new RandomTable using the specified RNG user.
        /// </summary>
        public RandomTable(RngUser rng)
        {
            _rng = rng ?? throw new ArgumentNullException(nameof(rng));
        }

        /// <summary>
        /// Adds an entry with equal weight (1.0f).
        /// </summary>
        public void Add(T value)
        {
            Add(value, 1.0f);
        }

        /// <summary>
        /// Adds an entry with the specified weight.
        /// </summary>
        public void Add(T value, float weight)
        {
            if (weight < 0)
                throw new ArgumentException("Weight must be non-negative");

            _entries.Add((value, weight));
            _totalWeight += weight;
        }

        /// <summary>
        /// Selects a random entry from the table based on weights.
        /// </summary>
        public T Select(string label = "random_select")
        {
            if (_entries.Count == 0)
                throw new InvalidOperationException("Cannot select from empty table");

            if (_totalWeight <= 0)
                throw new InvalidOperationException("Total weight must be positive");

            float roll = _rng.NextFloat(label) * _totalWeight;
            float accumulated = 0;

            foreach (var (value, weight) in _entries)
            {
                accumulated += weight;
                if (roll <= accumulated)
                    return value;
            }

            // Fallback to last entry (shouldn't reach here)
            return _entries[_entries.Count - 1].Value;
        }

        /// <summary>
        /// Selects N random entries with replacement.
        /// </summary>
        public List<T> SelectMultiple(int count, string label = "random_select_multi")
        {
            var results = new List<T>();
            for (int i = 0; i < count; i++)
            {
                results.Add(Select($"{label}_{i}"));
            }
            return results;
        }

        /// <summary>
        /// Clears all entries from the table.
        /// </summary>
        public void Clear()
        {
            _entries.Clear();
            _totalWeight = 0;
        }
    }
}
