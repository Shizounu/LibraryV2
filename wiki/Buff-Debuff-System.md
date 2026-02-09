# Buff/Debuff System - Complete Guide

The **Buff/Debuff System** is the foundation of Shizounu Library. It provides a flexible framework for applying temporary or permanent effects to game objects.

## Core Concepts

### Effects
An **effect** is a temporary or permanent modification to an entity. Examples:
- Speed boost (temporary)
- Armor buff (can be permanent)
- Stun (temporary debuff)
- Poison (damage over time)

Each effect:
- Has a unique ID for tracking and stacking
- Has a duration (negative = infinite)
- Provides stat modifiers
- Can stack multiple times
- Has lifecycle callbacks (apply, update, remove)

### Modifiers
A **modifier** is a specific numeric change. Examples:
- \"damage.multiplier\": +50%
- \"armor.flat\": +20 points
- \"movement.speed.multiplier\": ×1.25

Modifiers are:
- Identified by string IDs
- Queried by game systems
- Combined from multiple effects
- Either additive or multiplicative

### Stacking
When the same effect is applied multiple times:
- Can stack up to a configured limit
- Each stack increases the effect
- Resets the duration timer
- Increases modifier values

## System Architecture

```
BuffDebuffSystem (MonoBehaviour)
├── Manages active effects
├── Coordinates updates via UpdateSystem
├── Fires events (Added, Removed, Stacked)
└── Provides modifier queries

Effects
├── Buff (positive)
├── Debuff (negative)
└── Custom (your own)
     └── Provides IStatModifier[] modifiers
     │   ├── SimpleStatModifier (flat value)
     │   ├── StackableModifier (scales with stacks)
     │   └── Custom modifiers

Game Systems Query Modifiers
├── Movement system
├── Damage system
├── Health system
└── Your custom systems
```

## Quick Start

### Add to GameObject

```csharp
var buffSystem = gameObject.AddComponent<BuffDebuffSystem>();
```

### Apply an Effect

```csharp
// Create effect
var speedBoost = new SpeedBoostEffect(\"boost_speed\", duration: 5f, bonus: 0.5f);

// Apply it
buffSystem.AddEffect(speedBoost);
```

### Query in Your Systems

```csharp
// In movement code
float speedMult = buffSystem.GetModifierMultiplier(CommonModifiers.MOVEMENT_SPEED_MULTIPLIER);
float finalSpeed = baseSpeed * speedMult;

// In damage code
float damageMult = buffSystem.GetModifierMultiplier(CommonModifiers.DAMAGE_MULTIPLIER);
float finalDamage = baseDamage * damageMult;

// Check for specific effect
if (buffSystem.GetEffect(\"stun\")?.IsActive ?? false)
    return 0; // Can't move while stunned
```

## Creating Custom Effects

Inherit from `BuffDebuffEffect`:

```csharp
public class FireEffectLike : BuffDebuffEffect
{
    private float damagePerSecond;

    public FireEffectLike(string id, float duration, float dps)
        : base(id, duration, maxStackCount: 1)
    {
        damagePerSecond = dps;
    }

    // Define what modifiers this effect provides
    public override IStatModifier[] GetModifiers()
    {
        return new[]
        {
            // 5% movement speed reduction
            new SimpleStatModifier(CommonModifiers.MOVEMENT_SPEED_MULTIPLIER, -0.05f)
        };
    }

    // Called when effect is first applied
    public override void OnApply()
    {
        base.OnApply();
        Debug.Log(\"On fire!\");
    }

    // Called every frame/update
    public override void OnUpdate(float deltaTime)
    {
        base.OnUpdate(deltaTime);
        
        // Apply damage over time
        // Get the health component and damage it
        // (Implementation depends on your setup)
    }

    // Called when effect expires or is removed
    public override void OnRemove()
    {
        base.OnRemove();
        Debug.Log(\"Fire extinguished\");
    }
}
```

### More Complex Example

```csharp
public class StunEffect : BuffDebuffEffect
{
    private Color originalColor;
    private Renderer rend;

    public StunEffect(Renderer renderer, float duration)
        : base(\"stun\", duration, maxStackCount: 3)
    {
        rend = renderer;
    }

    public override IStatModifier[] GetModifiers()
    {
        return new[]
        {
            // Movement locked
            new SimpleStatModifier(CommonModifiers.MOVEMENT_SPEED_FLAT, -1000f),
            // Can't act
            new SimpleStatModifier(\"ability.disabled\", 1f)
        };
    }

    public override void OnApply()
    {
        base.OnApply();
        originalColor = rend.material.color;
        rend.material.color = Color.blue; // Visual feedback
    }

    public override void OnRemove()
    {
        base.OnRemove();
        rend.material.color = originalColor; // Restore
    }
}

// Usage
var stun = new StunEffect(GetComponent<Renderer>(), 3f);
buffSystem.AddEffect(stun);
buffSystem.AddEffect(stun); // Stacks! Now 2 stacks
```

## Included Effects

The library provides common predefined effects:

### From CommonModifiers

```csharp
// Damage modifiers
new SimpleStatModifier(CommonModifiers.DAMAGE_MULTIPLIER, 0.5f);      // +50%
new SimpleStatModifier(CommonModifiers.DAMAGE_FLAT, 10f);             // +10 flat

// Movement modifiers
new SimpleStatModifier(CommonModifiers.MOVEMENT_SPEED_MULTIPLIER, 0.25f); // +25%
new SimpleStatModifier(CommonModifiers.MOVEMENT_SPEED_FLAT, 2f);      // +2 units/sec

// Defense modifiers
new SimpleStatModifier(CommonModifiers.ARMOR_FLAT, 15f);              // +15 armor
new SimpleStatModifier(CommonModifiers.ARMOR_MULTIPLIER, 0.3f);       // +30% armor

// Ability modifiers
new SimpleStatModifier(CommonModifiers.ABILITY_COOLDOWN_MULTIPLIER, -0.25f); // 25% cooldown reduction
new SimpleStatModifier(CommonModifiers.ABILITY_DAMAGE_MULTIPLIER, 0.5f);     // +50% ability damage

// Special
new SimpleStatModifier(\"stun.immune\", 1f); // Flag-type modifier

// Attack/Cast speed
new SimpleStatModifier(CommonModifiers.ATTACK_SPEED, 0.3f);
new SimpleStatModifier(CommonModifiers.CAST_SPEED, 0.5f);
```

## BuffDebuffSystem API

### Properties

```csharp
// Get/set how often effects update (0 = every frame)
buffSystem.UpdateInterval = 0.1f;
```

### Core Methods

```csharp
// Add or stack an effect
buffSystem.AddEffect(effect);

// Remove specific effect
buffSystem.RemoveEffect(\"effect_id\");

// Remove all effects
buffSystem.ClearAllEffects();

// Get specific effect
IBuffDebuffEffect effect = buffSystem.GetEffect(\"stun\");

// Get all active effects
IReadOnlyList<IBuffDebuffEffect> effects = buffSystem.GetAllEffects();

// Get count of active effects
int count = buffSystem.GetActiveEffectCount();
```

### Modifier Queries

```csharp
// Get all modifiers of a type
IReadOnlyList<IStatModifier> mods = buffSystem.GetModifiers(\"damage.flat\");

// Sum all additive modifiers
float totalArmor = buffSystem.GetModifierSum(CommonModifiers.ARMOR_FLAT);

// Multiply all multiplicative modifiers
float damageBonus = buffSystem.GetModifierMultiplier(CommonModifiers.DAMAGE_MULTIPLIER);

// Check if any effect provides a modifier
bool hasStunImmune = buffSystem.HasModifier(\"stun.immune\");
```

### Events

```csharp
// When effect is added
buffSystem.OnEffectAdded += (effect) => 
{
    Debug.Log($\"{effect.EffectID} applied\");
};

// When effect expires or is removed
buffSystem.OnEffectRemoved += (effect) => 
{
    Debug.Log($\"{effect.EffectID} removed\");
};

// When effect is stacked
buffSystem.OnEffectStacked += (effect) => 
{
    Debug.Log($\"{effect.EffectID} now has {effect.StackCount} stacks\");
};
```

## IBuffDebuffEffect API

### Properties

```csharp
// Unique identifier
string id = effect.EffectID;

// Time remaining (-1 = infinite)
float remaining = effect.TimeRemaining;

// Is still active
bool active = effect.IsActive;

// Current stack count
int stacks = effect.StackCount;
```

### Methods

```csharp
// Get all modifiers this effect provides
IStatModifier[] mods = effect.GetModifiers();

// Get specific modifier
IStatModifier mod = effect.GetModifier(CommonModifiers.ARMOR_FLAT);

// Try to add another stack
bool stacked = effect.TryAddStack();

// Reset the duration timer
effect.RefreshDuration();

// Lifecycle callbacks
effect.OnApply();      // First application
effect.OnUpdate(dt);   // Each frame
effect.OnRemove();     // When expires/removed
```

## Common Patterns

### Damage Boost with Stacking

```csharp
public class DamageBoostEffect : BuffDebuffEffect
{
    public DamageBoostEffect(string id, float duration, float bonusPerStack)
        : base(id, duration, maxStackCount: 3)
    {
    }

    public override IStatModifier[] GetModifiers()
    {
        // Bonus scales with stack count
        float totalBonus = 0.1f * StackCount; // 10% per stack
        
        return new[]
        {
            new SimpleStatModifier(CommonModifiers.DAMAGE_MULTIPLIER, totalBonus)
        };
    }
}

// Usage
var boost = new DamageBoostEffect(\"attack_boost\", 5f, 0.1f);
buffSystem.AddEffect(boost); // 1 stack: 10% bonus
buffSystem.AddEffect(boost); // 2 stacks: 20% bonus
buffSystem.AddEffect(boost); // 3 stacks: 30% bonus
buffSystem.AddEffect(boost); // Still 3 stacks (max reached)
```

### Dispellable Debuff

```csharp
public class SlowEffect : BuffDebuffEffect
{
    public SlowEffect(float duration, float slowAmount)
        : base(\"slow\", duration, maxStackCount: 1)
    {
    }

    public override IStatModifier[] GetModifiers()
    {
        return new[]
        {
            new SimpleStatModifier(CommonModifiers.MOVEMENT_SPEED_MULTIPLIER, -0.5f)
        };
    }
}

// Apply
var slow = new SlowEffect(3f, 0.5f);
buffSystem.AddEffect(slow);

// Dispel (remove)
if (HasDispel)
{
    buffSystem.RemoveEffect(\"slow\");
}
```

### Temporary Immunity

```csharp
public class InvulnerabilityEffect : BuffDebuffEffect
{
    public InvulnerabilityEffect(float duration)
        : base(\"invuln\", duration, maxStackCount: 1)
    {
    }

    public override IStatModifier[] GetModifiers()
    {
        return new[]
        {
            // Flag modifier - checked for presence, not value
            new SimpleStatModifier(\"invulnerable\", 1f)
        };
    }

    public override void OnApply()
    {
        base.OnApply();
        Debug.Log(\"Invulnerable!\");
    }
}

// Check immunity
if (buffSystem.HasModifier(\"invulnerable\"))
{
    return; // Don't take damage
}
```

## Best Practices

1. **Use Constants** - Always use `CommonModifiers` constants instead of magic strings
   ```csharp
   // Good
   buffSystem.GetModifierMultiplier(CommonModifiers.DAMAGE_MULTIPLIER);
   
   // Bad
   buffSystem.GetModifierMultiplier(\"damage\");
   ```

2. **Unique Effect IDs** - Always use unique IDs for tracking
   ```csharp
   new ArmorEffect(\"suit_of_armor_1\", -1f, 50f); // Unique ID
   ```

3. **Subscribe to Events** - Don't poll, use events
   ```csharp
   buffSystem.OnEffectAdded += OnBuffApplied;
   buffSystem.OnEffectRemoved += OnBuffExpired;
   ```

4. **Separate Concerns** - Keep effect logic separate from application logic
   ```csharp
   // Effect defines what it does
   public override IStatModifier[] GetModifiers() { ... }
   
   // Game system uses the modifiers
   float damage = baseDamage * buffSystem.GetModifierMultiplier(...);
   ```

5. **Clean Up** - Remove subscriptions when no longer needed
   ```csharp
   private void OnDestroy()
   {
       buffSystem.OnEffectAdded -= OnBuffApplied;
   }
   ```

## See Also

- **[Health & Damage System](Health-Damage-System)** - Uses buffs for resistances and healing
- **[Custom Effects](Custom-Effects)** - Deep dive into creating effects
- **[Examples](Examples-and-Tutorials)** - Working implementations
- **[API Reference](API-Reference)** - Complete API listing
