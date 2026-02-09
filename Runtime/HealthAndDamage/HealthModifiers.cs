namespace Shizounu.Library.HealthAndDamage
{
    /// <summary>
    /// Custom modifier IDs specific to the Health and Damage system.
    /// These work with the BuffDebuff system to modify health mechanics.
    /// </summary>
    public static class HealthModifiers
    {
        // Incoming damage modifiers
        public const string DAMAGE_TAKEN_MULTIPLIER = "health.damage.taken.multiplier";  // How much damage you take (% increase)
        public const string DAMAGE_TAKEN_FLAT = "health.damage.taken.flat";              // Flat reduction/increase to damage taken
        public const string ARMOR = "health.armor";                                      // Physical armor (reduces damage)
        public const string MAGIC_RESISTANCE = "health.magic.resistance";                // Magic resistance (reduces magic damage)
        public const string TENACITY = "health.tenacity";                                // Crowd control reduction

        // Healing modifiers
        public const string HEALING_RECEIVED_MULTIPLIER = "health.healing.received.multiplier";  // How much healing you receive
        public const string HEALING_DEALT_MULTIPLIER = "health.healing.dealt.multiplier";      // How much healing you deal to others
        public const string LIFESTEAL_PERCENT = "health.lifesteal.percent";                    // % of damage dealt converted to healing

        // Health pool modifiers
        public const string MAX_HEALTH_MULTIPLIER = "health.max.multiplier";  // Multiplicative max health increase
        public const string MAX_HEALTH_FLAT = "health.max.flat";              // Additive max health increase
        public const string HEALTH_REGENERATION = "health.regeneration";      // Health restored per second

        // Special status modifiers
        public const string INVULNERABLE = "health.invulnerable";             // Immunity to damage
        public const string UNSTOPPABLE = "health.unstoppable";               // Cannot be stunned/slowed/disabled
    }
}
