# Health and Damage System - Getting Started Guide

## Step 1: Basic Setup

Create a new `GameObject` in your scene for an entity (player, enemy, etc.):

```csharp
using UnityEngine;
using Shizounu.Library.BuffDebuff;
using Shizounu.Library.HealthAndDamage;

public class MyCharacter : MonoBehaviour
{
    private HealthComponent health;
    private BuffDebuffSystem buffs;
    private DamageDealer dealer;

    private void Start()
    {
        // Get components (or AddComponent if not already present)
        health = GetComponent<HealthComponent>();
        buffs = GetComponent<BuffDebuffSystem>();
        dealer = GetComponent<DamageDealer>();

        if (health == null)
            health = gameObject.AddComponent<HealthComponent>();
        if (buffs == null)
            buffs = gameObject.AddComponent<BuffDebuffSystem>();
        if (dealer == null)
            dealer = gameObject.AddComponent<DamageDealer>();

        // Configure health
        health.SetMaxHealth(100f);
        dealer.SetBaseDamage(20f);

        // Subscribe to events
        health.OnDeath += OnDeath;
        health.OnHealthChanged += OnHealthChanged;
    }

    private void OnDeath(string source)
    {
        Debug.Log($"Character died from: {source}");
        // Handle death logic here
    }

    private void OnHealthChanged(float oldHealth, float newHealth, float maxHealth)
    {
        Debug.Log($"Health: {newHealth:F1}/{maxHealth:F1}");
        // Update UI here
    }
}
```

## Step 2: Applying Effects

Add buffs and debuffs to your entity using the predefined effects:

```csharp
// In your MyCharacter class - apply initial effects
private void ApplyStartingEffects()
{
    // Give the character armor
    var armor = new ArmorEffect("starting_armor", -1f, 20f); // Infinite duration, 20 armor
    buffs.AddEffect(armor);

    // Add passive health regeneration
    var regen = new HealthRegenEffect("passive_regen", -1f, 5f); // 5 HP per second
    buffs.AddEffect(regen);
}
```

## Step 3: Handling Damage

Create a method to take damage from attacks:

```csharp
public void TakeDamageFromAttack(float baseDamage, GameObject attacker)
{
    // Apply damage (considers armor, resistances, buffs)
    float actualDamage = health.TakeDamage(baseDamage, attacker.name, DamageType.Physical);

    Debug.Log($"Took {actualDamage} damage (base was {baseDamage})");

    // Check if dead
    if (health.IsDead)
    {
        Respawn();
    }
}
```

## Step 4: Dealing Damage

Create an attack system that damages other entities:

```csharp
public void AttackTarget(GameObject target)
{
    // Get target's health system
    HealthComponent targetHealth = target.GetComponent<HealthComponent>();
    if (targetHealth == null)
    {
        Debug.LogWarning("Target has no HealthComponent!");
        return;
    }

    // Deal damage (applies our damage buffs and their resistances)
    float effectiveDamage = dealer.GetEffectiveDamage();
    float actualDamage = dealer.DealDamageToTarget(target, DamageType.Physical);

    Debug.Log($"Attacked {target.name} for {actualDamage} damage (base: {effectiveDamage})");

    // Check for special effects
    if (dealer.HasDamageBonus())
    {
        Debug.Log("Critical hit!");
    }
}
```

## Step 5: Healing System

Add healing methods:

```csharp
public void HealSelf(float amount)
{
    float actualHealing = health.Heal(amount, "potion");
    Debug.Log($"Healed for {actualHealing} HP");
}

public void HealOther(GameObject target, float amount)
{
    HealthComponent targetHealth = target.GetComponent<HealthComponent>();
    if (targetHealth != null)
    {
        float actualHealing = targetHealth.Heal(amount, "heal_spell");
        Debug.Log($"Healed {target.name} for {actualHealing} HP");
    }
}
```

## Step 6: Applying Temporary Buffs

Create a method to apply temporary buffs:

```csharp
public void ApplyStrengthBuff(float duration = 10f)
{
    // Add damage multiplier through a custom effect
    var strengthBuff = new BuffDebuffEffect("strength_buff", duration, maxStackCount: 1);
    // Note: For custom modifiers, you'd create a custom effect class like in HealthEffects.cs
    buffs.AddEffect(strengthBuff);
}

public void ApplyVulnerability(float duration = 5f)
{
    // Vulnerability debuff - takes 50% more damage
    var vulnerability = new VulnerabilityEffect("curse", duration, damageIncrease: 0.5f);
    buffs.AddEffect(vulnerability);
}

public void ApplyInvulnerability(float duration = 3f)
{
    // Complete protection from damage
    var shield = new InvulnerabilityEffect("god_mode", duration);
    buffs.AddEffect(shield);
}
```

## Step 7: Status Checks

Create helper methods to check status:

```csharp
public bool IsHealthLow() => health.HealthPercent < 0.25f;
public bool IsDead() => health.IsDead;
public bool IsAlive() => health.IsAlive;
public float GetHealthPercent() => health.HealthPercent * 100f;
public float GetCurrentHealth() => health.CurrentHealth;
public float GetMaxHealth() => health.EffectiveMaxHealth;

public void PrintStatus()
{
    Debug.Log($"=== {gameObject.name} ===");
    Debug.Log($"Health: {health.CurrentHealth:F1}/{health.EffectiveMaxHealth:F1}");
    Debug.Log($"Status: {(health.IsDead ? "DEAD" : "ALIVE")}");
    Debug.Log($"Armor: {buffs.GetModifierSum(HealthModifiers.ARMOR):F1}");
    Debug.Log($"Active Effects: {buffs.GetActiveEffectCount()}");
}
```

## Step 8: Complete Example

Here's a complete character class combining everything:

```csharp
using UnityEngine;
using Shizounu.Library.BuffDebuff;
using Shizounu.Library.HealthAndDamage;

public class CompleteCharacterExample : MonoBehaviour
{
    [SerializeField] private float baseMaxHealth = 100f;
    [SerializeField] private float baseDamage = 20f;

    private HealthComponent health;
    private BuffDebuffSystem buffs;
    private DamageDealer dealer;

    private void Start()
    {
        // Setup components
        health = GetComponent<HealthComponent>();
        buffs = GetComponent<BuffDebuffSystem>();
        dealer = GetComponent<DamageDealer>();

        if (health == null) health = gameObject.AddComponent<HealthComponent>();
        if (buffs == null) buffs = gameObject.AddComponent<BuffDebuffSystem>();
        if (dealer == null) dealer = gameObject.AddComponent<DamageDealer>();

        // Configure
        health.SetMaxHealth(baseMaxHealth);
        dealer.SetBaseDamage(baseDamage);

        // Subscribe to events
        health.OnDeath += OnDeath;
        health.OnFullyHealed += OnFullyHealed;
        buffs.OnEffectAdded += OnEffectAdded;
        buffs.OnEffectRemoved += OnEffectRemoved;

        // Apply starting effects
        ApplyStartingEffects();
    }

    private void Update()
    {
        // Example input handling
        if (Input.GetKeyDown(KeyCode.Space))
            TakeDamageFromAttack(25f, gameObject);

        if (Input.GetKeyDown(KeyCode.H))
            HealSelf(30f);

        if (Input.GetKeyDown(KeyCode.A))
            ApplyArmorBuff(10f);

        if (Input.GetKeyDown(KeyCode.V))
            ApplyVulnerability(5f);
    }

    private void ApplyStartingEffects()
    {
        var armor = new ArmorEffect("base_armor", -1f, 15f);
        var regen = new HealthRegenEffect("base_regen", -1f, 3f);
        buffs.AddEffect(armor);
        buffs.AddEffect(regen);
    }

    public void TakeDamageFromAttack(float baseDamage, GameObject attacker)
    {
        float actualDamage = health.TakeDamage(baseDamage, attacker.name, DamageType.Physical);
        Debug.Log($"Took {actualDamage:F1} damage from {attacker.name}");
    }

    public void AttackTarget(GameObject target)
    {
        HealthComponent targetHealth = target.GetComponent<HealthComponent>();
        if (targetHealth != null)
        {
            float damage = dealer.GetEffectiveDamage();
            float actual = dealer.DealDamageToTarget(target, DamageType.Physical);
            Debug.Log($"Attacked {target.name} for {actual:F1} damage");
        }
    }

    public void HealSelf(float amount)
    {
        float actualHealing = health.Heal(amount, "potion");
        Debug.Log($"Healed for {actualHealing:F1} HP");
    }

    public void ApplyArmorBuff(float duration)
    {
        var armor = new ArmorEffect("temp_armor", duration, 30f);
        buffs.AddEffect(armor);
        Debug.Log("Armor buff applied!");
    }

    public void ApplyVulnerability(float duration)
    {
        var vulnerability = new VulnerabilityEffect("curse", duration, 0.5f);
        buffs.AddEffect(vulnerability);
        Debug.Log("Vulnerability curse applied!");
    }

    private void OnDeath(string source)
    {
        Debug.Log($"Died to: {source}");
        Destroy(gameObject);
    }

    private void OnFullyHealed()
    {
        Debug.Log("Fully healed!");
    }

    private void OnEffectAdded(IBuffDebuffEffect effect)
    {
        Debug.Log($"Effect added: {effect.EffectID}");
    }

    private void OnEffectRemoved(IBuffDebuffEffect effect)
    {
        Debug.Log($"Effect removed: {effect.EffectID}");
    }
}
```

## Step 9: Testing in the Scene

1. Create a new GameObject in your scene
2. Add the components:
   - HealthComponent
   - BuffDebuffSystem
   - DamageDealer
   - CompleteCharacterExample (or your custom character script)
3. Run the game and test with keyboard:
   - **Space** = Take damage
   - **H** = Heal
   - **A** = Apply armor buff
   - **V** = Apply vulnerability

## Step 10: Create UI Integration

Here's how to connect it to a health bar:

```csharp
public class HealthBar : MonoBehaviour
{
    [SerializeField] private Image fillImage;
    private HealthComponent health;

    private void Start()
    {
        health = GetComponentInParent<HealthComponent>();
        health.OnHealthChanged += UpdateHealthBar;
        UpdateHealthBar(health.CurrentHealth, health.CurrentHealth, health.EffectiveMaxHealth);
    }

    private void UpdateHealthBar(float oldHealth, float newHealth, float maxHealth)
    {
        float percent = newHealth / maxHealth;
        fillImage.fillAmount = percent;

        // Change color based on health
        if (percent > 0.5f)
            fillImage.color = Color.green;
        else if (percent > 0.25f)
            fillImage.color = Color.yellow;
        else
            fillImage.color = Color.red;
    }
}
```

## Common Issues and Solutions

### Issue: Effects not applying damage reduction
**Solution**: Make sure you're using `TakeDamage()` not `TakeDamageRaw()`. Also verify the BuffDebuffSystem component is present.

### Issue: Damage appears to do nothing
**Solution**: Check that entity has a HealthComponent and that health is above 0 (not dead).

### Issue: Events not firing
**Solution**: Make sure you subscribe to events BEFORE they happen. Subscribe in Start() or OnEnable().

### Issue: Modifiers not stacking
**Solution**: Check the `maxStackCount` parameter when creating effects. Default is 1 (no stacking).

### Issue: Max health buffs not increasing effective health
**Solution**: Use `health.EffectiveMaxHealth`, not `BaseMaxHealth`. Current health is automatically clamped to effective max.

## Next Steps

1. See [README.md](README.md) for comprehensive documentation
2. See [QUICK_REFERENCE.md](QUICK_REFERENCE.md) for API reference
3. Check [HealthAndDamageExample.cs](Example/HealthAndDamageExample.cs) for more examples
4. Create custom effects by extending `BuffDebuffEffect` for game-specific mechanics
5. Integrate with your game's UI and gameplay systems

## Performance Tips

1. Cache component references instead of calling GetComponent repeatedly
2. Use event subscriptions instead of polling health status in Update()
3. For many entities, consider object pooling effects
4. Health calculations are O(1), modifier queries are O(n) where n = effects (usually small)

Good luck with your game! 🎮
