# Shizounu Library V2 - Documentation

Welcome to the Shizounu Library V2 documentation wiki. This is a modular Unity library with reusable systems for game development.

## Documentation Structure

### Getting Started
- **[Getting Started](Getting-Started.md)** - Installation and baseline setup
- **[Quick Start](Quick-Start.md)** - Minimal setup and examples
- **[Architecture Overview](Architecture.md)** - System relationships

### Core Systems

The library provides several interconnected systems that can be used together or independently.

#### Buff/Debuff System
- **[Buff/Debuff System Guide](Buff-Debuff-System.md)** - Effect management, modifiers, stacking
- **[Creating Custom Effects](Buff-Debuff-System.md#creating-custom-effects)** - Custom effect patterns

#### Health and Damage System
- **[Health and Damage Guide](Health-Damage-System.md)** - Health, damage, healing
- **[UI Integration](Health-Damage-UI.md)** - Scriptable Architecture UI binding
- Damage types: Physical, Magic, True
- Integrates with BuffDebuff modifiers

#### Scriptable Architecture
- **[Scriptable Architecture Guide](Scriptable-Architecture.md)** - Variables and events
- UI decoupling and data flow

#### Character Controllers
- **[Character Controllers Guide](Character-Controllers.md)** - Movement and control

#### Dialogue, AI, Tween, Update, Utilities
- **[Other Systems](Other-Systems.md)** - Overview and entry points

### Reference
- **[API Reference](API-Reference.md)** - API lookup
- **[Examples and Tutorials](Examples-and-Tutorials.md)** - Working samples
- **[Best Practices](Best-Practices.md)** - Patterns and recommendations
- **[Troubleshooting](Troubleshooting.md)** - Common issues and fixes

## Common Entry Points

1. Start here: [Getting Started](Getting-Started.md)
2. Health system: [Health and Damage](Health-Damage-System.md)
3. Buffs and effects: [Buff/Debuff System](Buff-Debuff-System.md)
4. Examples: [Examples and Tutorials](Examples-and-Tutorials.md)
5. API lookup: [API Reference](API-Reference.md)

## Feature Overview

| System | Purpose | Key Feature |
|--------|---------|-------------|
| **Buff/Debuff** | Game effect management | Event-driven, stackable effects |
| **Health** | Entity health & damage | Integrated with buffs, 3 damage types |
| **ScriptableArchitecture** | Data architecture | UI-logic decoupling |
| **Character Controllers** | Player movement | Ready-to-use controller |
| **Dialogue** | Story & conversations | Branching dialogue trees |
| **AI** | NPC behaviors | Pathfinding, behavior trees |
| **Tween** | Smooth animations | Lightweight tweening |
| **Update System** | Frame management | Centralized update coordination |
| **Utilities** | Helper functions | Extensions and algorithms |

## Design Principles

- **Modularity** - Use only the systems you need
- **Extensibility** - Extend with custom effects and components
- **Decoupling** - Data and UI separation via Scriptable Architecture
- **Event-driven** - Prefer events over polling
- **Performance** - Minimize per-frame work
- **Reusability** - Components are drop-in and reusable

## Example Setup Patterns

### Common Setup Patterns

**Health & Buffs:**
```csharp
// Simple 3-component setup
gameObject.AddComponent<HealthComponent>();
gameObject.AddComponent<BuffDebuffSystem>();
gameObject.AddComponent<DamageDealer>();
```

**Health with UI:**
```csharp
// Create scriptable variables in Inspector
// Connect HealthComponent to variables
// Add HealthUIConnector to Canvas
// UI updates via scriptable variables
```

**Custom Game Logic:**
```csharp
// Query buff modifiers in your systems
float damageBonus = buffSystem.GetModifierMultiplier("damage.multiplier");
float finalDamage = baseDamage * damageBonus;
```

## Documentation Conventions

Throughout this wiki:

- **Code blocks** show C# examples
- **Bold text** highlights important concepts
- **Links** navigate between related topics
- **Tables** organize reference information
- **Warning** indicates important cautions
- **Tip** provides helpful hints
- **Note** adds context or explanation

## Support

1. **Check the relevant guide** for your system
2. **Review [Examples](Examples-and-Tutorials.md)** for working code
3. **See [Troubleshooting](Troubleshooting.md)** for common issues
4. **Read [Best Practices](Best-Practices.md)** for design patterns

---

**Last Updated:** February 2026
**Library Version:** 2.0
**Unity Version Required:** 2021.3 or later
