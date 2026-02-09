using UnityEngine;
using Shizounu.Library.BuffDebuff;

namespace Shizounu.Library.HealthAndDamage
{
    /// <summary>
    /// Helper component for dealing damage to other entities.
    /// Calculates damage output considering buffs/debuffs on the attacker.
    /// Add this to the attacker's GameObject alongside BuffDebuffSystem.
    /// </summary>
    public class DamageDealer : MonoBehaviour
    {
        private BuffDebuffSystem _buffDebuffSystem;

        [SerializeField]
        private float _baseDamage = 10f;

        private void Awake()
        {
            _buffDebuffSystem = GetComponent<BuffDebuffSystem>();
        }

        /// <summary>
        /// Get the effective damage output after applying all buffs/debuffs.
        /// </summary>
        public float GetEffectiveDamage()
        {
            if (_buffDebuffSystem == null)
                return _baseDamage;

            float damage = _baseDamage;

            // Apply flat damage bonuses
            damage += _buffDebuffSystem.GetModifierSum(CommonModifiers.DAMAGE_FLAT);

            // Apply multiplicative damage bonuses
            float damageMultiplier = _buffDebuffSystem.GetModifierMultiplier(CommonModifiers.DAMAGE_MULTIPLIER);
            damage *= damageMultiplier;

            return Mathf.Max(0, damage);
        }

        /// <summary>
        /// Set the base damage value.
        /// </summary>
        public void SetBaseDamage(float damage)
        {
            _baseDamage = Mathf.Max(0, damage);
        }

        /// <summary>
        /// Deal damage to a target, applying both attacker's damage buffs and target's resistances.
        /// </summary>
        /// <param name="target">The target GameObject with HealthComponent.</param>
        /// <param name="damageType">Type of damage being dealt.</param>
        /// <returns>Actual damage applied after all modifiers.</returns>
        public float DealDamageToTarget(GameObject target, DamageType damageType = DamageType.Physical)
        {
            if (target == null)
            {
                Debug.LogWarning("Attempting to deal damage to null target", this);
                return 0;
            }

            HealthComponent healthComponent = target.GetComponent<HealthComponent>();
            if (healthComponent == null)
            {
                Debug.LogWarning($"Target {target.name} does not have a HealthComponent", this);
                return 0;
            }

            float effectiveDamage = GetEffectiveDamage();
            float actualDamage = healthComponent.TakeDamage(effectiveDamage, gameObject.name, damageType);

            // Apply lifesteal if attacker has it
            if (_buffDebuffSystem != null)
            {
                float lifestealPercent = _buffDebuffSystem.GetModifierSum(HealthModifiers.LIFESTEAL_PERCENT);
                if (lifestealPercent > 0)
                {
                    float healAmount = actualDamage * Mathf.Clamp01(lifestealPercent);
                    Heal(healAmount, "lifesteal");
                }
            }

            return actualDamage;
        }

        /// <summary>
        /// Deal custom damage amount to target.
        /// </summary>
        public float DealDamageToTarget(GameObject target, float customDamage, DamageType damageType = DamageType.Physical)
        {
            if (target == null)
            {
                Debug.LogWarning("Attempting to deal damage to null target", this);
                return 0;
            }

            HealthComponent healthComponent = target.GetComponent<HealthComponent>();
            if (healthComponent == null)
            {
                Debug.LogWarning($"Target {target.name} does not have a HealthComponent", this);
                return 0;
            }

            return healthComponent.TakeDamage(customDamage, gameObject.name, damageType);
        }

        /// <summary>
        /// Heal this damage dealer (if it has a HealthComponent).
        /// </summary>
        public float Heal(float baseHealing, string healingSource = "generic")
        {
            HealthComponent healthComponent = GetComponent<HealthComponent>();
            if (healthComponent == null)
                return 0;

            return healthComponent.Heal(baseHealing, healingSource);
        }

        /// <summary>
        /// Check if attacker has a damage bonus from buffs.
        /// </summary>
        public bool HasDamageBonus()
        {
            if (_buffDebuffSystem == null) return false;
            return _buffDebuffSystem.HasModifier(CommonModifiers.DAMAGE_MULTIPLIER) ||
                   _buffDebuffSystem.HasModifier(CommonModifiers.DAMAGE_FLAT);
        }
    }
}
