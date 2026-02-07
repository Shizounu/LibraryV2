using System;
using UnityEngine;

namespace Shizounu.Library.GameAI.GOAP
{
    /// <summary>
    /// Abstract base class for GOAP actions.
    /// Provides default implementations and helper methods.
    /// </summary>
    public abstract class GoapAction
    {
        public string Name;
        public float Cost;
        public WorldState Preconditions;
        public WorldState Effects;

        protected GoapAction(string name, float cost = 1f)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            Name = name;
            Cost = cost;
            Preconditions = new WorldState();
            Effects = new WorldState();
        }

        /// <summary>
        /// Adds a precondition to this action.
        /// </summary>
        protected void AddPrecondition<T>(string key, T value)
        {
            Preconditions.SetValue(key, value);
        }

        /// <summary>
        /// Adds an effect to this action.
        /// </summary>
        protected void AddEffect<T>(string key, T value)
        {
            Effects.SetValue(key, value);
        }

        /// <summary>
        /// Sets the cost of this action.
        /// </summary>
        protected void SetCost(float cost)
        {
            Cost = cost;
        }

        public virtual bool IsAvailable(GameObject agent)
        {
            return true;
        }

        public virtual void OnEnter(GameObject agent)
        {
        }

        public abstract bool Execute(GameObject agent);

        public virtual void OnExit(GameObject agent)
        {
        }

        public override string ToString()
        {
            return $"{Name} (Cost: {Cost})";
        }
    }
}
