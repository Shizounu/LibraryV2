namespace Shizounu.Library.BuffDebuff
{
    /// <summary>
    /// Example: A damage boost effect that increases outgoing damage.
    /// </summary>
    public class DamageBoostEffect : BuffDebuffEffect
    {
        private float _damageMultiplier;
        private IStatModifier[] _modifiers;

        public DamageBoostEffect(string effectID, float duration, float damageMultiplier, int maxStacks = 1)
            : base(effectID, duration, maxStacks)
        {
            _damageMultiplier = damageMultiplier;
            _modifiers = new IStatModifier[]
            {
                new SimpleStatModifier(CommonModifiers.DAMAGE_MULTIPLIER, damageMultiplier)
            };
        }

        public override IStatModifier[] GetModifiers()
        {
            return _modifiers;
        }
    }

    /// <summary>
    /// Example: A movement speed buff that increases movement speed.
    /// </summary>
    public class MovementSpeedEffect : BuffDebuffEffect
    {
        private float _speedMultiplier;
        private IStatModifier[] _modifiers;

        public MovementSpeedEffect(string effectID, float duration, float speedMultiplier, int maxStacks = 1)
            : base(effectID, duration, maxStacks)
        {
            _speedMultiplier = speedMultiplier;
            _modifiers = new IStatModifier[]
            {
                new SimpleStatModifier(CommonModifiers.MOVEMENT_SPEED_MULTIPLIER, speedMultiplier)
            };
        }

        public override IStatModifier[] GetModifiers()
        {
            return _modifiers;
        }
    }

    /// <summary>
    /// Example: An armor buff that reduces incoming damage via armor.
    /// </summary>
    public class ArmorEffect : BuffDebuffEffect
    {
        private float _armorAmount;
        private IStatModifier[] _modifiers;

        public ArmorEffect(string effectID, float duration, float armorAmount, int maxStacks = 1)
            : base(effectID, duration, maxStacks)
        {
            _armorAmount = armorAmount;
            _modifiers = new IStatModifier[]
            {
                new SimpleStatModifier(CommonModifiers.ARMOR_FLAT, armorAmount)
            };
        }

        public override IStatModifier[] GetModifiers()
        {
            return _modifiers;
        }
    }

    /// <summary>
    /// Example: A stun effect that prevents the target from acting.
    /// </summary>
    public class StunEffect : BuffDebuffEffect
    {
        public StunEffect(string effectID, float duration)
            : base(effectID, duration, maxStackCount: 1)
        {
        }

        public override IStatModifier[] GetModifiers()
        {
            // Stun doesn't provide stat modifiers, but other systems can check if stun is active
            return System.Array.Empty<IStatModifier>();
        }
    }

    /// <summary>
    /// Example: A multi-modifier effect that provides both damage and movement bonuses.
    /// </summary>
    public class HeroicBoostEffect : BuffDebuffEffect
    {
        private IStatModifier[] _modifiers;

        public HeroicBoostEffect(string effectID, float duration, float damageBuff, float speedBuff, int maxStacks = 1)
            : base(effectID, duration, maxStacks)
        {
            _modifiers = new IStatModifier[]
            {
                new SimpleStatModifier(CommonModifiers.DAMAGE_MULTIPLIER, damageBuff),
                new SimpleStatModifier(CommonModifiers.MOVEMENT_SPEED_MULTIPLIER, speedBuff),
                new SimpleStatModifier(CommonModifiers.ARMOR_FLAT, 10f)
            };
        }

        public override IStatModifier[] GetModifiers()
        {
            return _modifiers;
        }

        public override void OnApply()
        {
            base.OnApply();
            // Can add visual effects, sounds, etc. here
        }

        public override void OnRemove()
        {
            base.OnRemove();
            // Clean up visual effects, sounds, etc. here
        }
    }

    /// <summary>
    /// Example: A damage over time effect that deals repeated damage.
    /// </summary>
    public class DamageOverTimeEffect : BuffDebuffEffect
    {
        private float _tickInterval;
        private float _damagePerTick;
        private float _timeSinceLastTick;
        private System.Action<float> _onTick;

        public DamageOverTimeEffect(
            string effectID, 
            float duration, 
            float damagePerTick, 
            float tickInterval,
            System.Action<float> onTick = null
        )
            : base(effectID, duration, maxStackCount: 1)
        {
            _damagePerTick = damagePerTick;
            _tickInterval = tickInterval;
            _timeSinceLastTick = 0;
            _onTick = onTick;
        }

        public override void OnUpdate(float deltaTime)
        {
            base.OnUpdate(deltaTime);

            _timeSinceLastTick += deltaTime;
            while (_timeSinceLastTick >= _tickInterval && IsActive)
            {
                _timeSinceLastTick -= _tickInterval;
                _onTick?.Invoke(_damagePerTick);
            }
        }

        public override IStatModifier[] GetModifiers()
        {
            // DoT doesn't provide stat modifiers, damage is applied via callback
            return System.Array.Empty<IStatModifier>();
        }
    }
}
