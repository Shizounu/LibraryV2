using System;
using System.Collections.Generic;
using UnityEngine;
using Shizounu.Library.Update;

namespace Shizounu.Library.BuffDebuff
{
    /// <summary>
    /// Main buff/debuff system component that sits on a game object.
    /// Other systems can query this to get stat modifiers or register callbacks.
    /// </summary>
    public class BuffDebuffSystem : MonoBehaviour
    {
        [System.Serializable]
        public class EffectList
        {
            public List<IBuffDebuffEffect> effects = new List<IBuffDebuffEffect>();
        }

        private Dictionary<string, IBuffDebuffEffect> _activeEffects = new Dictionary<string, IBuffDebuffEffect>();
        
        /// <summary>
        /// How frequently to update effects (in seconds). Default is every frame.
        /// </summary>
        [SerializeField]
        private float _updateInterval = 0f;
        
        // Events for external systems to react to effect changes
        public event Action<IBuffDebuffEffect> OnEffectAdded;
        public event Action<IBuffDebuffEffect> OnEffectRemoved;
        public event Action<IBuffDebuffEffect> OnEffectStacked;

        private void OnEnable()
        {
            // Register with custom UpdateSystem
            Shizounu.Library.Update.UpdateSystem.Instance.RegisterCallback(OnUpdateCallback, _updateInterval, UpdateThreading.MainThread);
        }

        private void OnDisable()
        {
            // Unregister from custom UpdateSystem
            Shizounu.Library.Update.UpdateSystem.Instance.UnregisterCallback(OnUpdateCallback, _updateInterval, UpdateThreading.MainThread);
        }

        /// <summary>
        /// Callback for the custom UpdateSystem.
        /// </summary>
        private void OnUpdateCallback(float deltaTime, UpdateContext context)
        {
            UpdateEffects(deltaTime);
        }

        /// <summary>
        /// Get or set the update interval for effect processing (in seconds).
        /// 0 = every frame, positive value = update at specified interval.
        /// </summary>
        public float UpdateInterval
        {
            get => _updateInterval;
            set
            {
                if (_updateInterval != value)
                {
                    // Unregister with old interval
                    if (gameObject.activeInHierarchy)
                    {
                        Shizounu.Library.Update.UpdateSystem.Instance.UnregisterCallback(OnUpdateCallback, _updateInterval, UpdateThreading.MainThread);
                    }

                    _updateInterval = value;

                    // Register with new interval
                    if (gameObject.activeInHierarchy)
                    {
                        Shizounu.Library.Update.UpdateSystem.Instance.RegisterCallback(OnUpdateCallback, _updateInterval, UpdateThreading.MainThread);
                    }
                }
            }
        }

        /// <summary>
        /// Add a new effect to this object.
        /// </summary>
        /// <param name="effect">The effect to add.</param>
        /// <returns>The effect that was added or the existing effect if stacking occurred.</returns>
        public IBuffDebuffEffect AddEffect(IBuffDebuffEffect effect)
        {
            if (effect == null)
            {
                Debug.LogWarning("Attempting to add null effect to BuffDebuffSystem", this);
                return null;
            }

            string effectID = effect.EffectID;

            // Check if effect already exists and try to stack
            if (_activeEffects.TryGetValue(effectID, out IBuffDebuffEffect existingEffect))
            {
                if (existingEffect.TryAddStack())
                {
                    existingEffect.RefreshDuration();
                    OnEffectStacked?.Invoke(existingEffect);
                    return existingEffect;
                }
                else
                {
                    // Can't stack, refresh duration instead
                    existingEffect.RefreshDuration();
                    return existingEffect;
                }
            }

            // New effect
            effect.OnApply();
            _activeEffects[effectID] = effect;
            OnEffectAdded?.Invoke(effect);

            return effect;
        }

        /// <summary>
        /// Remove a specific effect by ID.
        /// </summary>
        /// <param name="effectID">The ID of the effect to remove.</param>
        /// <returns>Whether the effect was found and removed.</returns>
        public bool RemoveEffect(string effectID)
        {
            if (_activeEffects.TryGetValue(effectID, out IBuffDebuffEffect effect))
            {
                effect.OnRemove();
                _activeEffects.Remove(effectID);
                OnEffectRemoved?.Invoke(effect);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Remove all active effects.
        /// </summary>
        public void ClearAllEffects()
        {
            var effectsToRemove = new List<string>(_activeEffects.Keys);
            foreach (var effectID in effectsToRemove)
            {
                RemoveEffect(effectID);
            }
        }

        /// <summary>
        /// Get a specific effect by ID.
        /// </summary>
        public IBuffDebuffEffect GetEffect(string effectID)
        {
            _activeEffects.TryGetValue(effectID, out IBuffDebuffEffect effect);
            return effect;
        }

        /// <summary>
        /// Get all active effects.
        /// </summary>
        public IBuffDebuffEffect[] GetAllEffects()
        {
            var effects = new IBuffDebuffEffect[_activeEffects.Count];
            _activeEffects.Values.CopyTo(effects, 0);
            return effects;
        }

        /// <summary>
        /// Get all modifiers of a specific type across all active effects.
        /// </summary>
        /// <param name="modifierID">The ID of the modifier type to get.</param>
        /// <returns>Array of all matching modifiers.</returns>
        public IStatModifier[] GetModifiers(string modifierID)
        {
            var modifiers = new List<IStatModifier>();

            foreach (var effect in _activeEffects.Values)
            {
                if (effect.IsActive)
                {
                    var modifier = effect.GetModifier(modifierID);
                    if (modifier != null)
                    {
                        modifiers.Add(modifier);
                    }
                }
            }

            return modifiers.ToArray();
        }

        /// <summary>
        /// Get the total value of all modifiers of a specific type.
        /// Useful for additive modifiers (like damage multipliers or movement speed bonuses).
        /// </summary>
        /// <param name="modifierID">The ID of the modifier type.</param>
        /// <returns>Sum of all modifier values.</returns>
        public float GetModifierSum(string modifierID)
        {
            float sum = 0;
            foreach (var modifier in GetModifiers(modifierID))
            {
                sum += modifier.Value;
            }
            return sum;
        }

        /// <summary>
        /// Get the combined multiplier from all modifiers of a specific type.
        /// Useful for multiplicative modifiers (like damage multipliers).
        /// Each modifier is treated as (1 + value).
        /// </summary>
        /// <param name="modifierID">The ID of the modifier type.</param>
        /// <returns>Product of all (1 + modifier.Value).</returns>
        public float GetModifierMultiplier(string modifierID)
        {
            float multiplier = 1f;
            foreach (var modifier in GetModifiers(modifierID))
            {
                multiplier *= (1f + modifier.Value);
            }
            return multiplier;
        }

        /// <summary>
        /// Check if any active effects have a specific modifier type.
        /// </summary>
        public bool HasModifier(string modifierID)
        {
            foreach (var effect in _activeEffects.Values)
            {
                if (effect.IsActive && effect.GetModifier(modifierID) != null)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Get the number of active effects.
        /// </summary>
        public int GetActiveEffectCount()
        {
            return _activeEffects.Count;
        }

        private void UpdateEffects(float deltaTime)
        {
            var effectsToRemove = new List<string>();

            foreach (var kvp in _activeEffects)
            {
                var effect = kvp.Value;
                effect.OnUpdate(deltaTime);

                if (!effect.IsActive)
                {
                    effectsToRemove.Add(kvp.Key);
                }
            }

            // Remove expired effects
            foreach (var effectID in effectsToRemove)
            {
                RemoveEffect(effectID);
            }
        }
    }
}
