# Health and Damage System - Scriptable Architecture Integration

## Overview

The Health and Damage system integrates with the Scriptable Architecture system to expose health data through scriptable variables. This allows complete decoupling of UI from game logic - your UI can update based on health changes without needing direct references to the HealthComponent.

## Benefits

✅ **Decoupled UI** - UI updates without direct references to game systems
✅ **Reusable Assets** - Create UI systems that work with any entity
✅ **Designer-Friendly** - Non-programmers can set up UI in the inspector
✅ **Event-Driven** - UI responds automatically to health changes
✅ **Multiple Consumers** - Many UI elements can subscribe to the same health value
✅ **Live Editing** - Change UI behavior in play mode without code

## Architecture Flow

```
HealthComponent
    |
    +-- Updates ScriptableFloat: Current Health
    |       |
    |       +-- HealthUIConnector listens
    |       |       |
    |       |       +-- Updates health text "50/100"
    |       |       +-- Updates health bar fill
    |       |
    |       +-- Other systems can listen
    |
    +-- Updates ScriptableFloat: Max Health
    |
    +-- Updates ScriptableFloat: Health Percent (0-1)
    |
    +-- Updates ScriptableBool: Is Dead
```

## Setup Guide

### Step 1: Create Scriptable Variables

In your project, create scriptable variable assets for health:

1. Right-click in Assets folder → Create → Shizounu → ScriptableArchitecture
2. Create:
   - **PlayerCurrentHealth** (ScriptableFloat)
   - **PlayerMaxHealth** (ScriptableFloat)
   - **PlayerHealthPercent** (ScriptableFloat)
   - **PlayerIsDead** (ScriptableBool)

### Step 2: Assign to HealthComponent

In your player's HealthComponent in the inspector:

1. Assign **PlayerCurrentHealth** → Current Health Variable
2. Assign **PlayerMaxHealth** → Max Health Variable
3. Assign **PlayerHealthPercent** → Health Percent Variable
4. Assign **PlayerIsDead** → Is Dead Variable

```
HealthComponent Inspector:
┌─ Scriptable Variables ─────────────┐
│ Current Health Variable: ●Player    │
│ Max Health Variable: ●Player MH     │
│ Health Percent Variable: ●Player HP │
│ Is Dead Variable: ●Player Dead      │
└────────────────────────────────────┘
```

### Step 3: Create UI and HealthUIConnector

1. Create your Canvas/UI elements:
   - Text: "HealthText" (for "50/100")
   - Image: "HealthBar" (for bar fill)
   - Text: "HealthPercentText" (for "50%")
   - Text: "StatusText" (for "LOW HEALTH")

2. Add HealthUIConnector to your UI Canvas

3. Assign UI elements and scriptable variables in inspector:

```
HealthUIConnector Inspector:
┌─ Health Display ───────────────────┐
│ Health Text: ●HealthText           │
│ Health Bar Image: ●HealthBar       │
│ Health Percent Text: ●PercentText  │
│ Status Text: ●StatusText           │
└────────────────────────────────────┘
┌─ Scriptable Variables ─────────────┐
│ Current Health Variable: ●Player    │
│ Max Health Variable: ●Player MH     │
│ Health Percent Variable: ●Player HP │
│ Is Dead Variable: ●Player Dead      │
└────────────────────────────────────┘
```

### Step 4: Test

When the player takes damage:

1. HealthComponent.TakeDamage() is called
2. Internal health value updates
3. UpdateScriptableVariables() is called
4. ScriptableFloat.RuntimeValue changes
5. OnRuntimeValueChange event is invoked
6. HealthUIConnector.OnHealthChanged() responds
7. UI elements update automatically ✅

## UI Components

### HealthUIConnector

The main UI connector component with full features.

**Responsibilities:**
- Listen to all four scriptable variables
- Update text displays (current/max health, percentage)
- Update health bar with color coding
- Display status text (HEALTHY, LOW HEALTH, CRITICAL, DEAD)
- Color-code based on health percentage

**Assignments in Inspector:**
```
Health Text → Displays "50/100"
Health Bar Image → Fills 0-1
Health Percent Text → Displays "50%"
Status Text → Displays "HEALTHY" or "LOW HEALTH" etc
Health Slider → Alternative to Image, 0-1 range
```

**Features:**
- Automatic color transitions (green → yellow → red)
- Status text with color coding
- Optional slider component support
- Configurable color thresholds

**Example:**
```csharp
// Programmatic setup
healthUIConnector.SetScriptableVariables(
    currentHealth, maxHealth, healthPercent, isDead
);

healthUIConnector.SetUIElements(
    healthText, healthBar, percentText, statusText, slider
);

healthUIConnector.SetHealthColors(
    normal: Color.green,
    low: Color.yellow,
    critical: Color.red,
    dead: Color.gray
);
```

### SimpleHealthBar

Lightweight health bar that only displays a fill amount with gradient coloring.

**Assignment:**
```
Health Percent Variable → The float variable (0-1)
Health Bar Image → The fill image
Health Gradient → Color gradient from critical to healthy
```

**Use Case:** When you need just a simple bar without text or complex UI.

**Features:**
- Color gradient support
- Minimal overhead
- Clean, simple implementation

### DamageNumberDisplay

Shows floating damage numbers when the player takes damage.

**Assignment:**
```
Current Health Variable → Player's current health
Damage Number Prefab → Prefab with Text component
Canvas → Canvas to spawn in
Spawn Point → Where numbers appear
```

**Features:**
- Floating animation
- Fade out effect
- Customizable duration and speed
- Automatic damage calculation

## Complete Setup Example

### Project Structure

```
Assets/
  ScriptableArchitecture/
    UI/
      PlayerCurrentHealth.asset       ← ScriptableFloat
      PlayerMaxHealth.asset           ← ScriptableFloat
      PlayerHealthPercent.asset       ← ScriptableFloat
      PlayerIsDead.asset              ← ScriptableBool
  Scenes/
    GameScene.unity
      Canvas
        HealthBar (Image)
        HealthText (Text)
        PercentText (Text)
        StatusText (Text)
      Player
        HealthComponent
        BuffDebuffSystem
```

### Step-by-Step Setup

**1. Create Scriptable Variables**
```
Right-click → Create → Shizounu → ScriptableArchitecture → ScriptableFloat
Name: "PlayerCurrentHealth"
Initial Value: 100

Repeat for:
- PlayerMaxHealth (100)
- PlayerHealthPercent (1)
- PlayerIsDead (ScriptableBool, false)
```

**2. Configure HealthComponent**
```
Inspector on Player GameObject:
HealthComponent
├─ Max Health: 100
├─ Current Health Variable: PlayerCurrentHealth
├─ Max Health Variable: PlayerMaxHealth
├─ Health Percent Variable: PlayerHealthPercent
└─ Is Dead Variable: PlayerIsDead
```

**3. Create Canvas and UI**
```
Create Canvas with:
- Text "HealthText" (shows "100/100")
- Image "HealthBar" (shows fill)
- Text "PercentText" (shows "100%")
- Text "StatusText" (shows "HEALTHY")
```

**4. Add HealthUIConnector**
```
Inspector on Canvas:
HealthUIConnector
├─ Health Text: HealthText
├─ Health Bar Image: HealthBar
├─ Health Percent Text: PercentText
├─ Status Text: StatusText
├─ Current Health Variable: PlayerCurrentHealth
├─ Max Health Variable: PlayerMaxHealth
├─ Health Percent Variable: PlayerHealthPercent
└─ Is Dead Variable: PlayerIsDead
```

**5. Test in Play Mode**
```csharp
// In any script, take damage:
healthComponent.TakeDamage(25f, "attack", DamageType.Physical);

// UI updates automatically!
```

## Advanced Usage

### Multiple UI Screens

Create different UI systems that all listen to the same health variables:

```
PlayerCurrentHealth (shared)
  ├─ HUD health bar listens
  ├─ Character portrait damage effect listens
  ├─ Minimap health indicator listens
  └─ Game state system listens
```

All update independently without knowledge of each other.

### Runtime Variable Creation

Create health variables programmatically:

```csharp
// Create new variables
ScriptableFloat currentHealthVar = ScriptableObject.CreateInstance<ScriptableFloat>();
ScriptableFloat maxHealthVar = ScriptableObject.CreateInstance<ScriptableFloat>();

// Setup
healthComponent._currentHealthVariable = currentHealthVar;
healthComponent._maxHealthVariable = maxHealthVar;

// Update
healthComponent.UpdateScriptableVariables();
```

### Custom Health Displays

Create specialized UI components:

```csharp
public class CustomHealthDisplay : MonoBehaviour
{
    [SerializeField] private ScriptableFloat _healthPercent;
    
    private void OnEnable()
    {
        _healthPercent.OnRuntimeValueChange.RegisterListener(UpdateDisplay);
    }
    
    private void UpdateDisplay()
    {
        // Custom logic
        float percent = _healthPercent.RuntimeValue;
        
        // Show animated bar based on health tier
        if (percent > 0.75f) ShowTier(1);
        else if (percent > 0.5f) ShowTier(2);
        else if (percent > 0.25f) ShowTier(3);
        else ShowTier(4);
    }
}
```

### Networked Health (Multiplayer)

For multiplayer, sync scriptable variables across the network:

```csharp
[PunRPC]
void SyncHealth(float current, float max)
{
    _currentHealthVariable.RuntimeValue = current;
    _maxHealthVariable.RuntimeValue = max;
    // UI updates automatically on all clients!
}
```

## Performance Considerations

- **Event Subscription**: O(1) per listener
- **Variable Updates**: O(1) update time
- **UI Updates**: Only when values change (no polling)
- **Memory**: Small overhead for event listeners

**Best Practices:**
1. Reuse scriptable variables across multiple UI elements
2. Unsubscribe OnDisable (handled automatically in connector)
3. Cache variable references instead of finding them each frame
4. Use SimpleHealthBar for many entities (lower overhead)

## Troubleshooting

### UI Not Updating

**Problem:** Health changes but UI stays same

**Solutions:**
1. Verify scriptable variables are assigned in inspector
2. Check that HealthUIConnector is enabled
3. Verify OnRuntimeValueChange event exists
4. Check console for warnings

```csharp
// Debug: verify subscription
void OnEnable()
{
    Debug.Log($"Listening to {_currentHealthVariable.name}");
    _currentHealthVariable.OnRuntimeValueChange.RegisterListener(OnHealthChanged);
}
```

### Variables Not Updating

**Problem:** Health changes but scriptable variables stay same

**Solutions:**
1. Verify UpdateScriptableVariables() is called
2. Check that scriptable variables are not null
3. Verify HealthComponent has scribable variables assigned

```csharp
// In HealthComponent, after health change:
if (_currentHealthVariable != null)
    Debug.Log($"Updating to {_currentHealth}");
    _currentHealthVariable.RuntimeValue = _currentHealth;
```

### Multiple Entities

For enemies, create separate scriptable variables per entity:

```csharp
// Each enemy gets its own scriptable variables
ScriptableFloat enemyHealth = ScriptableObject.CreateInstance<ScriptableFloat>();
healthComponent._currentHealthVariable = enemyHealth;

// Or use ScriptableArchitecture pooling if available
```

## Integration Checklist

- [ ] Created 4 scriptable variable assets
- [ ] Assigned them to HealthComponent
- [ ] Created UI Canvas and elements
- [ ] Added HealthUIConnector to Canvas
- [ ] Assigned UI elements to connector
- [ ] Assigned scriptable variables to connector
- [ ] Tested damage behavior
- [ ] UI updates correctly
- [ ] Tested healing behavior
- [ ] Status text updates correctly
- [ ] Color changes at thresholds

## Related Systems

- **HealthComponent**: Tracks health, updates variables
- **BuffDebuffSystem**: Modifies health calculations
- **HealthUIConnector**: Updates UI from variables
- **ScriptableArchitecture**: Provides variable framework

## See Also

- [README.md](README.md) - Overall system documentation
- [QUICK_REFERENCE.md](QUICK_REFERENCE.md) - API reference
- [GETTING_STARTED.md](GETTING_STARTED.md) - Basic setup guide
- [HealthAndDamageExample.cs](Example/HealthAndDamageExample.cs) - Code examples
