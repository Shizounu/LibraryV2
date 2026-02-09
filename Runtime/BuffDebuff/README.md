# Buff and Debuff System

A flexible, extensible buff/debuff system for Unity that allows you to apply temporary or permanent effects to game objects and have other game systems query the effects to modify their behavior.

## Overview

The system consists of three main components:

- **BuffDebuffSystem**: A MonoBehaviour component that sits on a game object and manages all active effects
- **IBuffDebuffEffect**: An interface that all effects must implement, providing the core effect lifecycle
- **IStatModifier**: An interface for different types of stat modifications that effects can provide

### Update System Integration

The BuffDebuffSystem uses the custom **UpdateSystem** for effect processing instead of Unity's built-in Update method. This allows for:
- **Configurable update intervals** - Effects can update at custom rates (default is every frame)
- **Better performance** - Centralized update management across multiple buff systems
- **Integration with the custom threading system** - Future support for background thread processing if needed

The update interval is exposed as a serializable field and can be changed at runtime through the `UpdateInterval` property.

## Quick Start

### 1. Add the System to a Game Object

```csharp
// Create a game object and add the BuffDebuffSystem component
var gameObject = new GameObject("Player");
var buffSystem = gameObject.AddComponent<BuffDebuffSystem>();
```

### 2. Create and Apply an Effect

```csharp
// Create a damage boost effect (30% damage increase for 5 seconds, can stack up to 3 times)
var damageBoost = new DamageBoostEffect("boost_damage", 5f, 0.3f, maxStacks: 3);
buffSystem.AddEffect(damageBoost);
```

### 3. Configure Update Interval (Optional)

```csharp
// By default, BuffDebuffSystem updates every frame (UpdateInterval = 0)
// You can customize the update interval:
buffSystem.UpdateInterval = 0.1f;  // Update effects every 0.1 seconds

// Or set in the Inspector's serialized _updateInterval field
```

### 4. Query Effects in Your Game Systems

```csharp
// In your damage calculation system
float damageMultiplier = buffSystem.GetModifierMultiplier(CommonModifiers.DAMAGE_MULTIPLIER);
float finalDamage = baseDamage * damageMultiplier;

// In your movement system
float speedMultiplier = buffSystem.GetModifierMultiplier(CommonModifiers.MOVEMENT_SPEED_MULTIPLIER);
float effectiveSpeed = baseSpeed * speedMultiplier;
```

## Core Concepts

### Effects

Effects represent temporary or permanent changes applied to an entity. Each effect:
- Has a unique ID for identification and stacking
- Has a duration (negative = infinite)
- Provides stat modifiers that other systems can query
- Manages stacking (multiple applications of the same effect)
- Has lifecycle callbacks (OnApply, OnUpdate, OnRemove)

### Stat Modifiers

Modifiers represent specific changes to game mechanics. Each modifier:
- Has a unique ID (e.g., "damage.multiplier", "movement.speed.flat")
- Has a numeric value
- Can be queried individually or combined with other modifiers

### Common Patterns

#### Additive Modifiers
Apply multiple small bonuses that add together:
```csharp
float totalDamage = baseDamage + buffSystem.GetModifierSum(CommonModifiers.DAMAGE_FLAT);
```

#### Multiplicative Modifiers
Stack bonuses that multiply together:
```csharp
float multiplier = buffSystem.GetModifierMultiplier(CommonModifiers.DAMAGE_MULTIPLIER);
float finalDamage = baseDamage * multiplier;
```

#### Status Checks
Check if an entity has a specific effect:
```csharp
if (buffSystem.HasModifier(CommonModifiers.STUN_IMMUNITY))
{
    // Don't apply stun
}
```

## Creating Custom Effects

Inherit from `BuffDebuffEffect` to create custom effects:

```csharp
public class BerserkEffect : BuffDebuffEffect
{
    public BerserkEffect(string effectID, float duration)
        : base(effectID, duration, maxStackCount: 1)
    {
    }

    public override IStatModifier[] GetModifiers()
    {
        return new IStatModifier[]
        {
            new SimpleStatModifier(CommonModifiers.DAMAGE_MULTIPLIER, 0.5f),  // 50% damage boost
            new SimpleStatModifier(CommonModifiers.ARMOR_FLAT, -10f)          // 10 less armor
        };
    }

    public override void OnApply()
    {
        base.OnApply();
        // Play effect animation, sound, etc.
        Debug.Log("Berserk activated!");
    }

    public override void OnRemove()
    {
        base.OnRemove();
        // Clean up visual/audio effects
        Debug.Log("Berserk ended!");
    }
}
```

## API Reference

### BuffDebuffSystem

#### Properties

- `UpdateInterval` - Get or set how often effects are processed (0 = every frame, positive value = interval in seconds)

#### Methods

- `AddEffect(IBuffDebuffEffect effect)` - Add a new effect or stack an existing one
- `RemoveEffect(string effectID)` - Remove a specific effect
- `ClearAllEffects()` - Remove all active effects
- `GetEffect(string effectID)` - Get a specific effect
- `GetAllEffects()` - Get all active effects
- `GetModifiers(string modifierID)` - Get all modifiers of a type
- `GetModifierSum(string modifierID)` - Sum all modifiers (additive)
- `GetModifierMultiplier(string modifierID)` - Multiply all modifiers
- `HasModifier(string modifierID)` - Check if any effect provides a modifier
- `GetActiveEffectCount()` - Get the number of active effects

#### Events

- `OnEffectAdded(IBuffDebuffEffect effect)` - Fired when an effect is added
- `OnEffectRemoved(IBuffDebuffEffect effect)` - Fired when an effect is removed
- `OnEffectStacked(IBuffDebuffEffect effect)` - Fired when an effect is stacked

### IBuffDebuffEffect

#### Properties

- `EffectID` - Unique identifier for this effect
- `TimeRemaining` - Seconds remaining (negative = infinite)
- `IsActive` - Whether the effect is still active
- `StackCount` - Current number of stacks

#### Methods

- `OnApply()` - Called when effect is first applied
- `OnRemove()` - Called when effect expires or is removed
- `OnUpdate(float deltaTime)` - Called each frame
- `GetModifiers()` - Return all modifiers this effect provides
- `GetModifier(string modifierID)` - Get a specific modifier
- `TryAddStack()` - Attempt to add a stack
- `RefreshDuration()` - Reset the duration timer

## Included Modifiers

The `CommonModifiers` class provides constants for common game mechanics:

```csharp
// Damage
DAMAGE_MULTIPLIER           // Multiplicative damage increase
DAMAGE_FLAT                 // Flat damage increase

// Movement
MOVEMENT_SPEED_MULTIPLIER   // Multiplicative speed
MOVEMENT_SPEED_FLAT         // Flat speed increase

// Defense
ARMOR_MULTIPLIER            // Multiplicative armor boost
ARMOR_FLAT                  // Flat armor points
DAMAGE_REDUCTION            // Direct damage reduction (0-1)

// Abilities
ABILITY_COOLDOWN_MULTIPLIER // Cooldown reduction
ABILITY_DAMAGE_MULTIPLIER   // Ability damage boost

// Status
STUN_IMMUNITY               // Can't be stunned
SLOW_IMMUNITY               // Can't be slowed

// Other
ATTACK_SPEED
CAST_SPEED
CRITICAL_CHANCE
CRITICAL_DAMAGE
```

You can also define your own modifier IDs as strings.

## Example: Integrating with Damage System

```csharp
public class Character : MonoBehaviour
{
    public float baseDamage = 10f;
    private BuffDebuffSystem _buffDebuffSystem;

    private void Start()
    {
        _buffDebuffSystem = GetComponent<BuffDebuffSystem>();
    }

    public float GetEffectiveDamage()
    {
        float damage = baseDamage;
        
        // Apply additive damage modifiers first
        damage += _buffDebuffSystem.GetModifierSum(CommonModifiers.DAMAGE_FLAT);
        
        // Then apply multiplicative modifiers
        float multiplier = _buffDebuffSystem.GetModifierMultiplier(CommonModifiers.DAMAGE_MULTIPLIER);
        damage *= multiplier;
        
        return Mathf.Max(0, damage);
    }

    public void Damage(float incomingDamage)
    {
        // Consider armor/defense
        float armorBonus = _buffDebuffSystem.GetModifierSum(CommonModifiers.ARMOR_FLAT);
        float actualDamage = incomingDamage - armorBonus;
        
        health -= Mathf.Max(0.5f, actualDamage);  // Minimum 0.5 damage
    }
}
```

## Example: Integrating with Movement System

```csharp
public class PlayerMovement : MonoBehaviour
{
    public float baseSpeed = 5f;
    private BuffDebuffSystem _buffDebuffSystem;

    private void Update()
    {
        float effectiveSpeed = GetEffectiveSpeed();
        
        Vector3 input = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        transform.position += input * effectiveSpeed * Time.deltaTime;
    }

    private float GetEffectiveSpeed()
    {
        // Check for stun
        if (_buffDebuffSystem.GetEffect("stun")?.IsActive ?? false)
            return 0;
        
        float speed = baseSpeed;
        
        // Apply multiplicative modifiers
        float multiplier = _buffDebuffSystem.GetModifierMultiplier(CommonModifiers.MOVEMENT_SPEED_MULTIPLIER);
        speed *= multiplier;
        
        // Apply additive modifiers
        speed += _buffDebuffSystem.GetModifierSum(CommonModifiers.MOVEMENT_SPEED_FLAT);
        
        return Mathf.Max(0, speed);
    }
}
```

## Best Practices

1. **Use Constants**: Always use the constant modifier IDs from `CommonModifiers` when possible to avoid typos
2. **Separate Concerns**: Keep effect logic separate from application logic
3. **Subscribe to Events**: Use the events on BuffDebuffSystem for UI updates, effects, etc.
4. **Validate Effects**: Check the effect ID before applying to avoid duplicates if needed
5. **Test Stacking**: Make sure your stacking logic works correctly in edge cases
6. **Performance**: For many effects, cache modifier queries or subscribe to events instead of querying every frame

## Advanced Usage

### Dynamic Effect Creation

```csharp
var effect = new CustomMultiModifierEffect(
    "dynamic_buff",
    duration: 10f,
    (CommonModifiers.DAMAGE_MULTIPLIER, 0.25f),
    (CommonModifiers.ARMOR_FLAT, 15f)
);
buffSystem.AddEffect(effect);
```

### Damage Over Time

```csharp
var poison = new DamageOverTimeEffect(
    "poison",
    duration: 10f,
    damagePerTick: 5f,
    tickInterval: 1f,
    onTick: (damage) => character.TakeDamage(damage)
);
buffSystem.AddEffect(poison);
```

### Event Subscription

```csharp
buffSystem.OnEffectAdded += (effect) => 
{
    Debug.Log($"Buff applied: {effect.EffectID}");
    PlayVisualEffect(effect.EffectID);
};

buffSystem.OnEffectRemoved += (effect) =>
{
    Debug.Log($"Buff removed: {effect.EffectID}");
    StopVisualEffect(effect.EffectID);
};
```

## Files

- `IBuffDebuffEffect.cs` - Core interfaces
- `BuffDebuffEffect.cs` - Abstract base class for effects
- `BuffDebuffSystem.cs` - Main system component
- `CommonModifiers.cs` - Modifier ID constants and SimpleStatModifier
- `ExampleEffects.cs` - Pre-built effect implementations
- `BuffDebuffSystemExample.cs` - Usage examples and integration patterns
