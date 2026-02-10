# Shizounu Library V2 - Wiki

Entry point for the Shizounu Library V2 documentation.

## Navigation

### Start Here
- **[Home](Home)** - Complete documentation index
- **[Getting Started](Getting-Started)** - Installation and first steps
- **[Quick Start](Quick-Start)** - Cheat sheet and one-pagers
- **[Architecture Overview](Architecture)** - System relationships

### Systems
- **[Buff/Debuff System](Buff-Debuff-System)** - Core effect management
- **[Health & Damage System](Health-Damage-System)** - Complete health system
- **[Health & Damage UI](Health-Damage-UI)** - UI integration guide
- **[Scriptable Architecture](Scriptable-Architecture)** - Data decoupling
- **[Character Controllers](Character-Controllers)** - Movement systems
- **[Other Systems](Other-Systems)** - Dialogue, AI, Tween, Update, Utility

### Learning and Reference
- **[Examples and Tutorials](Examples-and-Tutorials)** - Working code samples
- **[Best Practices](Best-Practices)** - Design patterns
- **[API Reference](API-Reference)** - Quick API lookup
- **[Troubleshooting](Troubleshooting)** - Common issues and fixes

## Tasks

- Get started: [Quick Start](Quick-Start)
- Architecture overview: [Architecture Overview](Architecture)
- Implement health and damage: [Health and Damage System](Health-Damage-System)
- Use buffs and debuffs: [Buff/Debuff System](Buff-Debuff-System)
- UI integration: [Health and Damage UI](Health-Damage-UI) or [Scriptable Architecture](Scriptable-Architecture)
- Examples: [Examples and Tutorials](Examples-and-Tutorials)
- API lookup: [API Reference](API-Reference)
- Troubleshooting: [Troubleshooting](Troubleshooting)
- Code patterns: [Best Practices](Best-Practices)

## Documentation Structure

```
Getting Started ──────┬─→ Buff/Debuff System
                      ├─→ Health & Damage System
                      ├─→ Health & Damage UI Integration
                      ├─→ Scriptable Architecture
                      │
Quick Start ──────────┼─→ Examples & Tutorials
                      ├─→ API Reference
                      │
Architecture ─────────┼─→ Best Practices
                      ├─→ Troubleshooting
                      │
Other Systems ────────┘
```

## Quick Start Path

1. Read [Getting Started](Getting-Started)
2. Copy a code example from [Quick Start](Quick-Start)
3. Run it in Unity

## Key Concepts

### Buff/Debuff System
A flexible framework for applying effects (buffs/debuffs) to entities that modify game systems.

**Example:**
```csharp
buffSystem.AddEffect(new ArmorEffect(\"armor\", -1f, 20f));
float armor = buffSystem.GetModifierSum(HealthModifiers.ARMOR); // 20
```

### Health & Damage
Complete health management with damage types (Physical/Magic/True) and integrates with buffs.

**Example:**
```csharp
health.TakeDamage(50f, \"attack\", DamageType.Physical);
// Automatically applies armor modifiers
```

### Scriptable Architecture
Event-driven data system that completely decouples UI from gameplay.

**Example:**
```csharp
healthVariable.RuntimeValue = newHealth; // UI updates via events
```

## Common Workflows

### Health + UI Setup
1. Create ScriptableVariables
2. Assign to HealthComponent
3. Add HealthUIConnector
4. UI updates via events

[Full guide](Health-Damage-UI)

### Damage Calculation
1. Get attacker's damage buffs
2. Apply to base damage
3. Get target's resistances
4. Reduce by resistances

[Full guide](Health-Damage-System)

### Custom Buff
1. Inherit from BuffDebuffEffect
2. Implement GetModifiers()
3. Optional: OnApply(), OnRemove()
4. Use it

[Full guide](Buff-Debuff-System)

## Features at a Glance

| Feature | Purpose | Learn |
|---------|---------|-------|
| **Buff/Debuff** | Effect management | [Guide](Buff-Debuff-System) |
| **Health** | Entity health tracking | [Guide](Health-Damage-System) |
| **Damage** | Damage calculation | [Guide](Health-Damage-System) |
| **Modifiers** | Stat modification | [API Reference](API-Reference) |
| **Scriptable Variables** | Data decoupling | [Guide](Scriptable-Architecture) |
| **UI Integration** | Health UI | [Guide](Health-Damage-UI) |
| **Character Controller** | Movement | [Guide](Character-Controllers) |
| **Dialogue** | Conversations | [Overview](Other-Systems) |
| **AI** | NPC behavior | [Overview](Other-Systems) |
| **Tween** | Smooth animation | [Overview](Other-Systems) |

## Version Info

- **Library Version:** 2.0
- **Unity Version:** 2021.3+
- **C# Version:** 9.0+

## Quick Links

- [Home](Home) - Full index
- [Getting Started](Getting-Started) - Installation
- [Examples](Examples-and-Tutorials) - Working code
- [API](API-Reference) - Complete API
- [Troubleshooting](Troubleshooting) - Help
- [Best Practices](Best-Practices) - Code patterns
