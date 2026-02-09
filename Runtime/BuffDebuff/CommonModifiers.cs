namespace Shizounu.Library.BuffDebuff
{
    /// <summary>
    /// A simple stat modifier implementation for numeric values.
    /// </summary>
    public class SimpleStatModifier : IStatModifier
    {
        private string _modifierID;
        private float _value;

        public string ModifierID => _modifierID;
        public float Value => _value;

        public SimpleStatModifier(string modifierID, float value)
        {
            _modifierID = modifierID;
            _value = value;
        }
    }

    /// <summary>
    /// Common modifier IDs for typical game mechanics.
    /// Use these constants to keep your code consistent.
    /// </summary>
    public static class CommonModifiers
    {
        // Damage modifiers
        public const string DAMAGE_MULTIPLIER = "damage.multiplier";  // Multiplicative (value is added to 1.0)
        public const string DAMAGE_FLAT = "damage.flat";              // Additive (direct value)

        // Movement modifiers
        public const string MOVEMENT_SPEED_MULTIPLIER = "movement.speed.multiplier";  // Multiplicative
        public const string MOVEMENT_SPEED_FLAT = "movement.speed.flat";           // Additive

        // Defense modifiers
        public const string ARMOR_MULTIPLIER = "armor.multiplier";    // Multiplicative
        public const string ARMOR_FLAT = "armor.flat";                // Additive
        public const string DAMAGE_REDUCTION = "damage.reduction";    // Direct reduction (0-1)

        // Ability modifiers
        public const string ABILITY_COOLDOWN_MULTIPLIER = "ability.cooldown.multiplier";
        public const string ABILITY_DAMAGE_MULTIPLIER = "ability.damage.multiplier";

        // Status modifiers
        public const string STUN_IMMUNITY = "status.stun.immunity";
        public const string SLOW_IMMUNITY = "status.slow.immunity";

        // Generic modifiers
        public const string ATTACK_SPEED = "attack.speed";
        public const string CAST_SPEED = "cast.speed";
        public const string CRITICAL_CHANCE = "critical.chance";
        public const string CRITICAL_DAMAGE = "critical.damage";
    }
}
