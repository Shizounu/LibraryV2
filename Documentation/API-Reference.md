# API Quick Reference

Fast lookup for the most commonly used APIs.

## Buff/Debuff System

```csharp
// Add/Remove
buffSystem.AddEffect(effect);
buffSystem.RemoveEffect(\"effect_id\");
buffSystem.ClearAllEffects();

// Query
IBuffDebuffEffect effect = buffSystem.GetEffect(\"id\");
float sum = buffSystem.GetModifierSum(MODIFIER_ID);
float mult = buffSystem.GetModifierMultiplier(MODIFIER_ID);
bool has = buffSystem.HasModifier(MODIFIER_ID);
int count = buffSystem.GetActiveEffectCount();

// Configuration
buffSystem.UpdateInterval = 0.1f; // Default 0 (every frame)

// Events
buffSystem.OnEffectAdded += (effect) => { };
buffSystem.OnEffectRemoved += (effect) => { };
buffSystem.OnEffectStacked += (effect) => { };
```

## Health Component

```csharp
// Configuration
health.SetMaxHealth(100f);

// Damage/Healing
float actual = health.TakeDamage(damage, \"source\", DamageType.Physical);
health.TakeDamageRaw(damage);
float healed = health.Heal(amount, \"source\");

// State
health.Die(\"source\");
health.Revive(0.5f); // Revive at 50%
health.FullRestore();

// Query
float current = health.CurrentHealth;
float max = health.EffectiveMaxHealth;
float percent = health.HealthPercent; // 0-1
bool dead = health.IsDead;
bool alive = health.IsAlive;

// Events
health.OnHealthChanged += (old, new, max) => { };
health.OnDeath += (source) => { };
health.OnRevived += () => { };
health.OnFullyHealed += () => { };
health.OnMaxHealthChanged += (old, new) => { };
```

## Damage Dealer

```csharp
// Configuration
dealer.SetBaseDamage(20f);

// Dealing Damage
float actual = dealer.DealDamageToTarget(target, DamageType.Physical);
float actual = dealer.DealDamageToTarget(target, customDamage: 50f, DamageType.Magic);

// Query
float effective = dealer.GetEffectiveDamage();
bool hasBonus = dealer.HasDamageBonus();

// Self Healing
dealer.Heal(20f, \"lifesteal\");
```

## Damage Types

```csharp
DamageType.Physical    // Reduced by ARMOR
DamageType.Magic       // Reduced by MAGIC_RESISTANCE
DamageType.True        // Ignores resistances
```

## Common Modifiers

```csharp
// Damage
CommonModifiers.DAMAGE_MULTIPLIER           // Multiplicative
CommonModifiers.DAMAGE_FLAT                 // Additive

// Movement
CommonModifiers.MOVEMENT_SPEED_MULTIPLIER   // Multiplicative
CommonModifiers.MOVEMENT_SPEED_FLAT         // Additive

// Defense
CommonModifiers.ARMOR_FLAT                  // Additive
CommonModifiers.ARMOR_MULTIPLIER            // Multiplicative
CommonModifiers.DAMAGE_REDUCTION            // 0-1

// Abilities
CommonModifiers.ABILITY_COOLDOWN_MULTIPLIER // Cooldown reduction
CommonModifiers.ABILITY_DAMAGE_MULTIPLIER   // Ability damage boost
```

## Health Modifiers

```csharp
// Damage
HealthModifiers.DAMAGE_TAKEN_MULTIPLIER     // Multiplicative incoming damage
HealthModifiers.DAMAGE_TAKEN_FLAT           // Flat incoming damage
HealthModifiers.ARMOR                       // Physical armor
HealthModifiers.MAGIC_RESISTANCE            // Magic resistance
HealthModifiers.TENACITY                    // CC reduction

// Healing
HealthModifiers.HEALING_RECEIVED_MULTIPLIER // Incoming healing mult
HealthModifiers.HEALING_DEALT_MULTIPLIER    // Outgoing healing mult
HealthModifiers.LIFESTEAL_PERCENT           // Damage → healing

// Health Pool
HealthModifiers.MAX_HEALTH_MULTIPLIER       // Multiplicative max HP
HealthModifiers.MAX_HEALTH_FLAT             // Additive max HP
HealthModifiers.HEALTH_REGENERATION         // HP/sec

// Status
HealthModifiers.INVULNERABLE                // Damage immunity
HealthModifiers.UNSTOPPABLE                 // Can't be disabled
```

## Predefined Effects

```csharp
new ArmorEffect(\"id\", duration, armorAmount);
new MagicResistanceEffect(\"id\", duration, resistAmount);
new MaxHealthBuff(\"id\", duration, healthAmount, maxStacks: 3);
new HealthRegenEffect(\"id\", duration, regenPerSecond);
new InvulnerabilityEffect(\"id\", duration);
new TenacityEffect(\"id\", duration, reductionPercent);
new HealingReceivedBuff(\"id\", duration, bonusPercent);
new VulnerabilityEffect(\"id\", duration, damageIncrease);
new LifestealEffect(\"id\", duration, percentDamageToHealing);
new FortifyEffect(\"id\", duration, healthBoost, armorBoost);
```

## Scriptable Variables

```csharp
// Read
float value = variable.RuntimeValue;
float initial = variable.InitialValue;
bool isInitial = variable.IsAtInitialValue;

// Write
variable.RuntimeValue = newValue;
variable.ResetValue();

// Events
variable.OnRuntimeValueChanged += (newValue) => { };
variable.OnValueReset += () => { };
```

## Health UI Connector

```csharp
// Programmatic Setup
connector.SetUIElements(healthText, healthBar, percentText, statusText);
connector.SetScriptableVariables(currentHealth, maxHealth, healthPercent, isDead);
connector.SetHealthColors(normal: Color.green, low: Color.yellow, 
                         critical: Color.red, dead: Color.gray);

// Configuration
connector.lowHealthThreshold = 0.5f;        // 50%
connector.criticalHealthThreshold = 0.25f;  // 25%
```

## Character Controller

```csharp
// Movement
controller.Move(Vector3 direction);

// Jumping
controller.Jump();

// Query
bool grounded = controller.IsGrounded;

// Configuration
controller.speed = 5f;
controller.jumpForce = 5f;
controller.acceleration = 10f;
```

## Tween System

```csharp
// Position
Tween.To(transform, \"localPosition\", targetPos, duration: 2f);

// Scale
Tween.To(transform, \"localScale\", targetScale, duration: 1f);

// Rotation
Tween.To(transform, \"localEulerAngles\", targetRot, duration: 1f);

// Color
Tween.To(image, \"color\", targetColor, duration: 0.5f);

// Custom
Tween.To(GetValue, SetValue, targetValue, duration: 1f);
```

## Update System

```csharp
// Set update interval
buffSystem.UpdateInterval = 0.1f;  // Update every 0.1s
buffSystem.UpdateInterval = 0f;    // Update every frame
```

---

## Quick Copy-Paste Setup

### One-Liner Health Setup

```csharp
var health = (gameObject.AddComponent<HealthComponent>());
var buffs = gameObject.AddComponent<BuffDebuffSystem>();
health.SetMaxHealth(100f);
```

### Damage Application

```csharp
float damage = health.TakeDamage(25f, \"attack\", DamageType.Physical);
if (health.IsDead) { /* handle death */ }
```

### Buff Application

```csharp
buffSystem.AddEffect(new ArmorEffect(\"armor\", -1f, 20f));
```

### Query Modifier

```csharp
float armor = buffSystem.GetModifierSum(HealthModifiers.ARMOR);
```

### Subscribe to Event

```csharp
health.OnDeath += (source) => Debug.Log($\"Died from {source}\");
health.OnHealthChanged += (old, new, max) => UpdateUI();
```

---

## Performance Tips

- Use constants for modifier IDs (no string allocation)
- Cache component references (no GetComponent per frame)
- Unsubscribe from events (no memory leaks)
- Use UpdateInterval > 0 for non-frame-critical systems
- Remove effects on entity death

---

## See Also

- [Health & Damage System](Health-Damage-System.md) - Full API docs
- [Buff/Debuff System](Buff-Debuff-System.md) - Effect system details
- [Scriptable Architecture](Scriptable-Architecture.md) - Variable system
- [Best Practices](Best-Practices.md) - Code patterns
