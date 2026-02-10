# Shizounu Library V2 - Wiki

Entry point for the Shizounu Library V2 documentation.

## Navigation

### Start Here
- **[Home](Home.md)** - Complete documentation index
- **[Getting Started](Getting-Started.md)** - Installation and first steps
- **[Quick Start](Quick-Start.md)** - Cheat sheet and one-pagers
- **[Architecture Overview](Architecture.md)** - System relationships

### Systems
- **[Buff/Debuff System](Buff-Debuff-System.md)** - Core effect management
- **[Health & Damage System](Health-Damage-System.md)** - Complete health system
- **[Health & Damage UI](Health-Damage-UI.md)** - UI integration guide
- **[Scriptable Architecture](Scriptable-Architecture.md)** - Data decoupling
- **[Character Controllers](Character-Controllers.md)** - Movement systems
- **[Other Systems](Other-Systems.md)** - Dialogue, AI, Tween, Update, Utility

### Learning and Reference
- **[Examples and Tutorials](Examples-and-Tutorials.md)** - Working code samples
- **[Best Practices](Best-Practices.md)** - Design patterns
- **[API Reference](API-Reference.md)** - Quick API lookup
- **[Troubleshooting](Troubleshooting.md)** - Common issues and fixes

## Tasks

- Get started: [Quick Start](Quick-Start.md)
- Architecture overview: [Architecture Overview](Architecture.md)
- Implement health and damage: [Health and Damage System](Health-Damage-System.md)
- Use buffs and debuffs: [Buff/Debuff System](Buff-Debuff-System.md)
- UI integration: [Health and Damage UI](Health-Damage-UI.md) or [Scriptable Architecture](Scriptable-Architecture.md)
- Examples: [Examples and Tutorials](Examples-and-Tutorials.md)
- API lookup: [API Reference](API-Reference.md)
- Troubleshooting: [Troubleshooting](Troubleshooting.md)
- Code patterns: [Best Practices](Best-Practices.md)

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

1. Read [Getting Started](Getting-Started.md)
2. Copy a code example from [Quick Start](Quick-Start.md)
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

[Full guide](Health-Damage-UI.md)

### Damage Calculation
1. Get attacker's damage buffs
2. Apply to base damage
3. Get target's resistances
4. Reduce by resistances

[Full guide](Health-Damage-System.md)

### Custom Buff
1. Inherit from BuffDebuffEffect
2. Implement GetModifiers()
3. Optional: OnApply(), OnRemove()
4. Use it

[Full guide](Buff-Debuff-System.md)

## Features at a Glance

| Feature | Purpose | Learn |
|---------|---------|-------|
| **Buff/Debuff** | Effect management | [Guide](Buff-Debuff-System.md) |
| **Health** | Entity health tracking | [Guide](Health-Damage-System.md) |
| **Damage** | Damage calculation | [Guide](Health-Damage-System.md) |
| **Modifiers** | Stat modification | [API Reference](API-Reference.md) |
| **Scriptable Variables** | Data decoupling | [Guide](Scriptable-Architecture.md) |
| **UI Integration** | Health UI | [Guide](Health-Damage-UI.md) |
| **Character Controller** | Movement | [Guide](Character-Controllers.md) |
| **Dialogue** | Conversations | [Overview](Other-Systems.md) |
| **AI** | NPC behavior | [Overview](Other-Systems.md) |
| **Tween** | Smooth animation | [Overview](Other-Systems.md) |

## FAQ

**Q: Do I need all systems?**
A: No. Use only what your project needs.

**Q: Can I create custom effects?**
A: Yes. Inherit from `BuffDebuffEffect` and implement `GetModifiers()`.

**Q: How do I decouple UI from gameplay?**
A: Use Scriptable Variables and event listeners.

**Q: What is the performance impact?**
A: Effects and UI updates are event-driven and avoid per-frame polling.

**Q: Can I integrate with existing code?**
A: Yes. The systems are component-based and modular.

[More FAQs](Troubleshooting.md)

## Version Info

- **Library Version:** 2.0
- **Unity Version:** 2021.3+
- **C# Version:** 9.0+

## Quick Links

- [Home](Home.md) - Full index
- [Getting Started](Getting-Started.md) - Installation
- [Examples](Examples-and-Tutorials.md) - Working code
- [API](API-Reference.md) - Complete API
- [Troubleshooting](Troubleshooting.md) - Help
- [Best Practices](Best-Practices.md) - Code patterns
