# Random System

A comprehensive, reproducible random number generation system for Unity with history tracking, snapshots, and advanced distributions.

## Table of Contents

1. [Overview](#overview)
2. [Core Concepts](#core-concepts)
3. [Quick Start](#quick-start)
4. [RngContext](#rngcontext)
5. [RngUser](#rnguser)
6. [IRngSource](#irngsource)
7. [Distributions](#distributions)
8. [History & Snapshots](#history--snapshots)
9. [Random Tables](#random-tables)
10. [Quick RNG](#quick-rng)
11. [Best Practices](#best-practices)

---

## Overview

The Random System provides reproducible random number generation with advanced features like history tracking, state snapshots, and multiple RNG users working independently within the same context.

### Key Features

- **Reproducibility**: Same seed always produces same results
- **History Tracking**: Record every random call for debugging
- **Snapshots**: Save and restore RNG state
- **Multiple Users**: Independent RNG streams with shared history
- **Distributions**: Gaussian, exponential, dice rolls, and more
- **Performance**: Fast Xorshift algorithm
- **Integration**: Works seamlessly with Generation Algorithms

### Namespace

```csharp
using Shizounu.Library.RandomSystem;
```

---

## Core Concepts

### RNG Hierarchy

```
RngContext (manages everything)
├── History (tracks all calls)
├── Snapshots (saved states)
└── RngUsers (individual consumers)
    └── IRngSource (RNG algorithm)
```

### Why Use This System?

**Traditional System.Random Problems:**
- Not reproducible across platforms
- Hard to debug random issues
- Can't rewind or branch simulations
- Difficult to manage multiple RNG streams

**Random System Solutions:**
- Guaranteed reproducibility with seeds
- Full history of every random call
- Save/restore RNG state anytime
- Multiple independent RNG users
- Rich API with distributions

---

## Quick Start

### Simple One-Off RNG

```csharp
using Shizounu.Library.RandomSystem;

// Create an RNG source
var rng = new XorshiftRng(12345);

// Generate values
uint randomUInt = rng.Next();
float randomFloat = rng.NextFloat();  // [0, 1)
int randomInt = rng.NextInt(10);      // [0, 10)
int randomRange = rng.NextInt(5, 15); // [5, 15)
```

### Using RngContext and RngUser

```csharp
// Create context (central manager)
var context = new RngContext();

// Create user for specific purpose
var combatRng = context.GetOrCreateUser("Combat", seed: 12345);

// Generate values through user
float attackRoll = combatRng.NextFloat("attack_roll");
int damage = combatRng.NextInt(1, 20, "damage");

// Check history
Debug.Log($"Total RNG calls: {context.History.TotalCallCount}");
Debug.Log($"Combat made {combatRng.GenerationCount} calls");
```

### With Generation Algorithms

```csharp
var context = new RngContext();
var levelGenRng = context.GetOrCreateUser("LevelGen", seed: 42);

// Pass to algorithms
var bsp = new BspBuilder(levelGenRng.Source);
var ca = new CellularAutomata(100, 100, rules, levelGenRng.Source);

// All RNG tracked automatically
```

---

## RngContext

Central manager for reproducible RNG. Manages multiple users, tracks history, creates snapshots.

### Creation

```csharp
var context = new RngContext();
```

### Managing Users

```csharp
// Create or get existing user
var user = context.GetOrCreateUser("PlayerLoot", seed: 123);

// Get by name
if (context.TryGetUser("PlayerLoot", out var foundUser))
{
    // Use foundUser
}

// Get by ID
if (context.TryGetUser(userId, out var foundUser))
{
    // Use foundUser
}

// Remove user
context.RemoveUser(userId);
```

### History Control

```csharp
// Enable/disable history recording
context.SetHistoryRecording(false);  // Disable for performance
context.SetHistoryRecording(true);   // Re-enable

// Clear history (keeps RNG state)
context.ClearHistory();

// Check if recording
bool isRecording = context.RecordHistory;
```

### Snapshots

```csharp
// Create snapshot of current state
context.CreateSnapshot("before_boss_fight");

// Restore from snapshot
context.RestoreSnapshot(snapshotIndex);

// Clear all snapshots
context.ClearSnapshots();

// Access snapshots
foreach (var snapshot in context.Snapshots)
{
    Debug.Log(snapshot.Label);
}
```

### Properties

```csharp
// Get all users
IReadOnlyDictionary<int, RngUser> users = context.Users;

// Get history
RngHistory history = context.History;

// Get snapshots
IReadOnlyList<RngSnapshot> snapshots = context.Snapshots;
```

---

## RngUser

Represents an individual consumer of random numbers from a specific RNG source.

### Creation

```csharp
// Through context (recommended)
var user = context.GetOrCreateUser("EnemyAI", seed: 456);

// Direct creation (advanced)
var source = new XorshiftRng(456);
var user = new RngUser(id: 0, name: "EnemyAI", source, context);
```

### Generating Values

```csharp
// Basic values
uint value = user.Next();
float f = user.NextFloat();    // [0, 1)
double d = user.NextDouble();  // [0, 1)

// Integer ranges
int roll = user.NextInt(100);     // [0, 100)
int range = user.NextInt(5, 10);  // [5, 10)

// With labels for debugging
float crit = user.NextFloat("crit_chance");
int dmg = user.NextInt(1, 20, "damage_roll");
```

### Properties

```csharp
int id = user.Id;
string name = user.Name;
IRngSource source = user.Source;
int callCount = user.GenerationCount;
```

### Use Cases

```csharp
// Separate RNG for different systems
var combatRng = context.GetOrCreateUser("Combat");
var lootRng = context.GetOrCreateUser("Loot");
var aiRng = context.GetOrCreateUser("AI");

// Now combat RNG doesn't affect loot drops
bool hitEnemy = combatRng.NextFloat() < 0.8f;
var lootItem = lootRng.NextInt(lootTable.Count);
```

---

## IRngSource

Interface for RNG algorithm implementations. Allows pluggable RNG backends.

### Interface

```csharp
public interface IRngSource
{
    uint Seed { get; }
    void SetSeed(uint seed);
    uint Next();
    float NextFloat();
    double NextDouble();
    int NextInt(int maxValue);
    int NextInt(int minValue, int maxValue);
    IRngSource Clone();
}
```

### XorshiftRng (Default Implementation)

Fast, high-quality RNG based on Xorshift32 algorithm.

```csharp
// Create with seed
var rng = new XorshiftRng(12345);

// Default seed
var rng = new XorshiftRng();

// Reseed
rng.SetSeed(99999);

// Clone for independent stream
IRngSource clone = rng.Clone();
```

### RngSystemRandom (System.Random Wrapper)

Wraps System.Random to work with IRngSource interface.

```csharp
using Shizounu.Library.RandomSystem;

var systemRandom = new System.Random(12345);
var wrapper = new RngSystemRandom(systemRandom);

// Now works with RngUser
var user = new RngUser(0, "Test", wrapper);
```

### Custom Implementation

```csharp
public class MyCustomRng : IRngSource
{
    private uint _state;
    
    public uint Seed => _state;
    
    public void SetSeed(uint seed)
    {
        _state = seed;
    }
    
    public uint Next()
    {
        // Your algorithm here
        return _state++;
    }
    
    // Implement other methods...
}
```

---

## Distributions

Advanced random distributions and utilities.

### Gaussian (Normal) Distribution

```csharp
// Standard normal (mean=0, stdDev=1)
float value = RngDistributions.NextGaussian(rngUser);

// Custom mean and standard deviation
float value = RngDistributions.NextGaussian(rngUser, mean: 50, stdDev: 10);

// Use case: natural variation
float damage = RngDistributions.NextGaussian(rngUser, mean: 100, stdDev: 15);
```

### Exponential Distribution

```csharp
// Default lambda = 1
float value = RngDistributions.NextExponential(rngUser);

// Custom lambda
float value = RngDistributions.NextExponential(rngUser, lambda: 0.5f);

// Use case: spawn timing
float spawnDelay = RngDistributions.NextExponential(rngUser, lambda: 2f);
```

### Boolean with Probability

```csharp
// 50/50
bool coinFlip = RngDistributions.NextBool(rngUser);

// Custom probability
bool critHit = RngDistributions.NextBool(rngUser, probability: 0.15f);
```

### Percentage Roll

```csharp
// Returns 0-100
float roll = RngDistributions.PercentageRoll(rngUser);

if (roll < 75f)
{
    // 75% chance
}
```

### Dice Rolls

```csharp
// 1d20
int d20 = RngDistributions.DiceRoll(rngUser, count: 1, sides: 20);

// 3d6
int threeD6 = RngDistributions.DiceRoll(rngUser, count: 3, sides: 6);

// 2d10+5
int damage = RngDistributions.DiceRoll(rngUser, 2, 10) + 5;
```

### Collection Operations

```csharp
// Pick random element
var list = new List<string> { "sword", "shield", "potion" };
string item = RngDistributions.PickRandom(rngUser, list);

// Shuffle list in-place
RngDistributions.Shuffle(rngUser, list);
```

### Spatial Distributions

```csharp
// Random point on unit sphere surface
var (x, y, z) = RngDistributions.RandomPointOnSphere(rngUser);

// Random point in unit circle
var (x, y) = RngDistributions.RandomPointInCircle(rngUser);

// With radius
var (x, y) = RngDistributions.RandomPointInCircle(rngUser, radius: 5f);
```

---

## History & Snapshots

Track every random call and save/restore RNG state.

### RngHistory

Tracks all random generations.

```csharp
var history = context.History;

// Total calls
int totalCalls = history.EntryCount;

// Current position
int currentStep = history.CurrentStep;

// Access entries
foreach (var entry in history.Entries)
{
    Debug.Log($"User {entry.UserId}: {entry.Value} ({entry.Label})");
}
```

### History Entry

```csharp
public struct RngHistoryEntry
{
    int UserId;                    // Who generated it
    uint Value;                    // Generated value
    string Label;                  // Optional label
    uint StateBeforeGeneration;    // RNG state before
    uint StateAfterGeneration;     // RNG state after
    int GlobalStep;                // Timeline position
}
```

### RngSnapshot

Save complete RNG state at a point in time.

```csharp
// Create labeled snapshot
context.CreateSnapshot("turn_5_start");

// Create unnamed
context.CreateSnapshot();

// Restore from snapshot
context.RestoreSnapshot(0);  // Restore first snapshot

// Iterate snapshots
foreach (var snapshot in context.Snapshots)
{
    Debug.Log($"{snapshot.Label} at step {snapshot.HistoryStep}");
}

// Clear all
context.ClearSnapshots();
```

### Snapshot Properties

```csharp
DateTime timestamp = snapshot.Timestamp;
string label = snapshot.Label;
int historyStep = snapshot.HistoryStep;

// Get recorded user states
var userStates = snapshot.GetAllUserStates();
IEnumerable<int> userIds = snapshot.GetRecordedUserIds();
```

### Use Cases

**Save/Load System:**
```csharp
// On save
context.CreateSnapshot("save_point");
int saveSlot = context.Snapshots.Count - 1;
SaveToFile(saveSlot);

// On load
int saveSlot = LoadFromFile();
context.RestoreSnapshot(saveSlot);
```

**Branching Simulations:**
```csharp
// Try different strategies
context.CreateSnapshot("before_test");

SimulateStrategy1();
float score1 = EvaluateOutcome();

context.RestoreSnapshot(0);
SimulateStrategy2();
float score2 = EvaluateOutcome();

// Use best strategy
```

**Replay System:**
```csharp
// Record gameplay
while (playing)
{
    ProcessInput();
    context.CreateSnapshot($"frame_{frameCount}");
}

// Replay
for (int i = 0; i < framesToReplay; i++)
{
    context.RestoreSnapshot(i);
    Render();
}
```

---

## Random Tables

Weighted random selection system.

### Basic Usage

```csharp
var lootTable = new RandomTable<string>(rngUser);

// Add entries (equal weight)
lootTable.Add("common_sword");
lootTable.Add("common_shield");

// Add with custom weights
lootTable.Add("rare_gem", weight: 0.5f);    // Half as likely
lootTable.Add("legendary_item", weight: 0.1f);  // Very rare

// Select
string loot = lootTable.Select("loot_drop");
```

### Multiple Selections

```csharp
// Select 5 items with replacement
List<string> items = lootTable.SelectMultiple(5, "multi_loot");
```

### Dynamic Tables

```csharp
// Clear and rebuild
lootTable.Clear();

// Add based on player level
if (playerLevel > 10)
{
    lootTable.Add("magic_weapon", weight: 2.0f);
}

// Add based on difficulty
float rarityMultiplier = difficulty switch
{
    "easy" => 0.5f,
    "normal" => 1.0f,
    "hard" => 2.0f,
    _ => 1.0f
};

lootTable.Add("legendary", weight: 0.1f * rarityMultiplier);
```

### Use Cases

**Enemy Spawning:**
```csharp
var enemyTable = new RandomTable<EnemyType>(rngUser);
enemyTable.Add(EnemyType.Goblin, weight: 5f);
enemyTable.Add(EnemyType.Orc, weight: 3f);
enemyTable.Add(EnemyType.Dragon, weight: 0.5f);

EnemyType spawn = enemyTable.Select("enemy_spawn");
```

**Quest Rewards:**
```csharp
var rewardTable = new RandomTable<Reward>(rngUser);
rewardTable.Add(goldReward, weight: 10f);
rewardTable.Add(itemReward, weight: 5f);
rewardTable.Add(specialReward, weight: 1f);

Reward reward = rewardTable.Select("quest_reward");
```

---

## Quick RNG

Static helper for one-off operations without full context setup.

### Usage

```csharp
using Shizounu.Library.RandomSystem;

// Quick values (NOT reproducible!)
uint value = QuickRng.Next();
float f = QuickRng.NextFloat();
int i = QuickRng.NextInt(10);
int range = QuickRng.NextInt(5, 15);

// Reseed
QuickRng.Seed(12345);
```

### Warning

⚠️ **Not Reproducible**: QuickRng uses time-based seed and is NOT reproducible across runs. Use for:
- Prototype/testing
- Non-critical randomness
- When reproducibility doesn't matter

For production or debugging, **always use RngContext/RngUser**.

---

## Best Practices

### Use Explicit Seeds for Debugging

```csharp
// Good - reproducible
var context = new RngContext();
var user = context.GetOrCreateUser("Test", seed: 12345);

// Bad - changes every run
var user = context.GetOrCreateUser("Test");
```

### Label Important Calls

```csharp
// Good - easy to debug
float crit = rngUser.NextFloat("crit_check");
int damage = rngUser.NextInt(1, 20, "damage_roll");

// Okay - but harder to debug
float crit = rngUser.NextFloat();
```

### Separate RNG Users by Purpose

```csharp
// Good - independent streams
var combatRng = context.GetOrCreateUser("Combat");
var lootRng = context.GetOrCreateUser("Loot");
var worldGenRng = context.GetOrCreateUser("WorldGen");

// Bad - everything affects everything
var rng = context.GetOrCreateUser("Global");
```

### Disable History for Performance-Critical Code

```csharp
// Disable during bulk generation
context.SetHistoryRecording(false);

for (int i = 0; i < 10000; i++)
{
    GenerateChunk(rngUser);
}

// Re-enable for gameplay
context.SetHistoryRecording(true);
```

### Clone for Independent Branches

```csharp
var baseRng = new XorshiftRng(123);

// Each algorithm gets independent stream
var bsp = new BspBuilder(baseRng.Clone() as IRngSource);
var ca = new CellularAutomata(100, 100, rules, baseRng.Clone() as IRngSource);
```

### Use Distributions Over Raw Values

```csharp
// Good - natural variation
float damage = RngDistributions.NextGaussian(rng, mean: 100, stdDev: 15);

// Bad - uniform distribution (less natural)
float damage = rng.NextFloat() * 200;
```

### Snapshot Before Risky Operations

```csharp
// Before procedural generation
context.CreateSnapshot("before_level_gen");

if (!GenerateLevel())
{
    // Restore and try again
    context.RestoreSnapshot(0);
    GenerateLevel();
}
```

---

## Integration with Generation Algorithms

All generation algorithms support IRngSource:

```csharp
var context = new RngContext();
var genRng = context.GetOrCreateUser("Generation", seed: 42);

// Binary Space Partitioning
var bsp = new BspBuilder(genRng.Source);

// Cellular Automata
var ca = new CellularAutomata(100, 100, rules, genRng.Source);

// Graph Grammars
var engine = new GraphGrammarEngine<string>(graph, genRng.Source);

// Wave Function Collapse
var wfc = new WfcSolver<Tile>(width, height, tileSet, genRng.Source);

// All RNG tracked automatically
Debug.Log($"Total generation calls: {context.History.TotalCallCount}");
```

See [Generation Algorithms](Generation-Algorithms.md) for more details.

---

## Performance Considerations

### XorshiftRng vs System.Random

- **XorshiftRng**: Faster, guaranteed reproducible
- **System.Random**: Slower, platform-dependent behavior

### History Overhead

History recording has minimal overhead but uses memory:
- ~32 bytes per RNG call
- Disable for bulk operations (10,000+ calls)
- Clear history periodically if needed

### Snapshots

- Lightweight: ~8 bytes per user
- Fast to create/restore
- Unlimited snapshots supported

### Cloning

```csharp
// Cheap - just copies state
IRngSource clone = rng.Clone();
```

---

## Troubleshooting

### Different Results on Different Machines

**Problem**: Same seed gives different results  
**Solution**: Use XorshiftRng, never System.Random directly

### RNG Not Reproducible

**Problem**: Can't reproduce random sequence  
**Solution**: 
- Always use explicit seeds
- Ensure same call order
- Check all code paths use same RNG

### Performance Issues

**Problem**: RNG too slow  
**Solution**:
- Disable history recording
- Use XorshiftRng (not System.Random wrapper)
- Profile to ensure RNG is the bottleneck

### History Too Large

**Problem**: Memory usage growing  
**Solution**:
```csharp
// Clear periodically
context.ClearHistory();

// Or disable
context.SetHistoryRecording(false);
```

---

## API Quick Reference

### RngContext
```csharp
var context = new RngContext();
var user = context.GetOrCreateUser(name, seed);
context.CreateSnapshot(label);
context.RestoreSnapshot(index);
```

### RngUser
```csharp
uint v = user.Next(label);
float f = user.NextFloat(label);
int i = user.NextInt(max, label);
```

### IRngSource
```csharp
var rng = new XorshiftRng(seed);
uint v = rng.Next();
IRngSource clone = rng.Clone();
```

### Distributions
```csharp
float gaussian = RngDistributions.NextGaussian(user, mean, stdDev);
bool coinFlip = RngDistributions.NextBool(user, probability);
int dice = RngDistributions.DiceRoll(user, count, sides);
T item = RngDistributions.PickRandom(user, list);
```

### RandomTable
```csharp
var table = new RandomTable<T>(user);
table.Add(item, weight);
T selected = table.Select(label);
```

---

## Further Reading

- [Generation Algorithms](Generation-Algorithms.md) - Integration with procedural generation
- [Examples and Tutorials](Examples-and-Tutorials.md) - Working code samples
- [Best Practices](Best-Practices.md) - Design patterns
