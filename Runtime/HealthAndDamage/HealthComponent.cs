using System;
using UnityEngine;
using Shizounu.Library.BuffDebuff;
using Shizounu.Library.Update;
using Shizounu.Library.ScriptableArchitecture;

namespace Shizounu.Library.HealthAndDamage
{
    /// <summary>
    /// Main health management component for game objects.
    /// Works with BuffDebuffSystem to apply damage/healing modifiers from active effects.
    /// 
    /// Usage:
    /// 1. Add this component to a game object that has a BuffDebuffSystem
    /// 2. Set MaxHealth in the inspector or via SetMaxHealth()
    /// 3. Call TakeDamage() or Heal() to modify health
    /// 4. Listen to OnHealthChanged and OnDeath events for UI updates or game logic
    /// 5. (Optional) Attach ScriptableVariable references to expose to UI systems
    /// </summary>
    public class HealthComponent : MonoBehaviour
    {
        [SerializeField]
        private float _maxHealth = 100f;

        [SerializeField]
        private float _currentHealth;

        [SerializeField]
        private bool _isDead = false;

        /// <summary>
        /// Cached reference to BuffDebuffSystem on this game object.
        /// </summary>
        private BuffDebuffSystem _buffDebuffSystem;

        /// <summary>
        /// Optional scriptable variable references for UI integration.
        /// Assign these in the inspector to automatically update UI from scriptable objects.
        /// </summary>
        [SerializeField]
        [Tooltip("Optional: ScriptableFloat for current health updates (0-max)")]
        private ScriptableFloat _currentHealthVariable;

        [SerializeField]
        [Tooltip("Optional: ScriptableFloat for max health updates")]
        private ScriptableFloat _maxHealthVariable;

        [SerializeField]
        [Tooltip("Optional: ScriptableFloat for health percentage updates (0-1)")]
        private ScriptableFloat _healthPercentVariable;

        [SerializeField]
        [Tooltip("Optional: ScriptableBool for dead state")]
        private ScriptableBool _isDeadVariable;

        // Events
        /// <summary>
        /// Fired when health changes. Parameters: (oldHealth, newHealth, maxHealth)
        /// </summary>
        public event Action<float, float, float> OnHealthChanged;

        /// <summary>
        /// Fired when health is fully restored.
        /// </summary>
        public event Action OnFullyHealed;

        /// <summary>
        /// Fired when the entity dies. Parameter: damageSource (can be null)
        /// </summary>
        public event Action<string> OnDeath;

        /// <summary>
        /// Fired when the entity revives (health goes from 0 to positive).
        /// </summary>
        public event Action OnRevived;

        /// <summary>
        /// Fired when max health changes. Parameters: (oldMaxHealth, newMaxHealth)
        /// </summary>
        public event Action<float, float> OnMaxHealthChanged;

        #region Properties

        /// <summary>
        /// Current health value. Between 0 and effective max health.
        /// </summary>
        public float CurrentHealth => _currentHealth;

        /// <summary>
        /// Base max health value (before buffs/debuffs applied).
        /// </summary>
        public float BaseMaxHealth => _maxHealth;

        /// <summary>
        /// Effective max health including all BuffDebuff modifiers.
        /// </summary>
        public float EffectiveMaxHealth
        {
            get
            {
                if (_buffDebuffSystem == null) return _maxHealth;

                float health = _maxHealth;

                // Apply flat modifiers first
                health += _buffDebuffSystem.GetModifierSum(HealthModifiers.MAX_HEALTH_FLAT);

                // Apply multiplicative modifiers
                float healthMultiplier = _buffDebuffSystem.GetModifierMultiplier(HealthModifiers.MAX_HEALTH_MULTIPLIER);
                health *= healthMultiplier;

                return Mathf.Max(1f, health); // Ensure at least 1 HP
            }
        }

        /// <summary>
        /// Health as a percentage (0-1).
        /// </summary>
        public float HealthPercent => _currentHealth / EffectiveMaxHealth;

        /// <summary>
        /// Whether this entity is currently dead.
        /// </summary>
        public bool IsDead => _isDead;

        /// <summary>
        /// Whether this entity is alive.
        /// </summary>
        public bool IsAlive => !_isDead;

        #endregion

        private void Awake()
        {
            _buffDebuffSystem = GetComponent<BuffDebuffSystem>();
            _currentHealth = _maxHealth;
            UpdateScriptableVariables();
        }

        private void OnEnable()
        {
            // Register with UpdateSystem for passive health regeneration
            Shizounu.Library.Update.UpdateSystem.Instance.RegisterCallback(OnUpdateCallback, 0f, UpdateThreading.MainThread);
        }
        private void OnDisable()
        {
            Shizounu.Library.Update.UpdateSystem.Instance.UnregisterCallback(OnUpdateCallback, 0f, UpdateThreading.MainThread);
        }

        private void OnUpdateCallback(float deltaTime, UpdateContext context)
        {
            UpdateHealthRegeneration(deltaTime);
        }

        /// <summary>
        /// Set the base max health value.
        /// </summary>
        public void SetMaxHealth(float newMaxHealth)
        {
            if (newMaxHealth <= 0)
            {
                Debug.LogWarning("Max health must be greater than 0", this);
                return;
            }

            float oldMaxHealth = _maxHealth;
            _maxHealth = newMaxHealth;

            // Clamp current health to new effective max
            _currentHealth = Mathf.Min(_currentHealth, EffectiveMaxHealth);

            OnMaxHealthChanged?.Invoke(oldMaxHealth, _maxHealth);
            UpdateScriptableVariables();
        }

        /// <summary>
        /// Apply raw damage to this entity without modifiers.
        /// Use this for environmental hazards or effects that ignore buffs.
        /// </summary>
        public float TakeDamageRaw(float damage, string damageSource = "raw")
        {
            if (damage < 0) damage = 0;
            
            return ApplyDamage(damage, damageSource, applyModifiers: false);
        }

        /// <summary>
        /// Apply damage to this entity, considering all active BuffDebuff effects.
        /// </summary>
        /// <param name="baseDamage">The base damage amount.</param>
        /// <param name="damageSource">Identifier for what caused the damage (for events).</param>
        /// <param name="damageType">Type of damage (physical, magic, true). Affects which resistance applies.</param>
        /// <returns>Actual damage applied after modifiers.</returns>
        public float TakeDamage(float baseDamage, string damageSource = "generic", DamageType damageType = DamageType.Physical)
        {
            if (baseDamage < 0) baseDamage = 0;
            if (_isDead) return 0; // Already dead, can't take more damage

            float damage = baseDamage;

            // Check for invulnerability
            if (_buffDebuffSystem != null && _buffDebuffSystem.HasModifier(HealthModifiers.INVULNERABLE))
            {
                return 0;
            }

            // Apply resistance based on damage type
            if (_buffDebuffSystem != null)
            {
                switch (damageType)
                {
                    case DamageType.Magic:
                        // Magic resistance reduces damage
                        float magicResistance = Mathf.Clamp01(_buffDebuffSystem.GetModifierSum(HealthModifiers.MAGIC_RESISTANCE));
                        damage *= (1f - magicResistance);
                        break;

                    case DamageType.Physical:
                        // Armor reduces physical damage
                        float armor = _buffDebuffSystem.GetModifierSum(HealthModifiers.ARMOR);
                        if (armor > 0)
                        {
                            // Simple armor formula: damage reduction = armor / (armor + 100)
                            float damageReduction = armor / (armor + 100f);
                            damage *= (1f - damageReduction);
                        }
                        break;

                    case DamageType.True:
                        // True damage ignores resistances
                        break;
                }
            }

            return ApplyDamage(damage, damageSource, applyModifiers: true);
        }

        /// <summary>
        /// Heal this entity by a base amount, applying healing modifiers from buffs.
        /// </summary>
        /// <param name="baseHealing">The base healing amount.</param>
        /// <param name="healingSource">Identifier for what caused the healing.</param>
        /// <returns>Actual healing applied after modifiers.</returns>
        public float Heal(float baseHealing, string healingSource = "generic")
        {
            if (baseHealing < 0) baseHealing = 0;

            float healing = baseHealing;

            // Apply healing modifiers
            if (_buffDebuffSystem != null)
            {
                // Apply multiplicative healing modifiers
                float healingMultiplier = _buffDebuffSystem.GetModifierMultiplier(HealthModifiers.HEALING_RECEIVED_MULTIPLIER);
                healing *= healingMultiplier;
            }

            float oldHealth = _currentHealth;
            _currentHealth = Mathf.Min(_currentHealth + healing, EffectiveMaxHealth);
            float actualHealing = _currentHealth - oldHealth;

            if (actualHealing > 0)
            {
                OnHealthChanged?.Invoke(oldHealth, _currentHealth, EffectiveMaxHealth);
                UpdateScriptableVariables();

                if (oldHealth <= 0 && _currentHealth > 0)
                {
                    _isDead = false;
                    OnRevived?.Invoke();
                }

                if (_currentHealth >= EffectiveMaxHealth)
                {
                    OnFullyHealed?.Invoke();
                }
            }

            return actualHealing;
        }

        /// <summary>
        /// Instantly restore health to full.
        /// </summary>
        public void FullRestore()
        {
            float oldHealth = _currentHealth;
            _currentHealth = EffectiveMaxHealth;
            OnHealthChanged?.Invoke(oldHealth, _currentHealth, EffectiveMaxHealth);
            UpdateScriptableVariables();
            
            if (oldHealth <= 0)
            {
                _isDead = false;
                OnRevived?.Invoke();
            }
        }

        /// <summary>
        /// Kill this entity immediately.
        /// </summary>
        public void Die(string damageSource = "execution")
        {
            if (_isDead) return;

            float oldHealth = _currentHealth;
            _currentHealth = 0;
            _isDead = true;

            OnHealthChanged?.Invoke(oldHealth, 0, EffectiveMaxHealth);
            UpdateScriptableVariables();
            OnDeath?.Invoke(damageSource);
        }

        /// <summary>
        /// Revive the entity with set health.
        /// </summary>
        public void Revive(float healthPercent = 1f)
        {
            if (!_isDead) return;

            float reviveHealth = EffectiveMaxHealth * Mathf.Clamp01(healthPercent);
            float oldHealth = _currentHealth;
            _currentHealth = reviveHealth;
            _isDead = false;

            OnHealthChanged?.Invoke(oldHealth, _currentHealth, EffectiveMaxHealth);
            UpdateScriptableVariables();
            OnRevived?.Invoke();
        }

        /// <summary>
        /// Get the effective damage multiplier from current buffs/debuffs.
        /// </summary>
        public float GetDamageMultiplier()
        {
            if (_buffDebuffSystem == null) return 1f;
            return _buffDebuffSystem.GetModifierMultiplier(HealthModifiers.DAMAGE_TAKEN_MULTIPLIER);
        }

        /// <summary>
        /// Get the effective healing multiplier from current buffs/debuffs.
        /// </summary>
        public float GetHealingMultiplier()
        {
            if (_buffDebuffSystem == null) return 1f;
            return _buffDebuffSystem.GetModifierMultiplier(HealthModifiers.HEALING_RECEIVED_MULTIPLIER);
        }

        /// <summary>
        /// Check if this entity can take damage (not invulnerable).
        /// </summary>
        public bool CanTakeDamage()
        {
            if (_buffDebuffSystem == null) return true;
            return !_buffDebuffSystem.HasModifier(HealthModifiers.INVULNERABLE);
        }

        private float ApplyDamage(float damage, string damageSource, bool applyModifiers)
        {
            if (damage == 0) return 0;

            float actualDamage = damage;

            // Apply damage modifiers if requested
            if (applyModifiers && _buffDebuffSystem != null)
            {
                // Apply flat damage modifications
                actualDamage += _buffDebuffSystem.GetModifierSum(HealthModifiers.DAMAGE_TAKEN_FLAT);

                // Apply multiplicative damage modifiers
                float damageMultiplier = _buffDebuffSystem.GetModifierMultiplier(HealthModifiers.DAMAGE_TAKEN_MULTIPLIER);
                actualDamage *= damageMultiplier;
            }

            actualDamage = Mathf.Max(0, actualDamage);

            float oldHealth = _currentHealth;
            _currentHealth = Mathf.Max(0, _currentHealth - actualDamage);

            OnHealthChanged?.Invoke(oldHealth, _currentHealth, EffectiveMaxHealth);
            UpdateScriptableVariables();

            // Check if dead
            if (_currentHealth <= 0 && !_isDead)
            {
                _isDead = true;
                OnDeath?.Invoke(damageSource);
            }

            return actualDamage;
        }

        private void UpdateHealthRegeneration(float deltaTime)
        {
            if (_isDead || _buffDebuffSystem == null) return;

            float regenRate = _buffDebuffSystem.GetModifierSum(HealthModifiers.HEALTH_REGENERATION);
            if (regenRate > 0)
            {
                Heal(regenRate * deltaTime, "regeneration");
            }
        }

        /// <summary>
        /// Update all linked scriptable variables with current health state.
        /// Called automatically whenever health changes.
        /// </summary>
        private void UpdateScriptableVariables()
        {
            if (_currentHealthVariable != null)
                _currentHealthVariable.RuntimeValue = _currentHealth;

            if (_maxHealthVariable != null)
                _maxHealthVariable.RuntimeValue = EffectiveMaxHealth;

            if (_healthPercentVariable != null)
                _healthPercentVariable.RuntimeValue = HealthPercent;

            if (_isDeadVariable != null)
                _isDeadVariable.RuntimeValue = _isDead;
        }
    }

    /// <summary>
    /// Damage type enum for different kinds of damage.
    /// </summary>
    public enum DamageType
    {
        Physical,  // Reduced by armor
        Magic,     // Reduced by magic resistance
        True       // Bypasses all resistances
    }
}
