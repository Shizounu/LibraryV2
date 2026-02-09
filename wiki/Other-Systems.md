# Other Systems Quick Reference

Quick reference guides for Dialogue, AI, Tween, Update System, and Utilities.

## Dialogue System

### Purpose
Branching dialogue trees and NPC conversations.

### Quick Start

```csharp
public class DialogueExample : MonoBehaviour
{
    public DialogueTree tree;

    public void StartConversation()
    {
        DialogueManager.instance.StartDialogue(tree);
    }
}
```

### Key Features
- Branching dialogue choices
- Character interactions
- Conditional branching
- Event callbacks
- Save/Load state

### Learn More
See `Assets/Library/Dialogue/` for complete implementation.

---

## Game AI System

### Purpose
AI behaviors, pathfinding, and decision making.

### Quick Start

```csharp
public class EnemyAI : MonoBehaviour
{
    private AIAgent agent;

    private void Start()
    {
        agent = GetComponent<AIAgent>();
        agent.SetBehavior(new PatrolBehavior(patrolPoints));
    }

    private void Update()
    {
        agent.Update();
    }
}
```

### Key Features
- Behavior trees
- Pathfinding
- Decision systems
- Agent coordination
- State management

### Learn More
See `Assets/Library/Game AI/` for detailed docs.

---

## Tween System

### Purpose
Smooth animations and property transitions.

### Quick Start

```csharp
public class TweenExample : MonoBehaviour
{
    private void Start()
    {
        // Tween position
        Tween.To(
            () => transform.position,
            value => transform.position = value,
            targetPosition,
            duration: 2f
        );
    }
}
```

### Key Features
- Position, rotation, scale tweening
- Custom property tweening
- Easing functions
- Chaining tweens
- Callbacks (OnStart, OnComplete)

### Common Tweens

```csharp
// Position
Tween.To(transform, \"localPosition\", targetPos, 1f);

// Scale
Tween.To(transform, \"localScale\", targetScale, 1f);

// Rotation
Tween.To(transform, \"localEulerAngles\", targetRot, 1f);

// Color
Tween.To(image, \"color\", targetColor, 1f);

// Custom property
Tween.To(GetValue, SetValue, targetValue, 1f);
```

### Learn More
See `Assets/Library/Tween/` for documentation.

---

## Update System

### Purpose
Centralized frame update coordination with custom intervals.

### How It Works

Instead of MonoBehaviour.Update():
- Effects update on a timer
- Custom intervals per system
- Coordinated update cycle
- Thread support for background tasks

### Usage

```csharp
buffSystem.UpdateInterval = 0.1f; // Update every 0.1 seconds
// buffSystem.UpdateInterval = 0f; // Update every frame
```

### Key Features
- Configurable update intervals
- Centralized update management
- Less frame processing
- Predictable performance
- Thread integration ready

### Learn More
See `Assets/Library/Update System/` for details.

---

## Utilities

### Purpose
Helper functions, extensions, and common algorithms.

### Common Extensions

```csharp
// Array extensions
int randomIndex = array.GetRandomElement();

// Math extensions
float clamped = value.Clamp01();
bool inRange = value.InRange(min, max);

// Vector extensions
vector.SetX(5f);
float distance = vector.DistanceTo(other);

// String extensions
string capitalized = text.Capitalize();
bool contains = str.ContainsIgnoreCase(\"text\");
```

### Utility Functions

```csharp
// Probability
if (Utilities.RandomChance(0.25f)) { } // 25% chance

// Array operations
Utilities.Shuffle(array);
T random = Utilities.RandomElement(array);

// Easing
float eased = Utilities.Ease(t, EaseType.InOutQuad);
```

### Learn More
See `Assets/Library/Utility/` for the full reference.

---

## Integration Tips

### Combining Systems

```csharp
// Buff + Movement
var slowEffect = new SlowEffect(3f, 0.5f);
buffSystem.AddEffect(slowEffect);
// Movement automatically queries MOVEMENT_SPEED_MULTIPLIER

// Health + UI + Scriptable
healthComponent.TakeDamage(25f, \"attack\", DamageType.Physical);
// → Updates playerCurrentHealth variable
// → HealthUIConnector sees change
// → UI updates automatically

// AI + Dialogue
if (enemy.health.IsDead)
{
    DialogueManager.StartDialogue(victoryDialogue);
}
```

---

## System Comparison

| System | Best For | Learning Curve |
|--------|----------|-----------------|
| **Dialogue** | Story & NPCs | Medium |
| **AI** | Enemy behavior | Medium-High |
| **Tween** | Animations | Low |
| **Update** | Perf tuning | Low |
| **Utilities** | Helper code | Low |

---

## See Also

- **[Architecture Overview](Architecture)** - System relationships
- **[Examples](Examples-and-Tutorials)** - Working code
- **[Best Practices](Best-Practices)** - Design tips
