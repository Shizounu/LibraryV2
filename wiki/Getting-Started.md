# Getting Started with Shizounu Library V2

Welcome! This guide will get you up and running with the Shizounu Library in just a few minutes.

## Prerequisites

- **Unity 2021.3 or later**
- **C# 9.0 or later**
- Basic understanding of MonoBehaviour and Components

## Installation

1. **Clone or download** the LibraryV2 project into your Unity projects folder
2. **Open the project** in Unity
3. The library is pre-configured and ready to use!

### What's Included

```
Assets/
└── Library/
    ├── BuffDebuff/           # Buff & debuff system
    ├── HealthAndDamage/      # Health & damage management
    ├── ScriptableArchitecture/ # Variable & event architecture
    ├── Character Controllers/ # Player control systems
    ├── Dialogue/             # Dialogue system
    ├── Game AI/              # AI behaviors
    ├── Tween/                # Animation tweening
    ├── Update System/        # Frame coordination
    └── Utility/              # Helper functions
```

## Your First Buff/Debuff Effect

The most fundamental system is the **Buff/Debuff System**. Here's a minimal example:

### Step 1: Add the System to a GameObject

```csharp
using UnityEngine;
using Shizounu.Library.BuffDebuff;

public class MyGameObject : MonoBehaviour
{
    private void Start()
    {
        // Add the component
        var buffSystem = gameObject.AddComponent<BuffDebuffSystem>();
        
        // That's it! You can now apply effects
    }
}
```

### Step 2: Apply Your First Effect

```csharp
// Create a custom effect
public class SpeedBoostEffect : BuffDebuffEffect
{
    public SpeedBoostEffect() 
        : base("speed_boost", duration: 5f) // 5 second duration
    {
    }

    public override IStatModifier[] GetModifiers()
    {
        return new[]
        {
            new SimpleStatModifier(CommonModifiers.MOVEMENT_SPEED_MULTIPLIER, 0.5f) // 50% faster
        };
    }
}

// Apply it to the buff system
var boostEffect = new SpeedBoostEffect();
buffSystem.AddEffect(boostEffect);
```

### Step 3: Use the Modifier in Your Movement Code

```csharp
public class PlayerMovement : MonoBehaviour
{
    public float baseSpeed = 5f;
    private BuffDebuffSystem buffSystem;

    private void Start()
    {
        buffSystem = GetComponent<BuffDebuffSystem>();
    }

    private void Update()
    {
        // Get the speed multiplier from active buffs
        float speedMultiplier = buffSystem.GetModifierMultiplier(
            CommonModifiers.MOVEMENT_SPEED_MULTIPLIER);
        
        float effectiveSpeed = baseSpeed * speedMultiplier;
        
        // Use effectiveSpeed for movement
        Vector3 input = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        transform.position += input * effectiveSpeed * Time.deltaTime;
    }
}
```

## Your First Health System

The **Health & Damage System** works seamlessly with the Buff/Debuff system:

### Step 1: Add Components

```csharp
// Add both components to your game object
gameObject.AddComponent<HealthComponent>();
gameObject.AddComponent<BuffDebuffSystem>();

// Optional: for dealing damage to others
gameObject.AddComponent<DamageDealer>();
```

### Step 2: Configure Health

```csharp
public class CharacterSetup : MonoBehaviour
{
    private void Start()
    {
        var health = GetComponent<HealthComponent>();
        health.SetMaxHealth(100f);
        
        // Subscribe to events
        health.OnDeath += OnCharacterDeath;
        health.OnHealthChanged += OnHealthChanged;
    }

    private void OnCharacterDeath(string source)
    {
        Debug.Log($\"Character died from: {source}\");
    }

    private void OnHealthChanged(float oldHealth, float newHealth, float maxHealth)
    {
        Debug.Log($\"Health: {newHealth}/{maxHealth}\");
    }
}
```

### Step 3: Apply Damage & Healing

```csharp
var health = GetComponent<HealthComponent>();

// Take damage (respects armor/resistance from buffs)
health.TakeDamage(25f, \"attack\", DamageType.Physical);

// Heal
health.Heal(15f, \"potion\");

// Apply a buff that increases health
var armor = new ArmorEffect(\"steel_skin\", duration: -1f, armorAmount: 20f);
GetComponent<BuffDebuffSystem>().AddEffect(armor);
```

## Connecting to UI

The **Scriptable Architecture** system makes it easy to connect gameplay to UI without tight coupling:

### Step 1: Create Scriptable Variables

In your Project folder:

1. Right-click → **Create → Shizounu → ScriptableArchitecture → ScriptableFloat**
2. Name it **\"PlayerCurrentHealth\"**
3. Repeat for:
   - \"PlayerMaxHealth\" (ScriptableFloat)
   - \"PlayerHealthPercent\" (ScriptableFloat)
   - \"PlayerIsDead\" (ScriptableBool)

### Step 2: Connect Health Component

In your Player's **HealthComponent**:

1. Drag **PlayerCurrentHealth** → \"Current Health Variable\"
2. Drag **PlayerMaxHealth** → \"Max Health Variable\"
3. Drag **PlayerHealthPercent** → \"Health Percent Variable\"
4. Drag **PlayerIsDead** → \"Is Dead Variable\"

### Step 3: Create UI Elements

Create a Canvas with:
- Text: \"HealthText\" (shows \"50/100\")
- Image: \"HealthBar\" (fill amount 0-1)
- Text: \"StatusText\" (shows \"HEALTHY\", \"LOW HEALTH\", etc)

### Step 4: Add HealthUIConnector

Add the **HealthUIConnector** component to your Canvas, then:

1. Drag **HealthText** → \"Health Text\"
2. Drag **HealthBar** → \"Health Bar Image\"
3. Drag **StatusText** → \"Status Text\"
4. Assign the **ScriptableFloa/Bool** variables

**That's it!** Your UI now updates automatically when health changes.

## Common Patterns

### Pattern: Applying Multiple Buffs

```csharp
var buffSystem = GetComponent<BuffDebuffSystem>();

// Armor buff
buffSystem.AddEffect(new ArmorEffect(\"armor\", -1f, 30f));

// Health regen
buffSystem.AddEffect(new HealthRegenEffect(\"regen\", -1f, 5f));

// Damage boost
buffSystem.AddEffect(new DamageBoostEffect(\"boost\", 10f, 0.3f));
```

### Pattern: Check and Remove Buffs

```csharp
// Check if has effect
if (buffSystem.GetEffect(\"armor\") != null)
{
    Debug.Log(\"Has armor buff\");
    
    // Remove it
    buffSystem.RemoveEffect(\"armor\");
}
```

### Pattern: Query Current Modifiers

```csharp
// Get armor value
float totalArmor = buffSystem.GetModifierSum(HealthModifiers.ARMOR);

// Get damage multiplier
float damageBonus = buffSystem.GetModifierMultiplier(HealthModifiers.DAMAGE_MULTIPLIER);

// Check for immunity
bool isInvulnerable = buffSystem.HasModifier(HealthModifiers.INVULNERABLE);
```

## Next Steps

- **[Buff/Debuff System](Buff-Debuff-System)** - Learn the full system
- **[Health & Damage](Health-Damage-System)** - Explore health mechanics
- **[Examples](Examples-and-Tutorials)** - See more working code
- **[API Reference](API-Reference)** - Look up specific classes

## Troubleshooting

### \"BuffDebuffSystem not found\"
Make sure you have:
1. Added the component: `gameObject.AddComponent<BuffDebuffSystem>()`
2. The correct using statement: `using Shizounu.Library.BuffDebuff;`

### \"Effect not working\"
Check:
1. Is the component active? `buffSystem.isActiveAndEnabled == true`
2. Is the effect still active? `effect.IsActive == true`
3. Are you querying the right modifier ID?

### \"UI not updating\"
Verify:
1. ScriptableVariables are assigned in the Inspector
2. HealthUIConnector is on an active GameObject
3. UI elements are assigned to the connector
4. Health is actually changing (check Debug.Log)

## Getting Help

- **Need more details?** Check [Buff-Debuff-System](Buff-Debuff-System) or [Health-Damage-System](Health-Damage-System)
- **Want examples?** Browse [Examples-and-Tutorials](Examples-and-Tutorials)
- **Looking for an API?** See [API-Reference](API-Reference)
- **Still stuck?** Check [Troubleshooting](Troubleshooting)
