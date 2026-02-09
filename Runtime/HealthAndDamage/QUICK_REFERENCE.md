# Health and Damage System - Quick Reference

## Components Setup

### Minimal Setup
```csharp
// Add to game object
gameObject.AddComponent<HealthComponent>();
gameObject.AddComponent<BuffDebuffSystem>();
```

### Full Setup with Damage Dealing
```csharp
gameObject.AddComponent<HealthComponent>();
gameObject.AddComponent<BuffDebuffSystem>();
gameObject.AddComponent<DamageDealer>();
```

## HealthComponent Usage

### Properties
```csharp
float health = healthComponent.CurrentHealth;
float maxHealth = healthComponent.EffectiveMaxHealth; // With buffs
float percent = healthComponent.HealthPercent; // 0-1
bool dead = healthComponent.IsDead;
bool alive = healthComponent.IsAlive;
```

### Actions
```csharp
healthComponent.SetMaxHealth(100f);
healthComponent.TakeDamage(25f, "attack", DamageType.Physical);
healthComponent.TakeDamageRaw(10f); // Ignores modifiers
healthComponent.Heal(30f, "potion");
healthComponent.FullRestore();
healthComponent.Die("poison");
healthComponent.Revive(0.5f); // Revive at 50%
```

### Events
```csharp
healthComponent.OnHealthChanged += (old, new, max) => Debug.Log($"Health: {new}");
healthComponent.OnDeath += (source) => Debug.Log($"Died from: {source}");
healthComponent.OnRevived += () => Debug.Log("Revived!");
healthComponent.OnFullyHealed += () => Debug.Log("Full health!");
healthComponent.OnMaxHealthChanged += (old, new) => Debug.Log($"Max health: {new}");
```

## DamageDealer Usage

### Basic Damage
```csharp
DamageDealer dealer = GetComponent<DamageDealer>();
dealer.SetBaseDamage(20f);
float damage = dealer.GetEffectiveDamage(); // With buffs applied
float actual = dealer.DealDamageToTarget(enemy, DamageType.Physical);
```

### Custom Damage
```csharp
float actual = dealer.DealDamageToTarget(enemy, customAmount: 50f, DamageType.Magic);
```

### Self Healing
```csharp
dealer.Heal(20f, "lifesteal");
```

## Applying Effects

### Basic Effect
```csharp
BuffDebuffSystem buffs = GetComponent<BuffDebuffSystem>();

// Armor buff
var armor = new ArmorEffect("armor", duration: 10f, armorAmount: 30f);
buffs.AddEffect(armor);

// Healing boost
var healing = new HealingReceivedBuff("bless", 15f, 0.25f); // 25% more
buffs.AddEffect(healing);
```

### Stacking Effects
```csharp
var damage = new MaxHealthBuff("boost", 5f, 20f, maxStackCount: 3);
buffs.AddEffect(damage); // Stacks up to 3 times
buffs.AddEffect(damage); // Stacks if not at max
```

### Infinite Duration
```csharp
var regen = new HealthRegenEffect("passive_regen", -1f, 5f); // -1 = infinite
buffs.AddEffect(regen);
```

### Remove Effect
```csharp
buffs.RemoveEffect("armor");
```

## Damage Types

```csharp
// Physical - reduced by Armor
healthComponent.TakeDamage(30f, "slash", DamageType.Physical);

// Magic - reduced by Magic Resistance
healthComponent.TakeDamage(25f, "fireball", DamageType.Magic);

// True - ignores all resistances
healthComponent.TakeDamage(20f, "execution", DamageType.True);
```

## All Available Effects

| Effect | Usage |
|--------|-------|
| ArmorEffect | Reduces physical damage |
| MagicResistanceEffect | Reduces magic damage |
| MaxHealthBuff | Increases max HP |
| HealthRegenEffect | Passive HP regeneration |
| InvulnerabilityEffect | Immunity to all damage |
| TenacityEffect | CC reduction |
| HealingReceivedBuff | More healing received |
| VulnerabilityEffect | Takes more damage |
| LifestealEffect | Damage → healing |
| FortifyEffect | HP + Armor combo |

## All Modifiers

```csharp
// Query current modifiers
float armor = buffs.GetModifierSum(HealthModifiers.ARMOR);
float regenRate = buffs.GetModifierSum(HealthModifiers.HEALTH_REGENERATION);
float healingMult = buffs.GetModifierMultiplier(HealthModifiers.HEALING_RECEIVED_MULTIPLIER);
bool invuln = buffs.HasModifier(HealthModifiers.INVULNERABLE);
```

**Additive Modifiers** (use `GetModifierSum`):
- ARMOR
- MAGIC_RESISTANCE
- DAMAGE_TAKEN_FLAT
- MAX_HEALTH_FLAT
- HEALTH_REGENERATION
- LIFESTEAL_PERCENT

**Multiplicative Modifiers** (use `GetModifierMultiplier`):
- DAMAGE_TAKEN_MULTIPLIER
- HEALING_RECEIVED_MULTIPLIER
- HEALING_DEALT_MULTIPLIER
- MAX_HEALTH_MULTIPLIER

**Existence Modifiers** (use `HasModifier`):
- INVULNERABLE
- UNSTOPPABLE

## Common Patterns

### Player Taking Damage
```csharp
public void TakeDamageFromAttack(float attackDamage, GameObject attacker)
{
    float actualDamage = health.TakeDamage(attackDamage, attacker.name, DamageType.Physical);
    // Check if dead
    if (health.IsDead)
    {
        Respawn();
    }
}
```

### Enemy Attacking
```csharp
public void Attack(GameObject target)
{
    float effectiveDamage = dealer.GetEffectiveDamage();
    dealer.DealDamageToTarget(target, DamageType.Physical);
    
    if (dealer.HasDamageBonus())
    {
        Debug.Log("Critical strike!");
    }
}
```

### Healing Spell
```csharp
public float CastHeal(GameObject target, float baseHealing)
{
    HealthComponent targetHealth = target.GetComponent<HealthComponent>();
    if (targetHealth && targetHealth.IsAlive)
    {
        return targetHealth.Heal(baseHealing, "spell");
    }
    return 0;
}
```

### Buff Application
```csharp
public void ApplyShield(GameObject target, float armorBonus)
{
    BuffDebuffSystem buffs = target.GetComponent<BuffDebuffSystem>();
    var effect = new ArmorEffect("shield", 8f, armorBonus);
    buffs.AddEffect(effect);
}
```

### Check Health Status
```csharp
void Update()
{
    if (health.HealthPercent < 0.2f)
    {
        // Low health warning
        PlayLowHealthSound();
    }
    
    if (health.IsDead)
    {
        // Handle death state
        DisableInput();
    }
}
```

### Custom Damage Calculation
```csharp
public float CalculateAttackDamage(float baseDamage, GameObject target)
{
    float damage = baseDamage;
    
    // Apply my buffs
    float dmgMult = buffs.GetModifierMultiplier(CommonModifiers.DAMAGE_MULTIPLIER);
    damage *= dmgMult;
    
    // Get target's resistances
    HealthComponent targetHealth = target.GetComponent<HealthComponent>();
    float targetDmgMult = targetHealth.GetDamageMultiplier();
    damage *= targetDmgMult;
    
    return Mathf.Max(0, damage);
}
```

## Event Chain Example

```csharp
private void Start()
{
    health.OnHealthChanged += OnHealthChanged;
    health.OnDeath += OnDeath;
    health.OnRevived += OnRevived;
}

private void OnHealthChanged(float oldHealth, float newHealth, float maxHealth)
{
    // Update UI
    healthBar.SetValue(newHealth / maxHealth);
    
    // Play sound if took damage
    if (newHealth < oldHealth)
    {
        PlayDamageSound();
    }
}

private void OnDeath(string source)
{
    // Disable character
    enabled = false;
    
    // Drop loot
    DropLoot();
    
    // Log death
    Debug.Log($"Died to {source}");
}

private void OnRevived()
{
    // Enable character
    enabled = true;
    
    // Play revive effect
    PlayReviveEffect();
}
```

## Debugging Tips

```csharp
// Check current state
Debug.Log($"Health: {health.CurrentHealth}/{health.EffectiveMaxHealth}");
Debug.Log($"Armor: {buffs.GetModifierSum(HealthModifiers.ARMOR)}");
Debug.Log($"Active Effects: {buffs.GetActiveEffectCount()}");

// List all effects
foreach (var effect in buffs.GetAllEffects())
{
    Debug.Log($"Effect: {effect.EffectID}, Time: {effect.TimeRemaining}s, Stacks: {effect.StackCount}");
}

// Check specific effect
var armorEffect = buffs.GetEffect("armor");
if (armorEffect != null && armorEffect.IsActive)
{
    Debug.Log($"Armor expires in {armorEffect.TimeRemaining}s");
}
```
## Scriptable Architecture Integration

### Quick Setup (Inspector)

1. Create ScriptableFloat assets:
   - PlayerCurrentHealth
   - PlayerMaxHealth
   - PlayerHealthPercent

2. Create ScriptableBool asset:
   - PlayerIsDead

3. In HealthComponent inspector, assign all 4 variables

4. Add HealthUIConnector to Canvas

5. In HealthUIConnector, assign:
   - UI elements (Text, Image, Slider)
   - Scriptable variables (same 4 as above)

### UI Connector Setup

```csharp
// In inspector - complete decoupled setup:
/*
HealthUIConnector
├─ Health Text: MyHealthText
├─ Health Bar Image: MyHealthBar
├─ Status Text: MyStatusText
├─ Current Health Variable: PlayerCurrentHealth
├─ Max Health Variable: PlayerMaxHealth
├─ Health Percent Variable: PlayerHealthPercent
└─ Is Dead Variable: PlayerIsDead
*/

// Programmatic setup
connector.SetScriptableVariables(currentHealthVar, maxHealthVar, percentVar, isDeadVar);
connector.SetUIElements(healthText, healthBar, percentText, statusText, slider);
connector.SetHealthColors(Color.green, Color.yellow, Color.red, Color.gray);
```

### Simple Health Bar

For lightweight health bar with gradient:

```csharp
// In inspector:
/*
SimpleHealthBar
├─ Health Percent Variable: PlayerHealthPercent
├─ Health Bar Image: MyHealthBar
└─ Health Gradient: Green → Red gradient
*/
```

### Damage Numbers

Display floating damage text:

```csharp
// In inspector:
/*
DamageNumberDisplay
├─ Current Health Variable: PlayerCurrentHealth
├─ Damage Number Prefab: FloatingDamageText
├─ Canvas: MainCanvas
└─ Spawn Point: EntityHead (transform)
*/
```

### Benefits

✅ **Decoupled** - UI doesn't reference HealthComponent
✅ **Reusable** - Use same UI system for all entities
✅ **Designer-Friendly** - No code needed, inspector setup
✅ **Event-Driven** - Updates only when values change
✅ **Flexible** - Multiple UI elements can listen to same variable

### Common Pattern

```
Player/Enemy
  ├─ HealthComponent (manages health)
  │   └─ Updates: PlayerCurrentHealth (ScriptableFloat)
  └─ BuffDebuffSystem (manages effects)
       └─ Modifies damage calculations

Canvas
  └─ HealthUIConnector (subscribes to PlayerCurrentHealth)
      └─ Updates: HealthBar, HealthText, StatusText
```

Health changes flow: HealthComponent → ScriptableFloat → Event → UI