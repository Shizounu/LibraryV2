# Health & Damage UI Integration

This guide shows how to connect your **Health and Damage System** to UI using **Scriptable Architecture**, enabling complete decoupling of gameplay from UI.

## Why Scriptable Architecture?

Traditional approach (problems):
```csharp
// Tight coupling - UI needs direct reference to game logic
public class HealthUI : MonoBehaviour
{
    private HealthComponent gameHealth;
    
    private void Update()
    {
        text.text = gameHealth.CurrentHealth.ToString(); // Must access every frame!
    }
}
```

Problems:
- UI needs direct reference to gameplay
- Multiple UIs need same reference
- Can't test UI without gameplay
- Gameplay can't exist without UI
- Changes to one require changes to the other

**Scriptable Architecture Solution:**
```
Game Logic → Scriptable Variable → UI
```

Benefits:
- UI doesn't know about gameplay
- Gameplay doesn't know about UI
- One system, multiple UIs
- Network-friendly (update variable, all UIs update)
- Easy to test independently
- Designer-friendly

## Architecture Flow

```
┌─ Health Component ─────────────────────────────────┐
│                                                    │
│  OnTakeDamage() → UpdateScriptableVariables()       │
│  OnHeal() → UpdateScriptableVariables()             │
│  OnDie() → UpdateScriptableVariables()              │
│                                                    │
└────────────────────────────────────────────────────┘
                    ↓
┌─ Scriptable Variables (Data Bridge) ───────────────┐
│                                                    │
│  PlayerCurrentHealth (ScriptableFloat)             │
│  PlayerMaxHealth (ScriptableFloat)                 │
│  PlayerHealthPercent (ScriptableFloat)             │
│  PlayerIsDead (ScriptableBool)                     │
│                                                    │
│  Events fired when updated!                        │
└────────────────────────────────────────────────────┘
                    ↓
┌─ HealthUIConnector (UI Listener) ──────────────────┐
│                                                    │
│  OnHealthChanged() → Update UI elements            │
│                                                    │
│  Updates:                                          │
│  • Health Text (\"50/100\")                         │
│  • Health Bar (fill 0-1)                          │
│  • Percent Text (\"50%\")                           │
│  • Status Text (\"HEALTHY\", \"LOW HEALTH\")         │
│                                                    │
└────────────────────────────────────────────────────┘
                    ↓
┌─ Canvas & UI Elements ─────────────────────────────┐
│                                                    │
│  Text: \"HP: 50/100\"                               │
│  Image: Health bar fills to 50%                   │
│  Text: \"50%\"                                      │
│  Text: \"LOW HEALTH\" (yellow color)                │
│                                                    │
└────────────────────────────────────────────────────┘
```

## Complete Setup Guide

### Phase 1: Create Scriptable Variable Assets

1. In your Assets folder, create a **ScriptableArchitecture** folder
2. Right-click → **Create → Shizounu → ScriptableArchitecture**
3. Create these ScriptableFloat assets:
   - `PlayerCurrentHealth`
   - `PlayerMaxHealth`
   - `PlayerHealthPercent`
4. Create this ScriptableBool asset:
   - `PlayerIsDead`

### Phase 2: Configure HealthComponent

1. Select your Player GameObject
2. In the **HealthComponent** inspector, find the **Scriptable Variables** section:

```
┌─ Health Component Inspector ──────────────────┐
│                                              │
│ Base Max Health: 100                         │
│                                              │
│ ┌─ Scriptable Variables ────────────────────┐│
│ │ Current Health Variable: [PlayerCurrentH] ││
│ │ Max Health Variable: [PlayerMaxHealth]    ││
│ │ Health Percent Variable: [PlayerHealthP]  ││
│ │ Is Dead Variable: [PlayerIsDead]          ││
│ │                                            ││
│ └────────────────────────────────────────────┘│
└──────────────────────────────────────────────┘
```

3. Assign each asset:
   - Drag **PlayerCurrentHealth** to \"Current Health Variable\"
   - Drag **PlayerMaxHealth** to \"Max Health Variable\"
   - Drag **PlayerHealthPercent** to \"Health Percent Variable\"
   - Drag **PlayerIsDead** to \"Is Dead Variable\"

### Phase 3: Create UI Elements

Create a Canvas with these elements:

```
Canvas
├─ HealthText (UI/Text)                    Display: \"50/100\"
├─ HealthContainer
│  └─ HealthBar (UI/Image)                Displays bar fill
├─ PercentText (UI/Text)                   Display: \"50%\"
└─ StatusText (UI/Text)                    Display: \"HEALTHY\"
```

### Phase 4: Add HealthUIConnector Component

1. Select your Canvas
2. **Add Component** → Search \"HealthUIConnector\"
3. In the inspector, assign UI elements:

```
┌─ Health UI Connector Inspector ──────────────────┐
│                                                 │
│ ┌─ Health Display ──────────────────────────────┐│
│ │ Health Text: [HealthText]                    ││
│ │ Health Bar Image: [HealthBar]                ││
│ │ Health Percent Text: [PercentText]           ││
│ │ Status Text: [StatusText]                    ││
│ │ Health Slider: (empty - optional)            ││
│ │                                              ││
│ └──────────────────────────────────────────────┘│
│                                                 │
│ ┌─ Scriptable Variables ───────────────────────┐│
│ │ Current Health Variable: [PlayerCurrentH]   ││
│ │ Max Health Variable: [PlayerMaxHealth]      ││
│ │ Health Percent Variable: [PlayerHealthP]    ││
│ │ Is Dead Variable: [PlayerIsDead]            ││
│ │                                              ││
│ └──────────────────────────────────────────────┘│
│                                                 │
│ ┌─ Colors ──────────────────────────────────────┐│
│ │ Healthy Color: [Green]                       ││
│ │ Low Health Color: [Yellow]                   ││
│ │ Critical Color: [Red]                        ││
│ │ Dead Color: [Gray]                           ││
│ │ Low Health Threshold: 0.5                    ││
│ │ Critical Threshold: 0.25                     ││
│ │                                              ││
│ └──────────────────────────────────────────────┘│
└──────────────────────────────────────────────────┘
```

4. Assign elements:
   - Drag **HealthText** to \"Health Text\"
   - Drag **HealthBar** to \"Health Bar Image\"
   - Drag **PercentText** to \"Health Percent Text\"
   - Drag **StatusText** to \"Status Text\"

5. Assign variables:
   - Drag **PlayerCurrentHealth** to \"Current Health Variable\"
   - Drag **PlayerMaxHealth** to \"Max Health Variable\"
   - Drag **PlayerHealthPercent** to \"Health Percent Variable\"
   - Drag **PlayerIsDead** to \"Is Dead Variable\"

6. (Optional) Configure colors:
   - **Healthy Color** - Green (50%+ health)
   - **Low Health Color** - Yellow (25-50% health)
   - **Critical Color** - Red (<25% health)
   - **Dead Color** - Gray (dead)

### Phase 5: Test

1. Play the game
2. Reduce health (console command or damage)
3. Watch the UI update automatically ✅

## UI Connector Components

### HealthUIConnector (Full Featured)

The main connector with all features.

**Features:**
- Displays current health text: \"50/100\"
- Fills health bar (0-1 range)
- Shows health percentage: \"50%\"
- Displays status text: \"HEALTHY\", \"LOW HEALTH\", \"CRITICAL\", \"DEAD\"
- Color-codes based on health threshold
- Updates when any health value changes
- Supports Slider component (optional)

**Requirements:**
- Current Health Variable (ScriptableFloat)
- Max Health Variable (ScriptableFloat)
- Health Percent Variable (ScriptableFloat)
- Is Dead Variable (ScriptableBool)
- At least one UI element (text, image, or slider)

**Status Thresholds:**
- HEALTHY - 50%+ health (green)
- LOW HEALTH - 25-50% health (yellow)
- CRITICAL - <25% health (red)
- DEAD - IsDead = true (gray)

### SimpleHealthBar

Lightweight health bar with gradient coloring.

**Features:**
- Simple color gradient support
- Minimal overhead
- Works with Slider or Image

**Setup:**
```csharp
var bar = GetComponent<SimpleHealthBar>();
bar.healthPercentVariable = playerHealthPercent;
bar.healthBarImage = healthBarImage;
bar.healthGradient = gradient; // Color.green to Color.red
```

### DamageNumberDisplay

Shows floating damage numbers above entities.

**Features:**
- Floating animation (upward movement)
- Fade out effect
- Customizable duration and speed
- Automatic damage calculation

**Setup:**
```csharp
var display = GetComponent<DamageNumberDisplay>();
display.currentHealthVariable = playerCurrentHealth;
display.damageNumberPrefab = floatingTextPrefab;
display.canvas = worldCanvas;
display.spawnPoint = damageLabelPosition;
```

## Advanced Setup: Multiple UIs

The beauty of Scriptable Variables is that multiple UIs can listen to the same data:

```
Game Logic (HealthComponent)
        ↓ Updates
    Scriptable Variables
        ↓
    ├─ HealthUIConnector (Main UI)
    ├─ DamageNumberDisplay (Floating numbers)
    ├─ BossHealthBar (Boss screen UI)
    └─ NetworkHealthSync (Multiplayer sync)
```

All UIs update automatically from one data source!

## Programmatic Setup

Instead of inspector assignment, you can setup in code:

```csharp
public class HealthUISetup : MonoBehaviour
{
    public ScriptableFloat currentHealthVar;
    public ScriptableFloat maxHealthVar;
    public ScriptableFloat healthPercentVar;
    public ScriptableBool isDeadVar;

    public Text healthText;
    public Image healthBar;
    public Text statusText;

    private void Start()
    {
        var connector = GetComponent<HealthUIConnector>();
        
        // Assign UI elements
        connector.SetUIElements(
            healthText: healthText,
            healthBarImage: healthBar,
            healthPercentText: null,
            statusText: statusText
        );
        
        // Assign variables
        connector.SetScriptableVariables(
            currentHealthVar,
            maxHealthVar,
            healthPercentVar,
            isDeadVar
        );
        
        // Configure colors
        connector.SetHealthColors(
            normal: Color.green,
            low: Color.yellow,
            critical: Color.red,
            dead: Color.gray
        );
    }
}
```

## Common Patterns

### Separate Health and Mana UIs

```
Game Logic
├─ HealthComponent → PlayerCurrentHealth, PlayerMaxHealth
└─ ManaComponent → PlayerCurrentMana, PlayerMaxMana

Canvas
├─ HealthUIConnector (uses health variables)
└─ ManaUIConnector (uses mana variables)
```

### Boss Health Bar

```csharp
public class BossHealthBar : MonoBehaviour
{
    public ScriptableFloat bossHealthPercent;
    public Image healthBar;

    private void Start()
    {
        // Subscribe to changes
        bossHealthPercent.OnRuntimeValueChanged += UpdateBar;
    }

    private void UpdateBar(float percent)
    {
        healthBar.fillAmount = percent;
        
        // Hide when full
        if (percent >= 1f)
            gameObject.SetActive(false);
    }
}
```

### Networked Health Display

```csharp
public class NetworkHealthSync : MonoBehaviour
{
    public ScriptableFloat playerHealth;
    
    [PunRPC]
    public void SyncHealth(float health, float maxHealth)
    {
        playerHealth.RuntimeValue = health;
        // All listening UIs update instantly!
    }
}
```

## Data Flow Example

When the player takes damage:

1. **HealthComponent receives TakeDamage()**
   ```csharp
   health.TakeDamage(25f, \"arrow\", DamageType.Physical);
   ```

2. **Health is calculated and reduced**
   ```csharp
   // 25 damage - 20 armor = 5 actual damage
   currentHealth = 95f;
   ```

3. **Scriptable variables are updated**
   ```csharp
   currentHealthVariable.RuntimeValue = 95f;
   healthPercentVariable.RuntimeValue = 0.95f;
   ```

4. **ScriptableVariable fires OnRuntimeValueChanged event**
   ```csharp
   currentHealthVariable.OnRuntimeValueChanged?.Invoke(95f);
   healthPercentVariable.OnRuntimeValueChanged?.Invoke(0.95f);
   ```

5. **HealthUIConnector receives the event**
   ```csharp
   private void OnHealthChanged(float newValue)
   {
       // Update all UI elements
   }
   ```

6. **UI elements update instantly**
   ```csharp
   healthText.text = \"95/100\";
   healthBar.fillAmount = 0.95f;
   percentText.text = \"95%\";
   statusText.text = \"HEALTHY\"; // Green color
   ```

## Troubleshooting

### UI Not Updating

**Check:**
1. Are scriptable variables assigned in HealthComponent? ✓
2. Are scriptable variables assigned in HealthUIConnector? ✓
3. Is HealthUIConnector on an active GameObject? ✓
4. Are health changes actually happening? Add debug
   ```csharp
   health.OnHealthChanged += (old, new, max) => Debug.Log($\"Health: {new}\");
   ```

### Status Text Wrong

Check the thresholds:
```csharp
healthUIConnector.lowHealthThreshold = 0.5f;        // 50%
healthUIConnector.criticalHealthThreshold = 0.25f;  // 25%
```

### Colors Not Changing

Verify:
1. Status Text is assigned
2. Colors are configured in inspector
3. Health is actually crossing thresholds

## See Also

- **[Health & Damage System](Health-Damage-System)** - Core health system
- **[Scriptable Architecture](Scriptable-Architecture)** - Full architecture guide
- **[Examples](Examples-and-Tutorials)** - Complete working examples
