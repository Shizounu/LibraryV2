# Scriptable Architecture - Complete Guide

The **Scriptable Architecture** system provides a data-driven framework for decoupling game logic from UI and other systems through scriptable variables and events.

## Core Concepts

### Scriptable Variables
A **ScriptableVariable** is a scriptable object that holds a runtime value and fires events when that value changes.

Types:
- **ScriptableFloat** - Floating point values
- **ScriptableInt** - Integer values
- **ScriptableBool** - Boolean values
- **ScriptableString** - String values
- Custom types (you can create your own)

### Events
Scriptable variables fire events when values change:
- **OnRuntimeValueChanged** - Value changed (primary event)
- **OnValueReset** - Value reset to initial
- Multiple listeners can subscribe

### Use Cases
- **UI Updates** - UI listens to game data
- **Game Events** - Global events without dependencies
- **Data Storage** - Persist game state
- **Networking** - Sync data between clients
- **Debugging** - Inspect values in play mode

## Benefits vs Direct References

**Traditional:**
```csharp
public class HealthUI : MonoBehaviour
{
    private HealthComponent health; // Direct reference!
    
    private void Update()
    {
        healthText.text = health.CurrentHealth.ToString();
    }
}
```

Problems: Tight coupling, hard to test, UI needs game object references

**Scriptable Architecture:**
```csharp
public class HealthUI : MonoBehaviour
{
    public ScriptableFloat healthVariable; // Just data!
    
    private void Start()
    {
        healthVariable.OnRuntimeValueChanged += UpdateUI;
    }
    
    private void UpdateUI(float newHealth)
    {
        healthText.text = newHealth.ToString();
    }
}
```

Benefits:
- Decoupled (UI knows nothing about gameplay)
- Testable (inject data, test UI)
- Reusable (any system can update the variable)
- Network-friendly (sync the variable)
- Multi-listener (many UIs, one data source)

## Creating Scriptable Variables

### In Inspector

1. Right-click in Assets → **Create → Shizounu → ScriptableArchitecture**
2. Select the type (ScriptableFloat, ScriptableInt, etc.)
3. Name it (e.g., \"PlayerHealth\")

### In Code

```csharp
[SerializeField]
private ScriptableFloat playerHealth;

// Or in ScriptableObject
[CreateAssetMenu(menuName = \"Data/Player Health\")]
public class PlayerHealthVariable : ScriptableFloat
{
}
```

## Using Scriptable Variables

### Reading Values

```csharp
float currentHealth = healthVariable.RuntimeValue;
```

### Writing Values

```csharp
healthVariable.RuntimeValue = 50f;
// OnRuntimeValueChanged event fires!
```

### Listening to Changes

```csharp
private void Start()
{
    // Subscribe
    healthVariable.OnRuntimeValueChanged += OnHealthChanged;
}

private void OnHealthChanged(float newValue)
{
    Debug.Log($\"Health is now {newValue}\");
}

private void OnDestroy()
{
    // Unsubscribe
    healthVariable.OnRuntimeValueChanged -= OnHealthChanged;
}
```

### Reset to Initial Value

```csharp
healthVariable.ResetValue();
// Resets to initial value and fires OnValueReset event
```

## Common Usage Patterns

### Health Display

```csharp
public class HealthDisplay : MonoBehaviour
{
    public ScriptableFloat currentHealth;
    public ScriptableFloat maxHealth;
    public Text healthText;

    private void Start()
    {
        currentHealth.OnRuntimeValueChanged += UpdateHealth;
    }

    private void UpdateHealth(float health)
    {
        healthText.text = $\"{health:F0}/{maxHealth.RuntimeValue:F0}\";
    }
}
```

### Game State Management

```csharp
public class GameState : MonoBehaviour
{
    public ScriptableInt score;
    public ScriptableInt level;
    public ScriptableBool isGameOver;
    
    private void OnGameOver()
    {
        isGameOver.RuntimeValue = true;
        // All listening UIs and systems respond!
    }
    
    public void AddScore(int points)
    {
        score.RuntimeValue += points;
        // Score UI updates automatically
    }
}
```

### Event Broadcasting

```csharp
// Without tight coupling
public class EventBroadcaster : MonoBehaviour
{
    public ScriptableString eventMessage;
    
    public void BroadcastEvent(string message)
    {
        eventMessage.RuntimeValue = message;
        // All listeners receive the message
    }
}

public class EventListener : MonoBehaviour
{
    public ScriptableString eventMessage;
    
    private void Start()
    {
        eventMessage.OnRuntimeValueChanged += HandleEvent;
    }
    
    private void HandleEvent(string message)
    {
        Debug.Log($\"Received: {message}\");
    }
}
```

### Difficulty Settings

```csharp
[CreateAssetMenu]
public class DifficultySettings
{
    public ScriptableFloat enemyDamageMultiplier;
    public ScriptableFloat enemyHealthMultiplier;
    public ScriptableFloat playerHealthRegenRate;
}

public class Enemy : MonoBehaviour
{
    public DifficultySettings difficulty;
    
    private void CalculateDamage()
    {
        float damage = baseDamage * difficulty.enemyDamageMultiplier.RuntimeValue;
    }
}
```

## ScriptableVariable API

### Properties

```csharp
// Get/set value
float value = scriptableFloat.RuntimeValue;
scriptableFloat.RuntimeValue = 100f;

// Get initial value
float initial = scriptableFloat.InitialValue;

// Check if at initial value
bool isInitial = scriptableFloat.IsAtInitialValue;
```

### Methods

```csharp
// Reset to initial
scriptableFloat.ResetValue();

// Modify value
scriptableFloat.RuntimeValue += 10f;
scriptableFloat.RuntimeValue *= 2f;
scriptableFloat.RuntimeValue -= 5f;
```

### Events

```csharp
// Listen to value changes
scriptableFloat.OnRuntimeValueChanged += (newValue) => 
{
    Debug.Log($\"New value: {newValue}\");
};

// Listen to resets
scriptableFloat.OnValueReset += () => 
{
    Debug.Log(\"Reset to initial value\");
};
```

## Creating Custom Variable Types

```csharp
using Shizounu.Library.ScriptableArchitecture;

[CreateAssetMenu(menuName = \"Data/Vector3\")]
public class ScriptableVector3 : ScriptableVariable<Vector3>
{
    // Inherits all functionality automatically
}

public class PlayerPositionTracker : MonoBehaviour
{
    public ScriptableVector3 playerPosition;
    
    private void Update()
    {
        playerPosition.RuntimeValue = transform.position;
        // UI can display player position anywhere in scene
    }
}
```

## Integration with Health System

See **[Health Damage UI Integration](Health-Damage-UI.md)** for a complete example of using Scriptable Variables with the Health and Damage system.

The Health system automatically updates these variables:
- Current Health (ScriptableFloat)
- Max Health (ScriptableFloat)
- Health Percent (ScriptableFloat)
- Is Dead (ScriptableBool)

## Best Practices

1. **Separate Data from Logic**
   ```csharp
   // Good - separate concerns
   public ScriptableFloat playerHealth;        // Data
   public HealthComponent healthComponent;     // Logic
   
   // Bad - mixing concerns
   public class PlayerData : MonoBehaviour { } // Does everything
   ```

2. **Use Descriptive Names**
   ```csharp
   // Good
   playerCurrentHealth, playerMaxHealth, playerHealthPercent
   
   // Bad
   health, h, hp
   ```

3. **Keep Scope Simple**
   ```csharp
   // Good - specific to player
   playerHealth, playerScore, playerAmmo
   
   // Bad - too broad
   gameData, values, vars
   ```

4. **Unsubscribe from Events**
   ```csharp
   private void Start()
   {
       variable.OnRuntimeValueChanged += Handler;
   }
   
   private void OnDestroy()
   {
       variable.OnRuntimeValueChanged -= Handler;
   }
   ```

5. **Update Asynchronously**
   ```csharp
   // Good - update causes events
   healthVariable.RuntimeValue = newHealth;
   
   // Avoid - direct access bypasses events
   health.currentValue = newHealth; // No event!
   ```

## Organization Tips

Create folder structure:
```
Assets/
├── ScriptableArchitecture/
│   ├── Player/
│   │   ├── PlayerCurrentHealth.asset
│   │   ├── PlayerMaxHealth.asset
│   │   └── PlayerScore.asset
│   ├── Game/
│   │   ├── GameIsActive.asset
│   │   ├── GamePaused.asset
│   │   └── CurrentLevel.asset
│   ├── UI/
│   │   ├── SelectedHero.asset
│   │   └── MenuState.asset
│   └── Events/
│       ├── OnGameOver.asset
│       └── OnLevelComplete.asset
```

## Performance Considerations

- **Lightweight** - Just a wrapper around a value
- **No Update Cost** - Only processes on value changes
- **Memory Efficient** - Single asset shared by all systems
- **Network Friendly** - Single source of truth easy to sync

## Debugging

Scriptable variables are inspector-friendly:

1. Select the asset in Project
2. Change RuntimeValue in Inspector (play mode)
3. See changes reflected in game immediately
4. Perfect for balance testing and debugging

You can also add logging:

```csharp
public class DebugScriptableFloat : ScriptableFloat
{
    public override void SetValue(float value)
    {
        Debug.Log($\"{name} changed from {RuntimeValue} to {value}\");
        base.SetValue(value);
    }
}
```

## See Also

- **[Health Damage UI](Health-Damage-UI.md)** - Complete integration example
- **[Architecture Overview](Architecture.md)** - System architecture
- **[Examples](Examples-and-Tutorials.md)** - More examples
