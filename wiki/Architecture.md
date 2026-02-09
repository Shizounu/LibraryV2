# Architecture Overview

The **Shizounu Library V2** is organized into independent systems that work together seamlessly. This page explains the overall structure and how systems integrate.

## System Organization

```
┌─────────────────────────────────────────────────────────────────┐
│                        SHIZOUNU LIBRARY V2                      │
└─────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────┐
│  FOUNDATION SYSTEMS (Everything builds on these)               │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  ┌─ Update System ────────┐  ┌─ Scriptable Architecture ─┐    │
│  │ • Frame coordination   │  │ • Variables                │    │
│  │ • Custom intervals     │  │ • Events                   │    │
│  │ • Thread support       │  │ • UI decoupling            │    │
│  └────────────────────────┘  └────────────────────────────┘    │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────┐
│  CORE GAMEPLAY SYSTEMS                                          │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  ┌─ Buff/Debuff ─────────┐  ┌─ Health & Damage ──────────┐    │
│  │ • Effect management   │  │ • Health tracking          │    │
│  │ • Modifiers           │  │ • Damage calculation       │    │
│  │ • Stacking            │  │ • 3 damage types           │    │
│  │ • Events              │  │ • Integrated with Buffs    │    │
│  └────────────────────────┘  └────────────────────────────┘    │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
         ↑                                  ↑
         └───── These use Buff/Debuff for modifiers ─────┬
                                                         │
┌────────────────────────────────────────────────────────▼─────────┐
│  SPECIALIZED SYSTEMS                                            │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  ┌─ Character Controllers ┐  ┌─ Dialogue System ──────┐         │
│  │ • Player movement      │  │ • Dialogue trees       │         │
│  │ • Interaction handling │  │ • Branching choices    │         │
│  │ • Input processing     │  │ • Character interactions│         │
│  └────────────────────────┘  └────────────────────────┘         │
│                                                                 │
│  ┌─ Game AI ─────────────┐  ┌─ Tween System ────────┐          │
│  │ • AI behaviors        │  │ • Animations           │          │
│  │ • Pathfinding         │  │ • Smooth transitions   │          │
│  │ • Decision trees      │  │ • Lightweight tweening │          │
│  └────────────────────────┘  └────────────────────────┘          │
│                                                                 │
│  ┌─ Utility ─────────────────────────────────────────────┐      │
│  │ • Helper functions and extensions                    │      │
│  └────────────────────────────────────────────────────────┘      │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

## Design Principles

### 1. Modularity
Each system is independent:
- Use what you need
- Don't need it? Leave it out
- No required dependencies between systems
- Except Foundation → Everything else

### 2. Event-Driven
Systems communicate through events:
```
Game Logic → Events → UI/Other Systems
```

Not:
```
Game Logic → Direct References ← UI/Other Systems (BAD!)
```

### 3. Decoupling
- Game logic doesn't know about UI
- UI doesn't know about game logic
- Both know about Scriptable Variables
- Perfect for team workflows

### 4. Extensibility
- Inherit from base classes
- Create custom effects
- Override behavior
- Add new systems easily

## Integration Examples

### Example 1: Health + UI

```
Player.HealthComponent
        ↓ TakeDamage()
PlayerCurrentHealth (ScriptableFloat)
        ↓ OnRuntimeValueChanged event
HealthUIConnector
        ↓
UI Elements update
```

**Key Point:** UI never knows about HealthComponent!

### Example 2: Buff/Debuff with Movement

```
BuffDebuffSystem
        ↑
        ├─ Add SlowEffect
        └─ AddSpeedEffect
        
PlayerMovement queries:
GetModifierMultiplier(MOVEMENT_SPEED_MULTIPLIER)
        ↓
Uses modified speed
```

**Key Point:** Effects don't know about movement!

### Example 3: Dialogue + Game State

```
DialogueSystem
        ↓
Triggers game events
        ↓
GameState updates ScriptableVariables
        ↓
UI responds to changes
```

**Key Point:** Dialogue doesn't know about systems it affects!

## Dependency Graph

```
Foundation:
  Update System
  Scriptable Architecture

Core Systems (depend on Foundation):
  Buff/Debuff System
  Health & Damage System

Specialized Systems (can use Foundation + Core):
  Character Controllers
  Dialogue System
  Game AI
  Tween System
  Utility
```

## Communication Patterns

### Pattern 1: Event-Based
```csharp
// GameLogic fires event through ScriptableVariable
playerHealth.RuntimeValue = 50;

// UI listens
healthVariable.OnRuntimeValueChanged += UpdateUI;
```

**Best For:** Health, score, state changes

### Pattern 2: Query-Based
```csharp
// System queries current state
float totalArmor = buffSystem.GetModifierSum(ARMOR);

// Game update uses value
float damageReduction = totalArmor / 100f;
```

**Best For:** Modifiers, buffs, temporary effects

### Pattern 3: Component-Based
```csharp
// Direct component access (when appropriate)
health.TakeDamage(25);

// Not ideal for UI, but fine for internal logic
```

**Best For:** Internal game logic

## Typical Game Flow

```
Start Game
  ↓
Initialize Foundation Systems
  ├─ Update System starts
  └─ Create Scriptable Variables
  ↓
Initialize Core Systems
  ├─ Create BuffDebuffSystem
  ├─ Create HealthComponent
  └─ Connect to ScriptableVariables
  ↓
Initialize Specialized Systems
  ├─ Character Controller
  ├─ Dialogue System
  ├─ AI Agents
  └─ Create UI (listens to Variables)
  ↓
Game Running
  ├─ Input → CharacterController
  ├─ Actions → BuffDebuff/Health/AI
  ├─ Changes → ScriptableVariables
  ├─ UI updates automatically
  └─ Repeat...
  ↓
Game Over
  ├─ Save game state from Variables
  ├─ Clean up components
  └─ Shutdown systems
```

## Choosing What to Use

### For Health & Damage
→ **Health & Damage System** - Full featured, handles everything

### For Temporary Effects
→ **Buff/Debuff System** - Flexible, stackable, event-driven

### For Player Movement
→ **Character Controllers** - Ready-to-use controller

### For Smooth Animations
→ **Tween System** - Lightweight, easy tweening

### For AI Behavior
→ **Game AI** - Decision trees, pathfinding

### For Conversations
→ **Dialogue System** - Branching dialogue trees

### For UI Architecture
→ **Scriptable Architecture** - Variables, decoupling, events

### For Helpers & Extensions
→ **Utility** - Common algorithms, extensions

## Extending the Library

### Creating a Custom Buff

```csharp
public class CustomEffect : BuffDebuffEffect
{
    public CustomEffect() : base(\"custom\", duration: 5f) { }
    
    public override IStatModifier[] GetModifiers()
    {
        return new[] { /*modifiers*/ };
    }
}

// Use it
buffSystem.AddEffect(new CustomEffect());
```

### Creating a Custom System

```csharp
public class MyCustomSystem : MonoBehaviour
{
    public ScriptableFloat healthVariable; // Listen to events
    private BuffDebuffSystem buffs;        // Query modifiers
    
    private void Start()
    {
        buffs = GetComponent<BuffDebuffSystem>();
        healthVariable.OnRuntimeValueChanged += OnHealthChanged;
    }
    
    private void OnHealthChanged(float newHealth)
    {
        // React to health changes
    }
}
```

## Performance & Optimization

### Foundation Systems (Lightweight)
- Update System: Minimal overhead
- Scriptable Architecture: Just value holders

### Core Systems (Moderate)
- Buff/Debuff: Only processes active effects
- Health: Simple calculations, no allocation

### Specialized Systems (Variable)
- Character Controllers: Physics-based
- Game AI: Path computation
- Dialogue: Usually just UI

**General guideline:** Profile your specific use case!

## Module Dependencies

```
Update System (Foundation)
  ↓ (used by)
  ├─ Buff/Debuff System
  ├─ Health & Damage System
  ├─ Character Controllers
  └─ Game AI

Scriptable Architecture (Foundation)
  ↓ (used by)
  ├─ Health System (UI integration)
  ├─ Character Controllers (input)
  └─ Any custom system
```

## Naming Conventions

- **Systems:** PascalCase with \"System\" suffix
  - `BuffDebuffSystem`, `HealthComponent`, `DialogueSystem`
- **Variables:** camelCase, descriptive
  - `playerCurrentHealth`, `enemyDamageMultiplier`
- **Effects:** PascalCase with \"Effect\" suffix
  - `ArmorEffect`, `SlowEffect`, `HealEffect`
- **Modifiers:** UPPER_SNAKE_CASE constants
  - `ARMOR`, `DAMAGE_MULTIPLIER`, `HEALTH_REGEN`

## See Also

- **[Getting Started](Getting-Started)** - Quick start
- **[Individual System Guides](Home#core-systems)** - Detailed guides
- **[Best Practices](Best-Practices)** - Design recommendations
- **[Examples](Examples-and-Tutorials)** - Working code samples
