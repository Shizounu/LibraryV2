using Shizounu.Library.BuffDebuff;

namespace Shizounu.Library.HealthAndDamage
{
    /// <summary>
    /// Custom BuffDebuff effects for the Health and Damage system.
    /// Inherit from these or create your own for health-related buffs/debuffs.
    /// </summary>
    /// 
    /// <summary>
    /// Effect that reduces incoming damage (armor/damage reduction).
    /// </summary>
    public class ArmorEffect : BuffDebuffEffect
    {
        private float _armorAmount;

        public ArmorEffect(string effectID, float duration, float armorAmount, int maxStackCount = 1)
            : base(effectID, duration, maxStackCount)
        {
            _armorAmount = armorAmount;
        }

        public override IStatModifier[] GetModifiers()
        {
            return new IStatModifier[]
            {
                new SimpleStatModifier(HealthModifiers.ARMOR, _armorAmount)
            };
        }
    }

    /// <summary>
    /// Effect that provides magic resistance.
    /// </summary>
    public class MagicResistanceEffect : BuffDebuffEffect
    {
        private float _resistanceAmount;

        public MagicResistanceEffect(string effectID, float duration, float resistanceAmount, int maxStackCount = 1)
            : base(effectID, duration, maxStackCount)
        {
            _resistanceAmount = resistanceAmount;
        }

        public override IStatModifier[] GetModifiers()
        {
            return new IStatModifier[]
            {
                new SimpleStatModifier(HealthModifiers.MAGIC_RESISTANCE, _resistanceAmount)
            };
        }
    }

    /// <summary>
    /// Effect that increases max health.
    /// </summary>
    public class MaxHealthBuff : BuffDebuffEffect
    {
        private float _healthIncrease;
        private bool _isFlatBonus;

        public MaxHealthBuff(string effectID, float duration, float healthIncrease, bool isFlatBonus = true, int maxStackCount = 1)
            : base(effectID, duration, maxStackCount)
        {
            _healthIncrease = healthIncrease;
            _isFlatBonus = isFlatBonus;
        }

        public override IStatModifier[] GetModifiers()
        {
            string modifierID = _isFlatBonus ? HealthModifiers.MAX_HEALTH_FLAT : HealthModifiers.MAX_HEALTH_MULTIPLIER;
            return new IStatModifier[]
            {
                new SimpleStatModifier(modifierID, _healthIncrease)
            };
        }
    }

    /// <summary>
    /// Effect that provides passive health regeneration.
    /// </summary>
    public class HealthRegenEffect : BuffDebuffEffect
    {
        private float _regenPerSecond;

        public HealthRegenEffect(string effectID, float duration, float regenPerSecond, int maxStackCount = 1)
            : base(effectID, duration, maxStackCount)
        {
            _regenPerSecond = regenPerSecond;
        }

        public override IStatModifier[] GetModifiers()
        {
            return new IStatModifier[]
            {
                new SimpleStatModifier(HealthModifiers.HEALTH_REGENERATION, _regenPerSecond)
            };
        }
    }

    /// <summary>
    /// Effect that provides invulnerability (immunity to all damage).
    /// </summary>
    public class InvulnerabilityEffect : BuffDebuffEffect
    {
        public InvulnerabilityEffect(string effectID, float duration, int maxStackCount = 1)
            : base(effectID, duration, maxStackCount)
        {
        }

        public override IStatModifier[] GetModifiers()
        {
            return new IStatModifier[]
            {
                new SimpleStatModifier(HealthModifiers.INVULNERABLE, 1f) // Value doesn't matter, just needs to exist
            };
        }
    }

    /// <summary>
    /// Effect that provides immunity to crowd control effects.
    /// </summary>
    public class TenacityEffect : BuffDebuffEffect
    {
        private float _ccReduction;

        public TenacityEffect(string effectID, float duration, float ccReduction, int maxStackCount = 1)
            : base(effectID, duration, maxStackCount)
        {
            _ccReduction = ccReduction;
        }

        public override IStatModifier[] GetModifiers()
        {
            return new IStatModifier[]
            {
                new SimpleStatModifier(HealthModifiers.TENACITY, _ccReduction)
            };
        }
    }

    /// <summary>
    /// Effect that increases incoming healing received.
    /// </summary>
    public class HealingReceivedBuff : BuffDebuffEffect
    {
        private float _healingBonus;

        public HealingReceivedBuff(string effectID, float duration, float healingBonus, int maxStackCount = 1)
            : base(effectID, duration, maxStackCount)
        {
            _healingBonus = healingBonus;
        }

        public override IStatModifier[] GetModifiers()
        {
            return new IStatModifier[]
            {
                new SimpleStatModifier(HealthModifiers.HEALING_RECEIVED_MULTIPLIER, _healingBonus)
            };
        }
    }

    /// <summary>
    /// Effect that increases damage taken (weakness/vulnerability).
    /// </summary>
    public class VulnerabilityEffect : BuffDebuffEffect
    {
        private float _damageIncrease;

        public VulnerabilityEffect(string effectID, float duration, float damageIncrease, int maxStackCount = 1)
            : base(effectID, duration, maxStackCount)
        {
            _damageIncrease = damageIncrease;
        }

        public override IStatModifier[] GetModifiers()
        {
            return new IStatModifier[]
            {
                new SimpleStatModifier(HealthModifiers.DAMAGE_TAKEN_MULTIPLIER, _damageIncrease)
            };
        }
    }

    /// <summary>
    /// Effect that provides lifesteal (convert damage dealt to healing).
    /// </summary>
    public class LifestealEffect : BuffDebuffEffect
    {
        private float _lifestealPercent;

        public LifestealEffect(string effectID, float duration, float lifestealPercent, int maxStackCount = 1)
            : base(effectID, duration, maxStackCount)
        {
            _lifestealPercent = lifestealPercent;
        }

        public override IStatModifier[] GetModifiers()
        {
            return new IStatModifier[]
            {
                new SimpleStatModifier(HealthModifiers.LIFESTEAL_PERCENT, _lifestealPercent)
            };
        }
    }

    /// <summary>
    /// Combined effect that increases both max health and armor.
    /// Example of a multi-modifier effect.
    /// </summary>
    public class FortifyEffect : BuffDebuffEffect
    {
        private float _maxHealthBonus;
        private float _armorBonus;

        public FortifyEffect(string effectID, float duration, float maxHealthBonus, float armorBonus, int maxStackCount = 1)
            : base(effectID, duration, maxStackCount)
        {
            _maxHealthBonus = maxHealthBonus;
            _armorBonus = armorBonus;
        }

        public override IStatModifier[] GetModifiers()
        {
            return new IStatModifier[]
            {
                new SimpleStatModifier(HealthModifiers.MAX_HEALTH_FLAT, _maxHealthBonus),
                new SimpleStatModifier(HealthModifiers.ARMOR, _armorBonus)
            };
        }
    }
}
