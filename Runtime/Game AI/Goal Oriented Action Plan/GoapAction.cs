using System;
using UnityEngine;

namespace Shizounu.Library.GameAI.GOAP
{
    /// <summary>
    /// Abstract base class for GOAP actions.
    /// Provides default implementations and helper methods.
    /// </summary>
    public abstract class GoapAction : IGoapAction
    {
        private string _name;
        private float _cost;
        private WorldState _preconditions;
        private WorldState _effects;

        public string Name => _name;
        public float Cost => _cost;
        public WorldState Preconditions => _preconditions;
        public WorldState Effects => _effects;

        protected GoapAction(string name, float cost = 1f)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            _name = name;
            _cost = cost;
            _preconditions = new WorldState();
            _effects = new WorldState();
        }

        /// <summary>
        /// Adds a precondition to this action.
        /// </summary>
        protected void AddPrecondition<T>(string key, T value)
        {
            _preconditions.SetValue(key, value);
        }

        /// <summary>
        /// Adds an effect to this action.
        /// </summary>
        protected void AddEffect<T>(string key, T value)
        {
            _effects.SetValue(key, value);
        }

        /// <summary>
        /// Sets the cost of this action.
        /// </summary>
        protected void SetCost(float cost)
        {
            _cost = cost;
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

        public virtual IGoapAction Clone()
        {
            return (IGoapAction)MemberwiseClone();
        }

        public override string ToString()
        {
            return $"{_name} (Cost: {_cost})";
        }
    }
}
