using UnityEngine;
using Shizounu.Library.UpdateSystem;

namespace Shizounu.Library.BuffDebuff.Examples
{
    /// <summary>
    /// Example demonstrating how to use the BuffDebuffSystem.
    /// Add this component to a game object alongside BuffDebuffSystem to see it in action.
    /// </summary>
    public class BuffDebuffSystemExample : MonoBehaviour
    {
        private BuffDebuffSystem _buffDebuffSystem;

        private void Start()
        {
            _buffDebuffSystem = GetComponent<BuffDebuffSystem>();
            if (_buffDebuffSystem == null)
            {
                Debug.LogError("BuffDebuffSystemExample requires BuffDebuffSystem component!");
                return;
            }

            // Subscribe to events
            _buffDebuffSystem.OnEffectAdded += OnEffectAdded;
            _buffDebuffSystem.OnEffectRemoved += OnEffectRemoved;
            _buffDebuffSystem.OnEffectStacked += OnEffectStacked;

            // Example: Apply some buffs and debuffs
            ExampleAddBuffs();
        }

        private void ExampleAddBuffs()
        {
            // Add a damage boost buff (5 seconds, 0.3 = 30% damage increase)
            var damageBoost = new DamageBoostEffect("boost_damage", 5f, 0.3f, maxStacks: 3);
            _buffDebuffSystem.AddEffect(damageBoost);

            // Add a movement speed buff (10 seconds, 0.5 = 50% speed increase)
            var speedBoost = new MovementSpeedEffect("boost_speed", 10f, 0.5f, maxStacks: 2);
            _buffDebuffSystem.AddEffect(speedBoost);

            // Add armor (infinite duration)
            var armor = new ArmorEffect("buff_armor", -1f, 25f);
            _buffDebuffSystem.AddEffect(armor);

            // Add a multi-buff that provides both damage and speed
            var heroic = new HeroicBoostEffect("boost_heroic", 7f, damageBuff: 0.4f, speedBuff: 0.3f);
            _buffDebuffSystem.AddEffect(heroic);
        }

        private void Update()
        {
            // Example: Check damage multiplier from all active effects
            float damageMultiplier = _buffDebuffSystem.GetModifierMultiplier(CommonModifiers.DAMAGE_MULTIPLIER);
            Debug.Log($"Current Damage Multiplier: {damageMultiplier}");

            // Example: Get total armor from all sources
            float totalArmor = _buffDebuffSystem.GetModifierSum(CommonModifiers.ARMOR_FLAT);
            Debug.Log($"Total Armor: {totalArmor}");

            // Example: Check if any effects give movement speed bonus
            if (_buffDebuffSystem.HasModifier(CommonModifiers.MOVEMENT_SPEED_MULTIPLIER))
            {
                float speedMultiplier = _buffDebuffSystem.GetModifierMultiplier(CommonModifiers.MOVEMENT_SPEED_MULTIPLIER);
                Debug.Log($"Movement Speed Multiplier: {speedMultiplier}");
            }
        }

        private void OnEffectAdded(IBuffDebuffEffect effect)
        {
            Debug.Log($"Effect Added: {effect.EffectID}");
        }

        private void OnEffectRemoved(IBuffDebuffEffect effect)
        {
            Debug.Log($"Effect Removed: {effect.EffectID}");
        }

        private void OnEffectStacked(IBuffDebuffEffect effect)
        {
            Debug.Log($"Effect Stacked: {effect.EffectID}, Stack Count: {effect.StackCount}");
        }
    }

    /// <summary>
    /// Example implementation: A damage system that uses BuffDebuffSystem to modify damage.
    /// </summary>
    public class DamageSystemExample : MonoBehaviour
    {
        private BuffDebuffSystem _targetBuffDebuffSystem;

        /// <summary>
        /// Calculate actual damage dealt after applying all modifiers from buffs/debuffs.
        /// </summary>
        public float CalculateDamage(float baseDamage)
        {
            if (_targetBuffDebuffSystem == null)
                return baseDamage;

            float damage = baseDamage;

            // Apply additive flat damage modifiers
            damage += _targetBuffDebuffSystem.GetModifierSum(CommonModifiers.DAMAGE_FLAT);

            // Apply multiplicative damage modifiers
            float damageMultiplier = _targetBuffDebuffSystem.GetModifierMultiplier(CommonModifiers.DAMAGE_MULTIPLIER);
            damage *= damageMultiplier;

            // Apply damage reduction from armor or other effects
            float damageReduction = _targetBuffDebuffSystem.GetModifierSum(CommonModifiers.DAMAGE_REDUCTION);
            damage = damage * (1f - Mathf.Clamp01(damageReduction));

            return Mathf.Max(0, damage);
        }

        /// <summary>
        /// Deal damage to a target, considering all active effects.
        /// </summary>
        public void DealDamageToTarget(GameObject target, float baseDamage)
        {
            _targetBuffDebuffSystem = target.GetComponent<BuffDebuffSystem>();
            
            float actualDamage = CalculateDamage(baseDamage);
            Debug.Log($"Dealing {actualDamage} damage to {target.name}");

            // Apply damage to health system, etc.
        }
    }

    /// <summary>
    /// Example implementation: A movement system that uses BuffDebuffSystem to modify speed.
    /// </summary>
    public class MovementSystemExample : MonoBehaviour
    {
        private BuffDebuffSystem _buffDebuffSystem;
        public float baseMovementSpeed = 5f;

        private void Start()
        {
            _buffDebuffSystem = GetComponent<BuffDebuffSystem>();
        }

        private void Update()
        {
            float effectiveSpeed = GetEffectiveMovementSpeed();
            
            // Apply movement input with modified speed
            Vector3 movement = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            transform.Translate(movement * effectiveSpeed * Time.deltaTime);
        }

        private float GetEffectiveMovementSpeed()
        {
            if (_buffDebuffSystem == null)
                return baseMovementSpeed;

            float speed = baseMovementSpeed;

            // Apply multiplicative speed modifiers
            float speedMultiplier = _buffDebuffSystem.GetModifierMultiplier(CommonModifiers.MOVEMENT_SPEED_MULTIPLIER);
            speed *= speedMultiplier;

            // Apply additive speed modifiers
            speed += _buffDebuffSystem.GetModifierSum(CommonModifiers.MOVEMENT_SPEED_FLAT);

            return Mathf.Max(0, speed);
        }

        public void Stun(float duration)
        {
            var stunEffect = new StunEffect("stun", duration);
            _buffDebuffSystem.AddEffect(stunEffect);
        }

        public bool IsStunned()
        {
            return _buffDebuffSystem.GetEffect("stun") != null && _buffDebuffSystem.GetEffect("stun").IsActive;
        }
    }

    /// <summary>
    /// Example: Creating and applying custom effects at runtime.
    /// </summary>
    public class CustomEffectCreator : MonoBehaviour
    {
        private BuffDebuffSystem _buffDebuffSystem;

        private void Start()
        {
            _buffDebuffSystem = GetComponent<BuffDebuffSystem>();
        }

        /// <summary>
        /// Apply poison that deals damage over time.
        /// </summary>
        public void ApplyPoison(float duration = 10f, float damagePerTick = 5f, float tickRate = 1f)
        {
            var poison = new DamageOverTimeEffect(
                "poison",
                duration,
                damagePerTick,
                tickRate,
                onTick: (damage) => Debug.Log($"Poison damage: {damage}")
            );
            _buffDebuffSystem.AddEffect(poison);
        }

        /// <summary>
        /// Create and apply a custom attribute buff.
        /// </summary>
        public void ApplyAttributeBuff(string buffName, float duration, params (string modifierId, float value)[] modifiers)
        {
            var customBuff = new CustomMultiModifierEffect(buffName, duration, modifiers);
            _buffDebuffSystem.AddEffect(customBuff);
        }
    }

    /// <summary>
    /// A flexible effect that can have any number of modifiers.
    /// Useful for creating effects at runtime.
    /// </summary>
    public class CustomMultiModifierEffect : BuffDebuffEffect
    {
        private IStatModifier[] _modifiers;

        public CustomMultiModifierEffect(string effectID, float duration, params (string modifierId, float value)[] modifiers)
            : base(effectID, duration, maxStackCount: 1)
        {
            _modifiers = new IStatModifier[modifiers.Length];
            for (int i = 0; i < modifiers.Length; i++)
            {
                _modifiers[i] = new SimpleStatModifier(modifiers[i].modifierId, modifiers[i].value);
            }
        }

        public override IStatModifier[] GetModifiers()
        {
            return _modifiers;
        }
    }
}
