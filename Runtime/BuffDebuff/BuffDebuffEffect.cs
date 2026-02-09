using UnityEngine;

namespace Shizounu.Library.BuffDebuff
{
    /// <summary>
    /// Abstract base class for buff/debuff effects.
    /// Inherit from this to create custom effects.
    /// </summary>
    public abstract class BuffDebuffEffect : IBuffDebuffEffect
    {
        protected string _effectID;
        protected float _duration;
        protected float _timeRemaining;
        protected int _stackCount = 1;
        protected int _maxStackCount = 1;
        protected bool _isActive = true;

        public string EffectID => _effectID;
        public float TimeRemaining => _timeRemaining;
        public bool IsActive => _isActive;
        public int StackCount => _stackCount;

        /// <summary>
        /// Initialize a new effect.
        /// </summary>
        /// <param name="effectID">Unique identifier for this effect.</param>
        /// <param name="duration">Duration in seconds. Use negative value for infinite duration.</param>
        /// <param name="maxStackCount">Maximum number of stacks. Default is 1 (no stacking).</param>
        public BuffDebuffEffect(string effectID, float duration, int maxStackCount = 1)
        {
            _effectID = effectID;
            _duration = duration;
            _timeRemaining = duration;
            _maxStackCount = Mathf.Max(1, maxStackCount);
        }

        public virtual void OnApply()
        {
            // Override in subclasses for custom apply logic
        }

        public virtual void OnRemove()
        {
            _isActive = false;
            // Override in subclasses for custom removal logic
        }

        public virtual void OnUpdate(float deltaTime)
        {
            // Only update duration if it's finite
            if (_duration >= 0)
            {
                _timeRemaining -= deltaTime;
                if (_timeRemaining <= 0)
                {
                    _isActive = false;
                }
            }
        }

        public abstract IStatModifier[] GetModifiers();

        public virtual IStatModifier GetModifier(string modifierID)
        {
            var modifiers = GetModifiers();
            foreach (var modifier in modifiers)
            {
                if (modifier.ModifierID == modifierID)
                    return modifier;
            }
            return null;
        }

        public virtual bool TryAddStack()
        {
            if (_stackCount < _maxStackCount)
            {
                _stackCount++;
                return true;
            }
            return false;
        }

        public virtual void RefreshDuration()
        {
            _timeRemaining = _duration;
        }
    }
}
