# Health & Damage System - Complete Guide

The **Health & Damage System** provides comprehensive health management, damage calculations, and healing mechanics. It's fully integrated with the **Buff/Debuff System** for powerful synergies.

## Core Concepts

### Health Management
- **CurrentHealth** - Entity's current health value
- **MaxHealth** - Maximum possible health (can be buffed)
- **HealthPercent** - Current health as 0-1 percentage
- **IsDead / IsAlive** - Death state tracking

### Damage Types
- **Physical** - Reduced by Armor
- **Magic** - Reduced by Magic Resistance
- **True** - Bypasses all resistances

### Modifiers
All damage and healing is modified by BuffDebuff effects:

**Damage Modifiers:**
- `DAMAGE_TAKEN_MULTIPLIER` - Multiplicative incoming damage
- `DAMAGE_TAKEN_FLAT` - Flat incoming damage modification
- `ARMOR` - Physical armor (reduces physical damage)
- `MAGIC_RESISTANCE` - Magic resistance (reduces magic damage)
- `TENACITY` - Crowd control reduction

**Healing Modifiers:**
- `HEALING_RECEIVED_MULTIPLIER` - Multiplicative incoming healing
- `HEALING_DEALT_MULTIPLIER` - Outgoing healing multiplier
- `LIFESTEAL_PERCENT` - Damage converted to healing (0-1)

**Health Pool Modifiers:**
- `MAX_HEALTH_MULTIPLIER` - Multiplicative max health
- `MAX_HEALTH_FLAT` - Additive max health
- `HEALTH_REGENERATION` - Health restored per second

## System Architecture

```
┌─ HealthComponent ────────────────────┐
│├─ Manages health state                │
│├─ Applies damage/healing              │
│└─ Fires events                        │
└──────────────────────────────────────┘
         ↓
┌─ BuffDebuffSystem ───────────────────┐
│├─ Provides modifiers (armor, etc)    │
│└─ Tracks active effects              │
└──────────────────────────────────────┘
         ↓
┌─ DamageDealer (optional) ────────────┐
│├─ Helper for dealing damage           │
│└─ Applies attacker's modifiers        │
└──────────────────────────────────────┘
```

## Setup

### Basic Setup

```csharp
using UnityEngine;
using Shizounu.Library.BuffDebuff;
using Shizounu.Library.HealthAndDamage;

public class CharacterSetup : MonoBehaviour
{
    private HealthComponent health;
    private BuffDebuffSystem buffs;

    private void Start()
    {
        // Add components
        health = gameObject.AddComponent<HealthComponent>();
        buffs = gameObject.AddComponent<BuffDebuffSystem>();

        // Configure
        health.SetMaxHealth(100f);

        // Subscribe to events
        health.OnDeath += OnDeath;
        health.OnHealthChanged += OnHealthChanged;
    }

    private void OnDeath(string source)
    {
        Debug.Log($\"Died from: {source}\");
        // Handle death
    }

    private void OnHealthChanged(float oldHealth, float newHealth, float maxHealth)
    {
        Debug.Log($\"Health: {newHealth:F1}/{maxHealth:F1}\");
    }
}
```

### Full Setup with Damage Dealing

```csharp
public class CharacterSetup : MonoBehaviour
{
    private HealthComponent health;
    private BuffDebuffSystem buffs;
    private DamageDealer dealer;

    private void Start()
    {
        // Add all components
        health = gameObject.AddComponent<HealthComponent>();
        buffs = gameObject.AddComponent<BuffDebuffSystem>();
        dealer = gameObject.AddComponent<DamageDealer>();

        // Configure health
        health.SetMaxHealth(100f);

        // Configure damage dealing
        dealer.SetBaseDamage(20f);

        // Subscribe
        health.OnDeath += OnDeath;
    }

    private void OnDeath(string source) { /* ... */ }
}
```

## HealthComponent API

### Properties

```csharp
// Current and max health
float current = health.CurrentHealth;
float max = health.EffectiveMaxHealth;    // Includes buffs
float baseMax = health.BaseMaxHealth;     // Before buffs

// Percentages
float percent = health.HealthPercent;     // 0 to 1
bool dead = health.IsDead;
bool alive = health.IsAlive;
```

### Taking Damage

```csharp
// Damage with resistance calculation
float actualDamage = health.TakeDamage(
    damage: 25f,
    source: \"enemy_attack\",
    damageType: DamageType.Physical
);

// Raw damage (ignores all modifiers)
health.TakeDamageRaw(10f);
```

**Damage Calculation Flow:**
1. Start with base damage
2. Apply attacker's damage modifiers
3. Apply target's resistances based on damage type
4. Apply target's DAMAGE_TAKEN_MULTIPLIER/FLAT
5. Check INVULNERABLE (blocks all damage)
6. Apply damage to health
7. Fire OnHealthChanged event

### Healing

```csharp
// Heal with modifiers applied
float actualHealing = health.Heal(
    amount: 30f,
    source: \"potion\"
);

// Restore to full health
health.FullRestore();
```

**Healing Modifiers:**
- Target's HEALING_RECEIVED_MULTIPLIER
- Source's HEALING_DEALT_MULTIPLIER

### Death & Revival

```csharp
// Kill immediately
health.Die(\"execution\");

// Revive at specified health percent
health.Revive(healthPercent: 0.5f); // Revive at 50%
```

### Health Configuration

```csharp
// Set max health
health.SetMaxHealth(100f);

// Effective max includes buffs:
// EffectiveMax = (BaseMax + MAX_HEALTH_FLAT) × MAX_HEALTH_MULTIPLIER
```

### Events

```csharp
// Current health changed
health.OnHealthChanged += (oldHealth, newHealth, maxHealth) => 
{
    Debug.Log($\"Health: {newHealth}/{maxHealth}\");
};

// Entity died
health.OnDeath += (source) => 
{
    Debug.Log($\"Died from: {source}\");
};

// Entity revived
health.OnRevived += () => 
{
    Debug.Log(\"Revived!\");
};

// Reached full health
health.OnFullyHealed += () => 
{
    Debug.Log(\"Fully healed!\");
};

// Max health changed (from buffs)
health.OnMaxHealthChanged += (oldMax, newMax) => 
{
    Debug.Log($\"Max health: {newMax}\");
};
```

## DamageDealer API

### Setup

```csharp
DamageDealer dealer = GetComponent<DamageDealer>();
dealer.SetBaseDamage(20f);
```

### Dealing Damage

```csharp
// Deal damage based on configured base damage
float actualDamage = dealer.DealDamageToTarget(
    target: enemyGameObject,
    damageType: DamageType.Physical
);

// Deal custom amount
float actualDamage = dealer.DealDamageToTarget(
    target: enemyGameObject,
    customDamage: 50f,
    damageType: DamageType.Magic
);
```

**Features:**
- Applies attacker's damage modifier buffs
- Automatically applies target's resistances
- Supports lifesteal from buffs
- Returns actual damage dealt

### Querying Damage

```csharp
// Get effective damage with buffs applied
float effectiveDamage = dealer.GetEffectiveDamage();

// Check if has damage bonus
bool hasBonus = dealer.HasDamageBonus();
```

### Self Healing

```csharp
// Dealer can heal itself (for lifesteal mechanics)
dealer.Heal(20f, \"lifesteal\");
```

## Damage Types

### Physical Damage
Reduced by **Armor** modifier:

```csharp
health.TakeDamage(30f, \"slash\", DamageType.Physical);

// Armor reduces this damage
// Final Damage = BaseDamage - Armor
```

### Magic Damage
Reduced by **Magic Resistance** modifier:

```csharp
health.TakeDamage(25f, \"fireball\", DamageType.Magic);

// Magic resistance reduces this damage
// Final Damage = BaseDamage - MagicResistance
```

### True Damage
Bypasses all resistances:

```csharp
health.TakeDamage(20f, \"execution\", DamageType.True);

// Ignores armor, magic resistance, etc
// Final Damage = BaseDamage (only affected by DAMAGE_TAKEN modifiers)
```

## Predefined Effects

### Health Effects

```csharp
// Armor buff
new ArmorEffect(\"steel_skin\", duration: -1f, armorAmount: 30f);

// Magic resistance
new MagicResistanceEffect(\"shield\", 10f, 20f);

// Increase max health
new MaxHealthBuff(\"boost\", 5f, 50f, maxStacks: 3);

// Passive health regeneration
new HealthRegenEffect(\"regen\", -1f, 5f);  // 5 HP/sec

// Invulnerability (temporary)
new InvulnerabilityEffect(\"shield_spell\", 3f);

// Crowd control reduction
new TenacityEffect(\"steadfast\", -1f, 0.3f);

// Increase healing received
new HealingReceivedBuff(\"blessed\", 15f, 0.25f);

// More damage taken (debuff)
new VulnerabilityEffect(\"brittle\", 8f, 0.5f);

// Convert damage to healing
new LifestealEffect(\"vampirism\", -1f, 0.2f);  // 20% lifesteal

// Combined health + armor buff
new FortifyEffect(\"fortify\", 10f, 30f, 40f);  // 30 health, 40 armor
```

## Common Usage Patterns

### Character Taking an Attack

```csharp
public void TakeDamageFromAttack(GameObject attacker, float baseDamage)
{
    float actualDamage = health.TakeDamage(
        baseDamage, 
        attacker.name, 
        DamageType.Physical
    );

    Debug.Log($\"Took {actualDamage} damage\");

    if (health.IsDead)
    {
        OnDeath();
    }
}
```

### Enemy Attacking Another Entity

```csharp
public void Attack(GameObject target)
{
    DamageDealer dealer = GetComponent<DamageDealer>();
    
    // Calculate with attacker's buffs
    float effectiveDamage = dealer.GetEffectiveDamage();
    
    // Deal to target
    float actualDamage = dealer.DealDamageToTarget(
        target, 
        DamageType.Physical
    );
    
    Debug.Log($\"Dealt {actualDamage} damage\");
}
```

### Healing Spell

```csharp
public float CastHeal(GameObject target, float baseHealing)
{
    HealthComponent targetHealth = target.GetComponent<HealthComponent>();
    
    if (targetHealth == null || targetHealth.IsDead)
        return 0;
    
    float actualHealing = targetHealth.Heal(baseHealing, \"spell\");
    return actualHealing;
}
```

### Applying Protective Buffs

```csharp
public void ApplyShield(float armorAmount, float duration)
{
    var armor = new ArmorEffect(\"shield\", duration, armorAmount);
    GetComponent<BuffDebuffSystem>().AddEffect(armor);
}
```

### Temporary Invulnerability

```csharp
public void BecomInvulnerable(float duration)
{
    var invuln = new InvulnerabilityEffect(\"dodge\", duration);
    GetComponent<BuffDebuffSystem>().AddEffect(invuln);
}

// In health component damage check:
if (GetComponent<BuffDebuffSystem>().HasModifier(HealthModifiers.INVULNERABLE))
    return; // No damage taken
```

### Health Regeneration

```csharp
public void StartRegeneration(float hpPerSecond, float duration)
{
    var regen = new HealthRegenEffect(\"regen\", duration, hpPerSecond);
    GetComponent<BuffDebuffSystem>().AddEffect(regen);
}
```

## Integration with Scriptable Architecture

See **[Health & Damage UI Integration](Health-Damage-UI)** for decoupling UI from game logic using Scriptable Variables.

## Best Practices

1. **Always Provide Damage Source** - Helps with debugging and death messages
   ```csharp
   health.TakeDamage(25f, \"spike_trap\", DamageType.Physical);
   ```

2. **Check IsDead Before Action** - Prevent double-actions on dead entities
   ```csharp
   if (health.IsAlive)
   {
       // Do something
   }
   ```

3. **Use Events for Logic** - Subscribe to health changes
   ```csharp
   health.OnDeath += OnCharacterDeath;
   health.OnHealthChanged += UpdateUI;
   ```

4. **Apply Buffs Before Combat** - Set up modifiers before taking damage
   ```csharp
   buffSystem.AddEffect(armorBuff);
   health.TakeDamage(25f, \"attack\", DamageType.Physical); // Uses armor
   ```

5. **Consider Lifesteal** - Use LifestealEffect for self-healing attacks
   ```csharp
   buffSystem.AddEffect(new LifestealEffect(\"vampirism\", -1f, 0.15f));
   dealer.DealDamageToTarget(target, DamageType.Physical); // Heals on hit
   ```

## See Also

- **[Buff/Debuff System](Buff-Debuff-System)** - Understand modifiers
- **[Health & Damage UI](Health-Damage-UI)** - Connect to UI
- **[Creating Custom Effects](Custom-Effects)** - Build your own effects
- **[Examples](Examples-and-Tutorials)** - Working implementations
