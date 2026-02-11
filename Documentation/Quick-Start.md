# Quick Start Cheat Sheet

One-page reference for getting started quickly.

## 30-Second Setup

```csharp
// Step 1: Add components to GameObject
gameObject.AddComponent<HealthComponent>().SetMaxHealth(100f);
gameObject.AddComponent<BuffDebuffSystem>();

// Step 2: Apply a buff
GetComponent<BuffDebuffSystem>().AddEffect(
    new ArmorEffect(\"armor\", -1f, 20f)
);

// Step 3: Take damage
GetComponent<HealthComponent>().TakeDamage(
    25f, \"attack\", DamageType.Physical
);

// Step 4: Listen to events
GetComponent<HealthComponent>().OnDeath += (source) =>
{
    Debug.Log(\"Dead!\");
};
```

Done! You have a fully working health and damage system.

---

## Common Tasks

### Take Damage

```csharp
health.TakeDamage(25f, \"enemy_attack\", DamageType.Physical);
```

### Heal

```csharp
health.Heal(30f, \"potion\");
```

### Apply Effect

```csharp
buffSystem.AddEffect(new ArmorEffect(\"steel_skin\", -1f, 40f));
```

### Remove Effect

```csharp
buffSystem.RemoveEffect(\"steel_skin\");
```

### Query Modifier

```csharp
float totalArmor = buffSystem.GetModifierSum(HealthModifiers.ARMOR);
float damageBonus = buffSystem.GetModifierMultiplier(CommonModifiers.DAMAGE_MULTIPLIER);
```

### Listen to Health Change

```csharp
health.OnHealthChanged += (old, new, max) => Debug.Log($\"HP: {new}\");
```

### Configure Movement with Buffs

```csharp
float speedMult = buffSystem.GetModifierMultiplier(
    CommonModifiers.MOVEMENT_SPEED_MULTIPLIER);
transform.position += direction * baseSpeed * speedMult * Time.deltaTime;
```

---

## Effects at a Glance

```csharp
// Armor/Resistance
new ArmorEffect(id, duration, armorAmount);
new MagicResistanceEffect(id, duration, resistAmount);

// Health
new MaxHealthBuff(id, duration, healthAmount);
new HealthRegenEffect(id, duration, regenPerSecond);

// Utility
new InvulnerabilityEffect(id, duration);
new LifestealEffect(id, duration, percentToHealing);
```

Duration Notes:
- Positive = seconds
- -1 = infinite

---

## UI Setup (5 Steps)

1. **Create ScriptableVariables**
   - PlayerCurrentHealth (ScriptableFloat)
   - PlayerMaxHealth (ScriptableFloat)
   - PlayerHealthPercent (ScriptableFloat)
   - PlayerIsDead (ScriptableBool)

2. **Assign to HealthComponent**
   - Drag variables into inspector

3. **Create UI Elements**
   - Text for health display
   - Image for health bar
   - Text for status

4. **Add HealthUIConnector**
   - Add component to Canvas
   - Assign UI elements
   - Assign variables

5. **Play!**
   - UI updates automatically

---

## Common Patterns

### Enemy Attack

```csharp
public void AttackEnemy(GameObject enemy)
{
    float damage = dealer.DealDamageToTarget(enemy, DamageType.Physical);
    if (enemy.GetComponent<HealthComponent>().IsDead)
        Destroy(enemy);
}
```

### Stun Effect

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
}

// Use it
buffSystem.AddEffect(new StunEffect(3f));
```

### Stacking Buff

```csharp
// Max 3 stacks
var buff = new DamageBoostEffect(\"boost\", 5f, 0.3f, maxStacks: 3);
buffSystem.AddEffect(buff);  // 1 stack
buffSystem.AddEffect(buff);  // 2 stacks
buffSystem.AddEffect(buff);  // 3 stacks (max)
```

### Check State Before Action

```csharp
if (enemy.health.IsAlive)
{
    enemy.TakeDamage(25f);
}
```

---

## Debugging Checklist

- [ ] Component exists: `GetComponent<HealthComponent>() != null`
- [ ] MaxHealth set: `health.EffectiveMaxHealth > 0`
- [ ] Health changing: `Debug.Log(health.CurrentHealth)`
- [ ] Modifier ID correct: Use constant, not string
- [ ] Event subscribed: `health.OnHealthChanged += Handler`
- [ ] Effect active: `effect.IsActive == true`
- [ ] BuffSystem updating: `buffSystem.enabled == true`

---

## Performance Quick Tips

```csharp
// ✅ Cache components
private HealthComponent health;
private void Start() { health = GetComponent<HealthComponent>(); }

// ❌ Avoid GetComponent in Update
private void Update() { GetComponent<HealthComponent>().TakeDamage(1f); }

// ✅ Use update intervals
buffSystem.UpdateInterval = 0.1f;

// ✅ Clean up
private void OnDestroy() 
{ 
    health.OnDeath -= OnDeath; 
}
```

---

## Error Quick Fix

| Error | Fix |
|-------|-----|
| \"BuffDebuffSystem not found\" | `AddComponent<BuffDebuffSystem>()` |
| \"NullReferenceException\" | Check `!= null` before using |
| \"Effect not working\" | Check correct modifier ID constant |
| \"UI not updating\" | Check ScriptableVariables assigned |
| \"Damage too high/low\" | Check armor and resistances |

---

## Copy-Paste Templates

### Custom Effect Template

```csharp
public class MyEffect : BuffDebuffEffect
{
    public MyEffect(string id, float duration, float value)
        : base(id, duration, maxStackCount: 1)
    {
    }

    public override IStatModifier[] GetModifiers()
    {
        return new[] { /* your modifiers */ };
    }

    public override void OnApply()
    {
        base.OnApply();
        // On apply logic
    }

    public override void OnRemove()
    {
        base.OnRemove();
        // On remove logic
    }
}
```

### Combat System Template

```csharp
public class CombatSystem : MonoBehaviour
{
    private HealthComponent health;
    private BuffDebuffSystem buffs;
    private DamageDealer dealer;

    private void Start()
    {
        health = GetComponent<HealthComponent>();
        buffs = GetComponent<BuffDebuffSystem>();
        dealer = GetComponent<DamageDealer>();

        health.SetMaxHealth(100f);
        dealer.SetBaseDamage(20f);

        health.OnDeath += OnDeath;
    }

    public void TakeDamage(float amount, string source)
    {
        health.TakeDamage(amount, source, DamageType.Physical);
    }

    public void Heal(float amount)
    {
        health.Heal(amount, \"heal\");
    }

    private void OnDeath(string source)
    {
        // Handle death
    }
}
```

### UI Update Template

```csharp
public class HealthDisplay : MonoBehaviour
{
    public ScriptableFloat currentHealth;
    public ScriptableFloat maxHealth;
    public Text healthText;

    private void Start()
    {
        currentHealth.OnRuntimeValueChanged += UpdateDisplay;
    }

    private void UpdateDisplay(float health)
    {
        healthText.text = $\"{health:F1}/{maxHealth.RuntimeValue:F1}\";
    }

    private void OnDestroy()
    {
        currentHealth.OnRuntimeValueChanged -= UpdateDisplay;
    }
}
```

---

## Next Steps

1. **Understand the Systems**
   - Read [Architecture Overview](Architecture.md)

2. **Try the Examples**
   - Check [Examples](Examples-and-Tutorials.md)

3. **Read Full Guides**
   - [Health & Damage](Health-Damage-System.md)
   - [Buff/Debuff](Buff-Debuff-System.md)
   - [ScriptableArchitecture](Scriptable-Architecture.md)

4. **Follow Best Practices**
   - See [Best Practices](Best-Practices.md)

5. **Get Unstuck**
   - Check [Troubleshooting](Troubleshooting.md)

---

## Key Takeaways

✓ Buff/Debuff system is foundation
✓ Health system uses buffs for modifiers
✓ ScriptableVariables decouple UI from logic
✓ Always use constants for modifier IDs
✓ Subscribe to events, don't poll
✓ Clean up event subscriptions
✓ Cache component references

---

## Useful Shortcuts

```csharp
// Quick damage
GetComponent<HealthComponent>().TakeDamage(10f, \"x\", DamageType.True);

// Quick buff
GetComponent<BuffDebuffSystem>().AddEffect(new ArmorEffect(\"x\", -1f, 10f));

// Quick modifier query
float armor = GetComponent<BuffDebuffSystem>()
    .GetModifierSum(HealthModifiers.ARMOR);

// Quick heal
GetComponent<HealthComponent>().Heal(25f, \"x\");

// Quick death check
if (GetComponent<HealthComponent>().IsDead) { }
```

See [Getting Started](Getting-Started.md) for more details!
