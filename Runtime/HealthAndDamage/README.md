# Health and Damage System

A comprehensive health and damage system that seamlessly integrates with the BuffDebuff system. This system manages entity health, damage calculations, healing, and status effects with full support for buff/debuff modifiers.

## Overview

The Health and Damage system provides:

- **HealthComponent**: Core health management for game objects
- **DamageDealer**: Helper for dealing damage with modifier calculations
- **Damage Types**: Physical, Magic, and True damage with appropriate resistances
- **Predefined Effects**: Common health-related buffs and debuffs
- **Event System**: Subscribe to health changes, death, healing, and more
- **Modifier Integration**: Fully integrated with BuffDebuff system modifiers

## Core Components

### HealthComponent

Manages health for a game object. Add this alongside `BuffDebuffSystem`.

**Key Properties:**
- `CurrentHealth`: Current health value
- `BaseMaxHealth`: Base max health (before buffs)
- `EffectiveMaxHealth`: Max health including all buffs/debuffs
- `HealthPercent`: Health as 0-1 percentage
- `IsDead` / `IsAlive`: Death state

**Key Methods:**
- `TakeDamage(float damage, string source, DamageType type)`: Apply damage with resistances
- `TakeDamageRaw(float damage)`: Apply raw damage ignoring modifiers
- `Heal(float amount, string source)`: Heal with modifiers applied
- `FullRestore()`: Restore to maximum health
- `Die(string source)`: Kill immediately
- `Revive(float healthPercent)`: Revive with specified health

**Key Events:**
- `OnHealthChanged(oldHealth, newHealth, maxHealth)`
- `OnDeath(damageSource)`
- `OnRevived()`
- `OnFullyHealed()`
- `OnMaxHealthChanged(oldMax, newMax)`

**Setup:**
```csharp
// Create a game object with both components
healthComponent = GetComponent<HealthComponent>();
healthComponent.SetMaxHealth(100f);
healthComponent.OnDeath += HandleDeath;

// Take damage
float actualDamage = healthComponent.TakeDamage(25f, "enemy_attack", DamageType.Physical);

// Heal
float actualHealing = healthComponent.Heal(15f, "potion");
```

### DamageDealer

Helper component for dealing damage. Add to the attacker's game object.

**Key Methods:**
- `GetEffectiveDamage()`: Calculate damage with attacker's buffs
- `DealDamageToTarget(GameObject target, DamageType type)`: Deal damage to target
- `DealDamageToTarget(GameObject target, float customDamage, DamageType type)`: Deal custom damage
- `HasDamageBonus()`: Check if attacker has damage buffs

**Features:**
- Applies attacker's damage modifier buffs
- Applies target's resistances and armor
- Supports lifesteal from buffs

**Setup:**
```csharp
damageDealer = GetComponent<DamageDealer>();
damageDealer.SetBaseDamage(20f);
float actualDamage = damageDealer.DealDamageToTarget(enemy, DamageType.Physical);
```

## Health Modifiers

Custom modifier IDs for the Health system, used with BuffDebuff effects:

### Damage Modifiers
- `DAMAGE_TAKEN_MULTIPLIER`: Multiplicative incoming damage (% increase)
- `DAMAGE_TAKEN_FLAT`: Flat incoming damage modification
- `ARMOR`: Physical armor (reduces physical damage)
- `MAGIC_RESISTANCE`: Magic resistance (reduces magic damage)
- `TENACITY`: Crowd control reduction

### Healing Modifiers
- `HEALING_RECEIVED_MULTIPLIER`: Incoming healing multiplier
- `HEALING_DEALT_MULTIPLIER`: Outgoing healing multiplier
- `LIFESTEAL_PERCENT`: Damage converted to healing (0-1)

### Health Pool Modifiers
- `MAX_HEALTH_MULTIPLIER`: Multiplicative max health increase
- `MAX_HEALTH_FLAT`: Additive max health increase
- `HEALTH_REGENERATION`: Health restored per second

### Status Modifiers
- `INVULNERABLE`: Immunity to all damage
- `UNSTOPPABLE`: Cannot be controlled (future use)

## Predefined Effects

### ArmorEffect
Increases physical armor, reducing incoming physical damage.

```csharp
var armor = new ArmorEffect("iron_skin", duration: 10f, armorAmount: 30f);
buffSystem.AddEffect(armor);
```

### MagicResistanceEffect
Increases magic resistance, reducing incoming magic damage.

```csharp
var magicShield = new MagicResistanceEffect("magic_shield", 8f, 0.2f); // 20% reduction
buffSystem.AddEffect(magicShield);
```

### MaxHealthBuff
Temporarily increases max health.

```csharp
var healthBoost = new MaxHealthBuff("constitution_boost", 15f, 50f, isFlatBonus: true);
buffSystem.AddEffect(healthBoost);
```

### HealthRegenEffect
Provides passive health regeneration per second.

```csharp
var regen = new HealthRegenEffect("regeneration", -1f, 5f); // 5 HP/sec, infinite
buffSystem.AddEffect(regen);
```

### InvulnerabilityEffect
Complete immunity to all damage.

```csharp
var invuln = new InvulnerabilityEffect("god_mode", 3f);
buffSystem.AddEffect(invuln);
```

### TenacityEffect
Reduces crowd control effects.

```csharp
var steadfast = new TenacityEffect("steadfast", 10f, 0.5f); // 50% CC reduction
buffSystem.AddEffect(steadfast);
```

### HealingReceivedBuff
Increases incoming healing.

```csharp
var healBoost = new HealingReceivedBuff("holy_blessing", 12f, 0.25f); // 25% more healing
buffSystem.AddEffect(healBoost);
```

### VulnerabilityEffect
Increases incoming damage (weakness/curse).

```csharp
var vulnerability = new VulnerabilityEffect("weakness", 5f, 0.5f); // 50% more damage
buffSystem.AddEffect(vulnerability);
```

### LifestealEffect
Converts damage dealt to healing.

```csharp
var lifesteal = new LifestealEffect("vampirism", 10f, 0.3f); // 30% of damage as healing
buffSystem.AddEffect(lifesteal);
```

### FortifyEffect
Combined effect for both health and armor.

```csharp
var fortify = new FortifyEffect("fortification", 8f, maxHealthBonus: 50f, armorBonus: 25f);
buffSystem.AddEffect(fortify);
```

## Damage Types

Three damage types with different resistance applications:

```csharp
public enum DamageType
{
    Physical,  // Reduced by ARMOR
    Magic,     // Reduced by MAGIC_RESISTANCE
    True       // Bypasses all resistances
}
```

**Example:**
```csharp
// Physical damage affected by armor
healthComponent.TakeDamage(30f, "slash", DamageType.Physical);

// Magic damage affected by magic resistance
healthComponent.TakeDamage(25f, "fireball", DamageType.Magic);

// True damage ignores all resistances
healthComponent.TakeDamage(20f, "execution", DamageType.True);
```

## Integration with BuffDebuff System

The Health and Damage system fully integrates with BuffDebuff:

### Damage Calculation Flow

```
Base Damage
    ↓
[Armed with buffs on attacker]
    ├─ Add DAMAGE_FLAT bonuses
    └─ Multiply by DAMAGE_MULTIPLIER
    ↓
Applied to Target
    ↓
[Target resistances based on damage type]
    ├─ Physical: Reduced by ARMOR
    ├─ Magic: Reduced by MAGIC_RESISTANCE
    └─ True: No reduction
    ↓
[Target damage intake modifiers]
    ├─ Add DAMAGE_TAKEN_FLAT
    └─ Multiply by DAMAGE_TAKEN_MULTIPLIER
    ↓
Check INVULNERABLE (blocks all damage)
    ↓
Apply to Health
    ↓
[Lifesteal if attacker has LIFESTEAL_PERCENT]
```

### Healing Calculation

```
Base Healing
    ↓
Multiply by HEALING_RECEIVED_MULTIPLIER
    ↓
Apply to Health
    ↓
Check if FULLY_HEALED
```

### Max Health Calculation

```
Base Max Health
    ↓
Add MAX_HEALTH_FLAT bonuses
    ↓
Multiply by MAX_HEALTH_MULTIPLIER
    ↓
Result = Effective Max Health
```

## Usage Examples

### Basic Health System

```csharp
public class Player : MonoBehaviour
{
    private HealthComponent health;
    private BuffDebuffSystem buffs;

    private void Start()
    {
        health = GetComponent<HealthComponent>();
        buffs = GetComponent<BuffDebuffSystem>();
        
        health.SetMaxHealth(100f);
        health.OnDeath += OnPlayerDeath;
    }

    public void TakeDamageFromAttack(float attackDamage)
    {
        float actualDamage = health.TakeDamage(
            attackDamage, 
            "enemy_attack", 
            DamageType.Physical
        );
        Debug.Log($"Took {actualDamage} damage");
    }

    private void OnPlayerDeath(string source)
    {
        Debug.Log($"Defeated by: {source}");
        // Handle death
    }
}
```

### Combat System

```csharp
public class Combat : MonoBehaviour
{
    private DamageDealer dealer;

    private void Start()
    {
        dealer = GetComponent<DamageDealer>();
        dealer.SetBaseDamage(25f);
    }

    public void Attack(GameObject target)
    {
        float damage = dealer.GetEffectiveDamage();
        dealer.DealDamageToTarget(target, DamageType.Physical);
        Debug.Log($"Attack dealt {damage} damage!");
    }
}
```

### Boss with Complex Effects

```csharp
public class Boss : MonoBehaviour
{
    private HealthComponent health;
    private BuffDebuffSystem buffs;

    private void Start()
    {
        health = GetComponent<HealthComponent>();
        buffs = GetComponent<BuffDebuffSystem>();
        
        health.SetMaxHealth(500f);
        
        // Boss has permanent armor and regeneration
        var armor = new ArmorEffect("boss_armor", -1f, 50f);
        var regen = new HealthRegenEffect("boss_regen", -1f, 10f);
        
        buffs.AddEffect(armor);
        buffs.AddEffect(regen);
    }

    public void EnterVulnerablePhase()
    {
        // Remove armor temporarily
        buffs.RemoveEffect("boss_armor");
        
        // Apply vulnerability
        var weak = new VulnerabilityEffect("weakpoint", 5f, 0.5f);
        buffs.AddEffect(weak);
    }
}
```

### Healing Potion

```csharp
public void UseHealthPotion(GameObject target, float healAmount = 50f)
{
    HealthComponent health = target.GetComponent<HealthComponent>();
    if (health != null)
    {
        float actualHealing = health.Heal(healAmount, "potion");
        Debug.Log($"Restored {actualHealing} health");
    }
}
```

### Buff Application

```csharp
public void ApplyStrengthBuff(GameObject target)
{
    BuffDebuffSystem buffs = target.GetComponent<BuffDebuffSystem>();
    if (buffs != null)
    {
        // Create custom damage buff
        var strengthBuff = new CustomMultiModifierEffect(
            "strength_blessing",
            duration: 30f,
            (CommonModifiers.DAMAGE_MULTIPLIER, 0.3f), // 30% damage boost
            (HealthModifiers.MAX_HEALTH_FLAT, 25f)      // +25 max HP
        );
        buffs.AddEffect(strengthBuff);
    }
}
```

## Best Practices

1. **Always use DamageType**: Specify whether damage is Physical, Magic, or True so resistances apply correctly.

2. **Subscribe to Events**: Use OnDeath and OnHealthChanged to trigger game logic:
   ```csharp
   health.OnDeath += HandleDefeat;
   health.OnFullyHealed += PlayRestoreSound;
   ```

3. **Check Before Operations**: Always verify components exist:
   ```csharp
   HealthComponent health = GetComponent<HealthComponent>();
   if (health != null) health.TakeDamage(50f, "attack", DamageType.Physical);
   ```

4. **Use Modifier Constants**: Always use the predefined modifier IDs:
   ```csharp
   // Good
   buffs.GetModifierSum(HealthModifiers.ARMOR);
   
   // Avoid magic strings
   // buffs.GetModifierSum("my.armor");
   ```

5. **Create Reusable Effects**: Extend BuffDebuffEffect for your game's specific needs instead of creating custom effects inline.

6. **Test Modifier Stacking**: Different effects might stack differently. Test your modifier combinations.

## Architecture Notes

- **HealthComponent** updates passively through the UpdateSystem (health regen)
- **BuffDebuff modifiers** are applied dynamically on each damage/heal calculation
- **Events** allow external systems to react to health changes without tight coupling
- **Prefabricated effects** can be reused and stacked on multiple entities

## Related Systems

- **BuffDebuffSystem**: Manages effects and modifiers
- **UpdateSystem**: Handles passive health regeneration updates

## Performance Considerations

- Health calculations are O(1) - no loops
- Modifier queries are O(n) where n = active effects - typically small
- Events should not create garbage in callbacks
- Consider pooling effect instances for frequently used buffs/debuffs
