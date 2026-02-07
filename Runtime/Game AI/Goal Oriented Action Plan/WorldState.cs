using System;
using System.Collections.Generic;
using System.Linq;

namespace Shizounu.Library.GameAI.GOAP
{
    /// <summary>
    /// Represents the state of the world as a collection of key-value pairs.
    /// Used for GOAP planning to represent current state, goal state, preconditions, and effects.
    /// Utilizes the Blackboard system internally for data storage.
    /// </summary>
    public class WorldState
    {
        private readonly Blackboard _blackboard;

        public WorldState()
        {
            _blackboard = new SimpleBlackboard();
        }

        public WorldState(WorldState other)
        {
            // Deep copy the blackboard to prevent shared references
            _blackboard = other._blackboard.DeepCopy();
        }

        /// <summary>
        /// Creates a WorldState that wraps an existing blackboard.
        /// Useful for sharing state with other AI systems.
        /// </summary>
        public WorldState(Blackboard blackboard)
        {
            _blackboard = blackboard ?? throw new ArgumentNullException(nameof(blackboard));
        }

        /// <summary>
        /// Gets the underlying blackboard. Useful for advanced scenarios.
        /// </summary>
        public Blackboard Blackboard => _blackboard;

        /// <summary>
        /// Sets a value in the world state.
        /// </summary>
        public void SetValue<T>(string key, T value)
        {
            _blackboard.SetValue(key, value);
        }

        /// <summary>
        /// Gets a value from the world state.
        /// </summary>
        public T GetValue<T>(string key)
        {
            return _blackboard.GetValue<T>(key);
        }

        /// <summary>
        /// Tries to get a value from the world state.
        /// </summary>
        public bool TryGetValue<T>(string key, out T value)
        {
            return _blackboard.TryGetValue(key, out value);
        }

        /// <summary>
        /// Checks if the world state contains a key.
        /// </summary>
        public bool HasKey(string key)
        {
            return _blackboard.HasKey(key);
        }

        /// <summary>
        /// Removes a key from the world state.
        /// </summary>
        public bool RemoveKey(string key)
        {
            return _blackboard.RemoveValue(key);
        }

        /// <summary>
        /// Clears all values from the world state.
        /// </summary>
        public void Clear()
        {
            _blackboard.Clear();
        }

        /// <summary>
        /// Checks if this world state meets all the conditions specified in the target state.
        /// </summary>
        public bool MeetsConditions(WorldState targetState)
        {
            if (targetState == null)
                return true;

            foreach (var condition in targetState._blackboard.GetAllEntries())
            {
                if (!_blackboard.TryGetValue<object>(condition.Key, out var value))
                    return false;

                if (!value.Equals(condition.Value))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Applies the effects of another world state to this one.
        /// </summary>
        public void ApplyEffects(WorldState effects)
        {
            if (effects == null)
                return;

            foreach (var effect in effects._blackboard.GetAllEntries())
            {
                _blackboard.SetValue(effect.Key, effect.Value);
            }
        }

        /// <summary>
        /// Creates a copy of this world state.
        /// </summary>
        public WorldState Clone()
        {
            return new WorldState(this);
        }

        /// <summary>
        /// Gets all keys in the world state.
        /// </summary>
        public IEnumerable<string> GetAllKeys()
        {
            return _blackboard.GetAllKeys();
        }

        /// <summary>
        /// Gets all entries in the world state.
        /// </summary>
        public IEnumerable<KeyValuePair<string, object>> GetAllEntries()
        {
            return _blackboard.GetAllEntries();
        }

        /// <summary>
        /// Gets the number of entries in the world state.
        /// </summary>
        public int Count => _blackboard.GetAllKeys().Count();

        public override string ToString()
        {
            var entries = _blackboard.GetAllEntries().ToList();
            
            if (entries.Count == 0)
                return "WorldState: (empty)";

            var items = entries.Select(kvp => $"{kvp.Key}={kvp.Value}");
            return $"WorldState: {string.Join(", ", items)}";
        }
    }
}
