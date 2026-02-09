# Best Practices & Design Patterns

Guidelines for using Shizounu Library effectively in your projects.

## General Design Principles

### 1. Decouple Everything

❌ **Bad** - Tight coupling:
```csharp
public class PlayerUI : MonoBehaviour
{
    private HealthComponent gameHealth; // Direct reference!
    
    private void Update()
    {
        healthText.text = gameHealth.CurrentHealth.ToString();
    }
}
```

✅ **Good** - Decoupled via ScriptableVariable:
```csharp
public class PlayerUI : MonoBehaviour
{
    public ScriptableFloat playerHealth;
    
    private void Start()
    {
        playerHealth.OnRuntimeValueChanged += UpdateUI;
    }
}
```

**Benefits:**
- UI works without gameplay
- Easy to test
- Reusable across projects
- Network-friendly

### 2. Use Events, Not Polling

❌ **Bad** - Polling:
```csharp
private void Update()
{
    if (health.IsDead) { /* reaction */ }
}
```

✅ **Good** - Events:
```csharp
private void Start()
{
    health.OnDeath += OnDeath;
}

private void OnDeath(string source) { /* reaction */ }
```

**Benefits:**
- Cleaner code
- No frame cost if nothing changes
- Clear cause → effect
- Prevents double-processing

### 3. Group Related Components

❌ **Bad** - Scatter:
```csharp
gameObject.AddComponent<HealthComponent>();
gameObject.AddComponent<DamageDealer>();
someOtherScript.AddComponent<BuffDebuffSystem>();
```

✅ **Good** - Setup method:
```csharp
public void SetupCombatEntity(GameObject obj)
{
    obj.AddComponent<HealthComponent>().SetMaxHealth(100f);
    obj.AddComponent<BuffDebuffSystem>();
    obj.AddComponent<DamageDealer>().SetBaseDamage(20f);
}
```

**Benefits:**
- Clear what components work together
- Easy to replicate in other game objects
- Self-documenting code

## Buff/Debuff System Practices

### 1. Use Constant Modifier IDs

❌ **Bad** - Magic strings:
```csharp
buffSystem.GetModifierMultiplier(\"damage\");
buffSystem.GetModifierMultiplier(\"movement\");
```

✅ **Good** - Constants:
```csharp
buffSystem.GetModifierMultiplier(CommonModifiers.DAMAGE_MULTIPLIER);
buffSystem.GetModifierMultiplier(CommonModifiers.MOVEMENT_SPEED_MULTIPLIER);
```

**Benefits:**
- Prevents typos
- IDE autocomplete
- Refactoring safe
- Self-documenting

### 2. Create Custom Effect Classes

❌ **Bad** - Inline modifiers:
```csharp
var effect = new BuffDebuffEffect(\"stun\", 3f);
// Can't easily add behavior
```

✅ **Good** - Custom class:
```csharp
public class StunEffect : BuffDebuffEffect
{
    public StunEffect(float duration)
        : base(\"stun\", duration, maxStackCount: 1) { }

    public override IStatModifier[] GetModifiers()
    {
        return new[]
        {
            new SimpleStatModifier(CommonModifiers.MOVEMENT_SPEED_MULTIPLIER, -1f)
        };
    }

    public override void OnApply()
    {
        base.OnApply();
        // Add visual/sound effects
    }
}
```

**Benefits:**
- Reusable
- Consistent behavior
- Easy to extend
- Clear intent

### 3. Handle Effect Removal

❌ **Bad** - Forget cleanup:
```csharp
buffSystem.AddEffect(effect);
// Enemy dies but buff still runs?
```

✅ **Good** - Cleanup:
```csharp
private void OnDeath()
{
    buffSystem.ClearAllEffects();
    Destroy(gameObject);
}
```

**Benefits:**
- No memory leaks
- No ghost effects
- Predictable behavior

## Health System Best Practices

### 1. Always Provide Damage Source

❌ **Bad** - No source:
```csharp
health.TakeDamage(25f);
```

✅ **Good** - Clear source:
```csharp
health.TakeDamage(25f, \"spike_trap\", DamageType.Physical);
health.TakeDamage(35f, \"enemy_fireball\", DamageType.Magic);
```

**Benefits:**
- Debugging easier
- Better death messages
- Track damage sources
- Analytics/logging

### 2. Check IsDead Before Actions

❌ **Bad** - Possible double-death:
```csharp
dealer.DealDamageToTarget(enemy, DamageType.Physical);
// Enemy might be dead now
dealer.DealDamageToTarget(enemy, DamageType.Physical); // Dead entity!
```

✅ **Good** - State check:
```csharp
if (enemy.health.IsAlive)
{
    dealer.DealDamageToTarget(enemy, DamageType.Physical);
}
```

**Benefits:**
- No undefined behavior
- Clear state transitions
- Better debugging

### 3. Subscribe to Health Events

❌ **Bad** - Poll health:
```csharp
float lastHealth = 0;

private void Update()
{
    if (health.CurrentHealth != lastHealth)
    {
        OnHealthChanged();
        lastHealth = health.CurrentHealth;
    }
}
```

✅ **Good** - Use events:
```csharp
private void Start()
{
    health.OnHealthChanged += OnHealthChanged;
    health.OnDeath += OnDeath;
}
```

**Benefits:**
- No polling cost
- Cleaner code
- Professional architecture

## Scriptable Architecture Practices

### 1. Organize by Domain

```
Assets/ScriptableArchitecture/
├── Player/
│   ├── PlayerCurrentHealth
│   ├── PlayerMaxHealth
│   ├── PlayerScore
│   └── PlayerInventory
├── Enemy/
│   ├── EnemyHealth
│   ├── EnemyCount
│   └── EnemyDifficulty
├── Game/
│   ├── GameState
│   ├── GamePaused
│   └── CurrentLevel
└── Events/
    ├── PlayerDied
    ├── EnemyDefeated
    └── LevelComplete
```

**Benefits:**
- Easy to find variables
- Clear organization
- Scale as project grows

### 2. Use Descriptive Names

❌ **Bad** - Vague:
```csharp
health_1, health_2, hp, x, health_var
```

✅ **Good** - Explicit:
```csharp
playerCurrentHealth
playerMaxHealth
playerHealthPercent
bossCurrentHealth
enemyGroupHealth
```

**Benefits:**
- Self-documenting
- No confusion
- Easy collaboration

### 3. Separate Data from Logic

❌ **Bad** - Mixed:
```csharp
public class PlayerData : MonoBehaviour
{
    public float health;
    
    private void Update()
    {
        // Game logic mixed with data
    }
}
```

✅ **Good** - Separated:
```csharp
// Just data
public ScriptableFloat playerHealth;

// Logic in separate class
public class HealthSystem : MonoBehaviour
{
    public ScriptableFloat healthVariable;
    
    // Pure logic
    private void Update() { }
}
```

**Benefits:**
- Testable
- Reusable
- Clear separation
- Easy to change

## Code Organization Tips

### 1. Single Responsibility

Each class should do one thing:

```csharp
// ❌ Bad - Does too much
public class Player : MonoBehaviour
{
    private HealthComponent health;
    private BuffDebuffSystem buffs;
    private CharacterController controller;
    private Animator animator;
    private UIController ui;
    // Too many responsibilities!
}

// ✅ Good - Single responsibility
public class PlayerController : MonoBehaviour
{
    public float baseSpeed = 5f;
    
    // Only handles movement
    private void Update() { }
}

public class PlayerCombat : MonoBehaviour
{
    // Only handles damage
    public void DealDamage(GameObject target) { }
}
```

### 2. Use Composition

❌ **Bad** - Inheritance:
```csharp
public class Enemy : Character { }
public class Player : Character { }
// What if player should be different?
```

✅ **Good** - Composition:
```csharp
public class Character : MonoBehaviour
{
    public HealthComponent health;
    public BuffDebuffSystem buffs;
    // Add or remove as needed
}
```

### 3. Keep Methods Small

❌ **Bad** - Long method:
```csharp
public void TakeDamage(float damage, string source)
{
    // 50 lines of calculation
    // 50 lines of logic
    // 50 lines of effects
}
```

✅ **Good** - Small, focused:
```csharp
public void TakeDamage(float damage, string source, DamageType type)
{
    float actualDamage = CalculateDamage(damage, type);
    ApplyDamage(actualDamage);
    NotifyDamageTaken(actualDamage, source);
}

private float CalculateDamage(float baseDamage, DamageType type) { }
private void ApplyDamage(float damage) { }
private void NotifyDamageTaken(float damage, string source) { }
```

## Performance Considerations

### 1. Use Update Intervals

```csharp
// For effects that don't need every-frame updates
buffSystem.UpdateInterval = 0.1f; // Update every 0.1 seconds instead of every frame
```

**Saves:** Frame processing, memory allocation

### 2. Cache Components

❌ **Bad** - GetComponent in Update:
```csharp
private void Update()
{
    health.TakeDamage(1f);
    // GetComponent called every frame!
}
```

✅ **Good** - Cache in Start:
```csharp
private void Start()
{
    health = GetComponent<HealthComponent>();
}

private void Update()
{
    health.TakeDamage(1f);
}
```

### 3. Batch Updates

```csharp
// ✅ Good - One loop
foreach (var enemy in enemies)
{
    enemy.health.TakeDamage(aoe_damage, \"explosion\", DamageType.Physical);
}

// ❌ Bad - Call from many places
foreach (var enemy in enemies)
{
    DamageEnemy(enemy); // Gets called differently in different contexts
}
```

## Testing Tips

### Unit Test Patterns

```csharp
[TestFixture]
public class HealthComponentTests
{
    [Test]
    public void TakeDamage_WithArmor_ReducesDamage()
    {
        // Arrange
        var health = new HealthComponent();
        var buffs = new BuffDebuffSystem();
        buffs.AddEffect(new ArmorEffect(\"armor\", -1f, 20f));
        
        // Act
        float actualDamage = health.TakeDamage(50f, \"test\", DamageType.Physical);
        
        // Assert
        Assert.AreEqual(30f, actualDamage); // 50 - 20 armor
    }
}
```

### Scene Testing

1. Create a test scene with all components
2. Use keyboard inputs to test behavior
3. Log output to console
4. Check Results in Inspector

## Common Mistakes to Avoid

| Mistake | Problem | Solution |
|---------|---------|----------|
| Direct references | Tight coupling | Use ScriptableVariables |
| Magic strings | Typos, hard to refactor | Use constants |
| Polling in Update | Wasted CPU | Use events |
| No effect cleanup | Memory leaks | Listen to OnDeath |
| One giant class | Unmaintainable | Single responsibility |
| GetComponent in Update | Performance | Cache in Start |
| No error checking | Crashes | Check for null |
| Forgotten unsubscribe | Memory leaks | Use OnDestroy |

## Code Review Checklist

- [ ] All modifier IDs use constants
- [ ] Effects cleaned up on death
- [ ] Events chosen over polling
- [ ] Components cached, not looked up each frame
- [ ] Single responsibility per class
- [ ] Null checks where needed
- [ ] Event subscriptions matched with unsubscriptions
- [ ] Descriptive variable names
- [ ] No circular dependencies
- [ ] Tests cover main paths

## See Also

- **[Architecture Overview](Architecture)** - System design
- **[Examples](Examples-and-Tutorials)** - Working code
- **[Troubleshooting](Troubleshooting)** - Common issues
