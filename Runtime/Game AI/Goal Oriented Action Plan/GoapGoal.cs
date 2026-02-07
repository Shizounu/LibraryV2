using System;
using UnityEngine;

namespace Shizounu.Library.GameAI.GOAP
{
    /// <summary>
    /// Represents a goal in the GOAP system.
    /// Goals have desired world states and priorities.
    /// </summary>
    public class GoapGoal
    {
        private string _name;
        private float _priority;
        private WorldState _desiredState;

        public string Name => _name;
        public float Priority => _priority;
        public WorldState DesiredState => _desiredState;

        public GoapGoal(string name, float priority = 1f)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            _name = name;
            _priority = priority;
            _desiredState = new WorldState();
        }

        /// <summary>
        /// Adds a desired state condition to this goal.
        /// </summary>
        public GoapGoal WithCondition<T>(string key, T value)
        {
            _desiredState.SetValue(key, value);
            return this;
        }

        /// <summary>
        /// Sets the priority of this goal.
        /// </summary>
        public GoapGoal WithPriority(float priority)
        {
            _priority = priority;
            return this;
        }

        /// <summary>
        /// Checks if this goal is satisfied by the given world state.
        /// </summary>
        public bool IsSatisfied(WorldState currentState)
        {
            return currentState.MeetsConditions(_desiredState);
        }

        /// <summary>
        /// Checks if this goal is currently relevant.
        /// Override this in derived classes for dynamic goal selection.
        /// </summary>
        public virtual bool IsRelevant(GameObject agent)
        {
            return true;
        }

        public override string ToString()
        {
            return $"Goal: {_name} (Priority: {_priority})";
        }
    }
}
