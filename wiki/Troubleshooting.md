# Troubleshooting Guide

Common issues and their solutions.

## Buff/Debuff System Issues

### Effect Not Applying

**Problem:** Effect added but not taking effect

**Checklist:**
- [ ] BuffDebuffSystem component exists: `GetComponent<BuffDebuffSystem>() != null`
- [ ] Component is enabled: `buffSystem.enabled == true`
- [ ] Adding effect: `buffSystem.AddEffect(effect)` called
- [ ] Duration correct: Not already expired

**Solution:**
```csharp
var buffs = GetComponent<BuffDebuffSystem>();
if (buffs == null)
{
    buffs = gameObject.AddComponent<BuffDebuffSystem>();
    Debug.Log(\"Added BuffDebuffSystem\");
}

var effect = new MyEffect(\"test\", duration: 5f);
buffs.AddEffect(effect);

Debug.Log($\"Effect added: {effect.IsActive}\");
```

### Modifiers Not Working

**Problem:** Modifiers query returns 0 or wrong value

**Checklist:**
- [ ] Correct modifier ID used
- [ ] Effect provides modifier: `GetModifiers()` returns it
- [ ] Getter matches modifier type (Sum vs Multiplier)
- [ ] Effect still active: `effect.IsActive == true`

**Solution:**
```csharp
// ❌ Wrong ID
buffSystem.GetModifierSum(\"armor\");

// ✅ Correct constant
float armor = buffSystem.GetModifierSum(HealthModifiers.ARMOR);

// ❌ Wrong getter type (multiplicative as sum)
float mult = buffSystem.GetModifierSum(CommonModifiers.DAMAGE_MULTIPLIER);

// ✅ Correct getter
float mult = buffSystem.GetModifierMultiplier(CommonModifiers.DAMAGE_MULTIPLIER);
```

### Stack Limit Not Working

**Problem:** Effect keeps stacking beyond max

**Checklist:**
- [ ] maxStackCount set: `maxStackCount > 0`
- [ ] Same effect ID used: Same string ID
- [ ] Not exceeding limit

**Solution:**
```csharp
// Set max stacks
public class DamageBoostEffect : BuffDebuffEffect
{
    public DamageBoostEffect()
        : base(\"damage_boost\", 5f, maxStackCount: 3) // Limit to 3 stacks
    {
    }
}

// Check current stacks
var effect = buffSystem.GetEffect(\"damage_boost\");
if (effect != null && effect.StackCount < 3)
{
    buffSystem.AddEffect(effect); // Add another stack
}
```

###Effect Not Removing

**Problem:** Effect stays active after duration expires

**Checklist:**
- [ ] Duration > 0: Infinite (-1) not intended
- [ ] Time passing: Game not paused
- [ ] BuffDebuffSystem active and updating

**Solution:**
```csharp
// Infinite duration
var buff = new ArmorEffect(\"armor\", -1f, 10f);
buffSystem.AddEffect(buff);

// Finite duration (5 seconds)
var buff2 = new DamageBoostEffect(\"boost\", 5f, 0.5f);
buffSystem.AddEffect(buff2);

// Manual removal
buffSystem.RemoveEffect(\"boost\");
```

---

## Health System Issues

### Health Not Changing

**Problem:** TakeDamage/Heal called but health unchanged

**Checklist:**
- [ ] HealthComponent exists
- [ ] MaxHealth set: `SetMaxHealth()` called
- [ ] Not in invulnerable state
- [ ] Not at 0 health already

**Solution:**
```csharp
var health = GetComponent<HealthComponent>();

// Setup
if (health == null)
    health = gameObject.AddComponent<HealthComponent>();

health.SetMaxHealth(100f);

// Debug
Debug.Log($\"Current: {health.CurrentHealth}\");
Debug.Log($\"Max: {health.EffectiveMaxHealth}\");
Debug.Log($\"Dead: {health.IsDead}\");
Debug.Log($\"Invulnerable: {health.BuffSystem.HasModifier(HealthModifiers.INVULNERABLE)}\");

// Apply damage
float actual = health.TakeDamage(25f, \"test\", DamageType.Physical);
Debug.Log($\"Actual damage: {actual}\");
```

### Damage Too High/Low

**Problem:** Damage calculation wrong

**Checklist:**
- [ ] Damage type correct (Physical/Magic/True)
- [ ] Armor/resistance level
- [ ] Damage taken modifiers applied
- [ ] Attacker buffs applied

**Solution:**
```csharp
// Check all modifiers
float armor = targetHealth.BuffSystem.GetModifierSum(HealthModifiers.ARMOR);
float damageTakenMult = targetHealth.BuffSystem
    .GetModifierMultiplier(HealthModifiers.DAMAGE_TAKEN_MULTIPLIER);

Debug.Log($\"Target armor: {armor}\");
Debug.Log($\"Target damage taken mult: {damageTakenMult}\");

// Actual calculation
float finalDamage = (baseDamage - armor) * damageTakenMult;
Debug.Log($\"Final damage: {finalDamage}\");
```

### Death Not Triggering

**Problem:** Character dies but OnDeath event doesn't fire

**Checklist:**
- [ ] Subscribed to event: `health.OnDeath += Handler`
- [ ] OnDeath called before health reaches 0
- [ ] Handler not removed

**Solution:**
```csharp
// Subscribe properly
health.OnDeath += (source) => 
{
    Debug.Log($\"Death from: {source}\");
};

// Always unsubscribe in OnDestroy
private void OnDestroy()
{
    health.OnDeath -= OnDeath;
}

// Alternative pattern
public void TakeDamageAndCheckDeath(float damage)
{
    float actualDamage = health.TakeDamage(damage, \"attack\", DamageType.Physical);
    
    if (health.IsDead)
    {
        // Safe to check after
        Debug.Log(\"Character is dead\");
    }
}
```

---

## Health & UI Issues

### UI Not Updating

**Problem:** Health changes but UI stays same

**Checklist:**
- [ ] ScriptableVariables assigned in HealthComponent
- [ ] HealthUIConnector component exists
- [ ] ScriptableVariables assigned in connector
- [ ] UI elements assigned
- [ ] Health actually changing

**Debug Steps:**
```csharp
// 1. Check variable updates
Debug.Log(currentHealthVariable.RuntimeValue);

// 2. Check event fires
currentHealthVariable.OnRuntimeValueChanged += (val) =>
    Debug.Log($\"Health changed to: {val}\");

// 3. Check connector gets event
// Add debug log to HealthUIConnector.OnHealthChanged()

// 4. Check UI element exists
Debug.Log($\"Health text: {healthText != null}\");
Debug.Log($\"Health bar: {healthBar != null}\");
```

### Status Text Wrong

**Problem:** Shows wrong status (e.g., CRITICAL when healthy)

**Solution:**
```csharp
// Check thresholds
var connector = GetComponent<HealthUIConnector>();
Debug.Log($\"Low threshold: {connector.lowHealthThreshold}\");
Debug.Log($\"Critical threshold: {connector.criticalHealthThreshold}\");

// Check actual health
float percent = health.HealthPercent;
Debug.Log($\"Health percent: {percent}\");

// Expected status
if (percent >= connector.lowHealthThreshold)
    Debug.Log(\"Should be HEALTHY\");
else if (percent >= connector.criticalHealthThreshold)
    Debug.Log(\"Should be LOW HEALTH\");
else
    Debug.Log(\"Should be CRITICAL\");
```

### Colors Not Updating

**Problem:** Status text colors not changing

**Checklist:**
- [ ] Status Text assigned
- [ ] Colors configured in inspector
- [ ] Health actually crossing thresholds
- [ ] Text component exists

**Solution:**
```csharp
// Verify colors set
var connector = GetComponent<HealthUIConnector>();
Debug.Log($\"Healthy color: {connector.healthyColor}\");
Debug.Log($\"Low color: {connector.lowHealthColor}\");

// Force update
connector.SetHealthColors(
    normal: Color.green,
    low: Color.yellow,
    critical: Color.red,
    dead: Color.gray
);
```

---

## Scriptable Architecture Issues

### Variable Not Saving Changes

**Problem:** RuntimeValue changed but reverts

**Checklist:**
- [ ] Setting RuntimeValue, not InitialValue
- [ ] Not resetting value elsewhere
- [ ] Game not reloading scene

**Solution:**
```csharp
// ✅ Correct
healthVariable.RuntimeValue = 50f;

// ❌ Wrong
healthVariable.InitialValue = 50f; // Doesn't persist
```

### Event Not Firing

**Problem:** OnRuntimeValueChanged not called

**Checklist:**
- [ ] Subscribed to event
- [ ] Setting value, not just reading
- [ ] New value different from old
- [ ] Not in Awake (might not be initialized)

**Solution:**
```csharp
private void Start() // Not Awake!
{
    // Subscribe in Start, not Awake
    variable.OnRuntimeValueChanged += OnValueChanged;
}

private void OnValueChanged(float newValue)
{
    Debug.Log($\"Value: {newValue}\");
}

// Force update
variable.RuntimeValue = variable.RuntimeValue; // Triggers event
```

### Multiple Listeners Not Working

**Problem:** Only first listener responds

**Solution:**
```csharp
// Multiple listeners work fine
health.OnHealthChanged += UpdateUI;
health.OnHealthChanged += UpdateStats;
health.OnHealthChanged += LogHealth;

// All three will be called!
health.TakeDamage(25f);
```

---

## Performance Issues

### Frame Rate Drops

**Problem:** FPS drops with effects

**Solutions:**

```csharp
// 1. Use update intervals instead of every frame
buffSystem.UpdateInterval = 0.1f;

// 2. Reduce active effects
buffSystem.ClearAllEffects();

// 3. Cache component references
private HealthComponent health;

private void Start()
{
    health = GetComponent<HealthComponent>(); // Cache!
}

private void Update()
{
    health.TakeDamage(1f); // Use cache
}
```

### Memory Leaks

**Problem:** Increasing memory usage

**Common Causes & Solutions:**

```csharp
// 1. Unsubscribe from events
private void OnDestroy()
{
    buffSystem.OnEffectAdded -= HandleEffect;
    health.OnDeath -= HandleDeath;
}

// 2. Clear effects on death
private void OnDeath(string source)
{
    buffSystem.ClearAllEffects();
    Destroy(gameObject);
}

// 3. Remove handlers in proper order
var effect = buffSystem.GetEffect(\"old_effect\");
if (effect != null)
    buffSystem.RemoveEffect(\"old_effect\");
```

---

## Common Error Messages

### \"BuffDebuffSystem not found\"

```
Solution: Add component with AddComponent<BuffDebuffSystem>()
```

### \"Modifier ID not recognized\"

```csharp
// Use constants
buffSystem.GetModifierSum(CommonModifiers.ARMOR_FLAT);
```

### \"Effect is null\"

```csharp
var effect = buffSystem.GetEffect(\"effect_id\");
if (effect != null)
{
    // Safe to use
}
```

### \"Object reference not set to an instance\"

```csharp
// Check for null before using
if (health != null)
{
    health.TakeDamage(25f);
}
```

---

## Getting More Help

1. **Check Examples:** `Assets/Library/Examples/`
2. **Read Guides:** Re-read relevant wiki page
3. **Debug Output:** Add Debug.Log everywhere
4. **Inspect:** Check values in Inspector during play
5. **Check Code:** Review your implementation against examples

---

## Debugging Tips

### Add Debug Logs Strategically

```csharp
// Before action
Debug.Log($\"Taking {damage} damage\");

// After action  
Debug.Log($\"Health now: {health.CurrentHealth}\");

// Event log
public override void OnApply()
{
    base.OnApply();
    Debug.Log($\"{EffectID} applied!\");
}
```

### Watch in Inspector

During play mode, you can:
- Select GameObject
- See component values
- Change values in real-time
- See effect list in BuffDebuffSystem
- See active modifiers

### Unity Profiler

Check CPU/Memory usage:
- Window → Analysis → Profiler
- Check Update System performance
- Find bottlenecks

---

## Still Stuck?

1. Verify Setup: Follow Getting Started exactly
2. Compare to Examples: Check example scenes
3. Review Relevant Guide: Read the system docs
4. Check Best Practices: Follow patterns
5. Add Debug Output: Log everything
6. Check Inspector: Verify assignments

See Also: [Getting Started](Getting-Started), [Examples](Examples-and-Tutorials), [Architecture](Architecture)
