# Generation Algorithms

Comprehensive guide to procedural generation algorithms including Binary Space Partitioning, Cellular Automata, Graph Grammars, and Wave Function Collapse.

## Table of Contents

1. [Overview](#overview)
2. [Random System Integration](#random-system-integration)
3. [Binary Space Partitioning (BSP)](#binary-space-partitioning-bsp)
4. [Cellular Automata](#cellular-automata)
5. [Graph Grammars](#graph-grammars)
6. [Wave Function Collapse](#wave-function-collapse)
7. [Shared Utilities](#shared-utilities)
8. [Visualization Tools](#visualization-tools)

---

## Overview

The Generation Algorithms system provides battle-tested procedural generation tools for creating dungeons, maps, levels, and complex structures. All algorithms integrate with the Random System for reproducibility and feature flexible configuration options.

### Key Features

- **Reproducible Generation**: Same seeds always produce same results
- **Unified RNG System**: All algorithms use `IRngSource` interface
- **Performance Optimized**: Fast implementations suitable for runtime generation
- **Well-Documented**: Comprehensive examples and API references
- **Flexible Configuration**: Control every aspect of generation

### Namespace

```csharp
using Shizounu.Library.GenerationAlgorithms.BSP;
using Shizounu.Library.GenerationAlgorithms.CellularAutomata;
using Shizounu.Library.GenerationAlgorithms.GraphGrammars;
using Shizounu.Library.GenerationAlgorithms.WFC;
using Shizounu.Library.GenerationAlgorithms.Shared;
```

---

## Random System Integration

All generation algorithms implement the `IRngProvider` interface and integrate with the Random System for reproducibility.

### Basic Usage

```csharp
using Shizounu.Library.GenerationAlgorithms.Shared;

// Simple creation with seed
var rng = GenerationRng.Create(12345);
var bsp = new BspBuilder(rng);

// Time-based random seed
var rng = GenerationRng.Create(-1);
```

### Advanced: RNG Context Integration

```csharp
using Shizounu.Library.RandomSystem;

// Create RNG context for centralized management
var context = new RngContext();
var generatorRng = context.GetOrCreateUser("level_generator");

// Use with algorithms
var bsp = new BspBuilder(generatorRng.Source);
var ca = new CellularAutomata(100, 100, rules, generatorRng.Source);

// RNG history automatically tracked
Debug.Log($"Generated with {context.History.TotalCallCount} RNG calls");
```

### IRngProvider Interface

All algorithms implement this interface for consistent RNG access:

```csharp
if (algorithm is IRngProvider provider)
{
    var seed = provider.RngSource.Seed;
    var clone = provider.RngSource.Clone();
}
```

**Implemented by:**
- BspBuilder / BspBuilder3D
- CellularAutomata / CellularAutomata3D
- WfcSolver / WfcSolver3D
- GraphGrammarEngine

---

## Binary Space Partitioning (BSP)

Binary Space Partitioning recursively divides space into two half-spaces using axis-aligned planes, creating a hierarchical spatial data structure.

### Features

- **2D & 3D Support**: Partition rectangles or volumes
- **Multiple Split Strategies**: Alternating, Longest Side, Always X/Y/Z
- **Split Positioning**: Middle, Random, or Golden Ratio
- **Spatial Queries**: Fast point lookup
- **Tree Traversal**: Get all leaves, depth, node count

### Quick Start (2D)

```csharp
using Shizounu.Library.GenerationAlgorithms.BSP;
using Shizounu.Library.GenerationAlgorithms.Shared;
using UnityEngine;

// Create builder
var bounds = new Rect(0, 0, 100, 100);
var builder = new BspBuilder(
    axisSelection: BspBuilder.AxisSelection.Alternating,
    splitPosition: SplitPositionStrategy.Middle,
    minNodeSize: 10
);

// Build the tree
BspNode root = builder.Generate(bounds);

// Query which leaf contains a point
Vector2 testPoint = new Vector2(45, 67);
BspNode leaf = root.FindLeafAt(testPoint);

// Get all leaves
var leaves = new List<BspNode>();
root.GetAllLeaves(leaves);
```

### Quick Start (3D)

```csharp
// Create a 3D builder
var bounds = new Bounds(Vector3.one * 50, Vector3.one * 100);
var builder = new BspBuilder3D(
    axisSelection: BspBuilder3D.AxisSelection.Cycling,
    splitPosition: SplitPositionStrategy.Middle,
    minNodeSize: 5f
);

// Build and query
BspNode3D root = builder.Generate(bounds);
Vector3 testPoint = new Vector3(45, 67, 30);
BspNode3D leaf = root.FindLeafAt(testPoint);
```

### BspBuilder Configuration

**Axis Selection:**
- `Alternating`: X at even depths, Y at odd (balanced)
- `LongestSide`: Always split longest dimension
- `AlwaysX` / `AlwaysY`: Always split same axis

**Split Position:**
- `Middle`: Center of space (balanced tree)
- `Random`: Random position (varied layouts)
- `GoldenRatio`: 1/φ ratio split (aesthetic)

**Parameters:**
- `minNodeSize`: Minimum size before stopping subdivision
- `seed`: Random seed for reproducibility

### Use Cases

**Dungeon Generation:**
```csharp
// Divide space into rooms
var dungeon = new Rect(0, 0, 1000, 1000);
var builder = new BspBuilder(
    BspBuilder.AxisSelection.Alternating,
    SplitPositionStrategy.Random,
    minNodeSize: 50
);

BspNode root = builder.Generate(dungeon);
var rooms = new List<BspNode>();
root.GetAllLeaves(rooms);

foreach (var room in rooms)
{
    CreateRoom(room.Bounds);
}
```

**Spatial Indexing:**
```csharp
// Partition space for fast collision queries
var world = new Bounds(Vector3.zero, new Vector3(1000, 1000, 1000));
var builder = new BspBuilder3D(
    BspBuilder3D.AxisSelection.LongestSide,
    SplitPositionStrategy.Middle,
    minNodeSize: 50f
);

BspNode3D root = builder.Generate(world);

foreach (var obj in objects)
{
    var leaf = root.FindLeafAt(obj.position);
    leaf.UserData = obj;
}
```

### Performance

- **Build Time**: O(n) where n = number of leaf nodes
- **Point Query**: O(log n) average, O(n) worst case
- **Memory**: O(n) for n leaf nodes

---

## Cellular Automata

Cellular automata generate maps and dungeons through iterative local rules. Each cell's next state depends on its neighbors.

### Features

- **2D & 3D Support**: Generate 2D maps or 3D caves
- **Flexible Rules**: B/S notation (e.g., "B3/S23")
- **Multiple Neighborhoods**: Moore (8) and Von Neumann (4)
- **Pre-built Presets**: Conway's Life, Maze, HighLife, Amoebas

### Quick Start

```csharp
using Shizounu.Library.GenerationAlgorithms.CellularAutomata;

// Create rules
var rules = CellularAutomataRules.FromNotation("B3/S23");

// Create automata
var automata = new CellularAutomata(100, 100, rules, seed: 12345);

// Initialize and generate
automata.RandomizeGrid(0.5f);  // 50% alive
automata.Generate(5);           // 5 steps

// Get result
bool[][] grid = automata.GetGrid();
```

### Rule Notation

Rules use **B/S notation**:
- **B** = Birth rules (neighbor counts for dead → alive)
- **S** = Survival rules (neighbor counts to stay alive)

**Common Rules:**
- **B3/S23** (Conway's Game of Life): Stable oscillating patterns
- **B3/S12345** (Maze): Generates maze-like corridors
- **B36/S23** (HighLife): Creates replicating structures
- **B357/S1358** (Amoebas): Organic blob-like patterns

### Neighborhood Types

**Moore Neighborhood (8-neighbor):**
```
X X X
X O X
X X X
```

**Von Neumann Neighborhood (4-neighbor):**
```
  X
X O X
  X
```

### Preset Rules

```csharp
// Use presets
var rules = CellularAutomataRules.ConwaysGameOfLife;
var rules = CellularAutomataRules.Maze;
var rules = CellularAutomataRules.HighLife;
var rules = CellularAutomataRules.Amoebas;

// Or create custom
var rules = CellularAutomataRules.FromNotation(
    "B3/S23", 
    NeighborhoodType.Moore
);
```

### API Reference

**CellularAutomata:**
- `RandomizeGrid(float fillProbability)` - Random initialization
- `ClearGrid()` - Set all cells to dead
- `SetCell(int x, int y, bool alive)` - Set cell state
- `GetCell(int x, int y)` - Get cell state
- `Step()` - Perform one simulation step
- `Generate(int steps)` - Perform multiple steps
- `GetGrid()` - Get copy of current grid
- `SetGrid(bool[][] grid)` - Load external grid

### Practical Applications

**Cave Generation:**
```csharp
var rules = CellularAutomataRules.Maze;
var automata = new CellularAutomata(100, 100, rules, 12345);
automata.RandomizeGrid(0.65f);  // Dense fill
automata.Generate(3);           // Smooth
```

**Maze Generation:**
```csharp
var rules = CellularAutomataRules.Maze;
var automata = new CellularAutomata(100, 100, rules);
automata.RandomizeGrid(0.5f);
automata.Generate(5);
```

**3D Dungeons:**
```csharp
var rules = CellularAutomataRules.FromNotation("B3/S23");
var automata = new CellularAutomata3D(64, 64, 64, rules);
automata.RandomizeGrid(0.4f);
automata.Generate(4);
```

### Performance Tips

- Grid size: 64x64 or 64x64x64 for real-time
- Each step checks all cells
- Moore neighborhood slightly slower than Von Neumann
- Consider flood-fill post-processing for isolated regions

---

## Graph Grammars

Graph grammars provide powerful procedural generation using production rules to transform graphs. Define patterns and their replacements to grow complex structures.

### Features

- **Pattern Matching**: Subgraph isomorphism
- **Production Rules**: Pattern → Replacement
- **Preserved Nodes**: Keep and modify nodes
- **Predicates**: Conditional rule application
- **Multiple Strategies**: FirstMatch, Random, Priority-based
- **Real-time Visualization**: Unity Scene view integration

### Quick Start

```csharp
using Shizounu.Library.GenerationAlgorithms.GraphGrammars;

// Create initial graph
var initialGraph = new Graph<string>();
var startNode = new GraphNode<string>("Start");
startNode.SetAttribute("type", "root");
initialGraph.AddNode(startNode);

// Create production rule: Start → A - B
var pattern = new Graph<string>();
var patStart = new GraphNode<string>("Start");
patStart.SetAttribute("type", "root");
pattern.AddNode(patStart);

var replacement = new Graph<string>();
var repA = new GraphNode<string>("A");
var repB = new GraphNode<string>("B");
replacement.AddNode(repA);
replacement.AddNode(repB);
replacement.AddEdge(new GraphEdge<string>(repA, repB, "connects"));

var rule = new GraphProductionRule<string>(pattern, replacement, "Expand Start");
rule.Priority = 10;

// Generate
var engine = new GraphGrammarEngine<string>(initialGraph, seed: 12345);
engine.AddRule(rule);
engine.SelectionStrategy = RuleSelectionStrategy.FirstMatch;

var result = engine.Generate(steps: 5);
```

### Rule Selection Strategies

```csharp
// First matching rule (deterministic)
engine.SelectionStrategy = RuleSelectionStrategy.FirstMatch;

// Random selection (varied output)
engine.SelectionStrategy = RuleSelectionStrategy.Random;

// Highest priority wins
engine.SelectionStrategy = RuleSelectionStrategy.HighestPriority;

// Maximize/minimize growth
engine.SelectionStrategy = RuleSelectionStrategy.MaximizeGrowth;
engine.SelectionStrategy = RuleSelectionStrategy.MinimizeGrowth;
```

### Advanced Features

**Preserved Nodes:**
```csharp
var rule = new GraphProductionRule<string>(pattern, replacement);
// Pattern node updated with replacement data instead of deleted
rule.PreserveNode(patternNode, replacementNode);
```

**Rule Predicates:**
```csharp
rule.Predicate = (graph, match) => 
{
    // Only apply if graph has fewer than 50 nodes
    return graph.NodeCount < 50;
};
```

**Custom Callbacks:**
```csharp
rule.OnApply = (graph, match) => 
{
    Debug.Log($"Rule applied! Graph now has {graph.NodeCount} nodes");
};
```

**Fluent Rule Building:**
```csharp
var rule = new RuleBuilder<string>()
    .WithName("Branch Rule")
    .WithPriority(5)
    .WithPattern(CreatePatternGraph())
    .WithReplacement(CreateReplacementGraph())
    .Preserve(patternNodeA, replacementNodeA)
    .WithPredicate((g, m) => g.NodeCount < 100)
    .Build();
```

### Use Cases

- **Dungeon Generation**: Rooms as nodes, connections as edges
- **Story Generation**: Story beats and transitions
- **L-System-like**: Branching structures with graph topology
- **City Layout**: City blocks and road networks
- **Dialogue Trees**: Conversational structures

### Pattern Matching

```csharp
// Strict: all attributes must match exactly
engine.StrictAttributeMatching = true;

// Non-strict: pattern attributes must exist in target
engine.StrictAttributeMatching = false;
```

### Performance Considerations

- Pattern matching is NP-complete
- Keep patterns small (2-5 nodes)
- Use attributes to narrow matches early
- Set `MaxIterations` limits
- Use predicates to fail fast

### Common Patterns

**Simple Expansion (A → A B):**
```csharp
var pattern = new Graph<T>();
var a = new GraphNode<T>(value);
pattern.AddNode(a);

var replacement = new Graph<T>();
var newA = new GraphNode<T>(value);
var b = new GraphNode<T>(otherValue);
replacement.AddNode(newA);
replacement.AddNode(b);
replacement.AddEdge(new GraphEdge<T>(newA, b));

var rule = new GraphProductionRule<T>(pattern, replacement);
rule.PreserveNode(a, newA);
```

**Branching (A → B ← A → C):**
```csharp
var replacement = new Graph<T>();
var center = new GraphNode<T>(value);
var left = new GraphNode<T>(leftValue);
var right = new GraphNode<T>(rightValue);
replacement.AddNode(center);
replacement.AddNode(left);
replacement.AddNode(right);
replacement.AddEdge(new GraphEdge<T>(center, left, "left"));
replacement.AddEdge(new GraphEdge<T>(center, right, "right"));

rule.PreserveNode(patternNode, center);
```

### Result Codes

- `Success`: Generation completed requested iterations
- `NoRulesApplicable`: No rules match current graph
- `MaxIterationsReached`: Hit iteration limit
- `InvalidState`: No rules defined or invalid configuration

---

## Wave Function Collapse

Wave Function Collapse generates tilemap patterns by propagating constraints until all tiles are determined.

### Features

- **2D & 3D Support**: Generate tilemaps or voxel structures
- **Constraint Propagation**: Tiles enforce neighbor compatibility
- **Backtracking Support**: Handle contradictions
- **Custom Tile Sets**: Define your own tiles and rules

### Quick Start

```csharp
using Shizounu.Library.GenerationAlgorithms.WFC;

// Define tile set with adjacency rules
var tileSet = new WfcTileSet<MyTile>();
tileSet.AddTile(grassTile);
tileSet.AddTile(waterTile);
tileSet.AddTile(sandTile);

// Define which tiles can be adjacent
tileSet.SetCompatible(grassTile, sandTile, Direction2D.Right);
tileSet.SetCompatible(sandTile, waterTile, Direction2D.Right);

// Create solver and generate
var rng = GenerationRng.Create(12345);
var solver = new WfcSolver<MyTile>(width, height, tileSet, rng);
bool success = solver.Generate();

if (success)
{
    MyTile[][] result = solver.GetGrid();
}
```

### 3D Generation

```csharp
var solver = new WfcSolver3D<MyTile>(width, height, depth, tileSet, rng);
bool success = solver.Generate();
MyTile[][][] result = solver.GetGrid();
```

### Tile Set Configuration

```csharp
// Add tiles
tileSet.AddTile(tile);

// Set directional compatibility
tileSet.SetCompatible(tileA, tileB, Direction2D.Up);
tileSet.SetCompatible(tileA, tileB, Direction2D.Right);

// Set tile weights (frequency)
tileSet.SetWeight(tile, 2.0f);  // Twice as common
```

### Handling Failures

```csharp
if (!solver.Generate())
{
    // Generation failed (contradiction)
    // Try different seed or adjust tile constraints
}
```

---

## Shared Utilities

Common utilities used across all generation algorithms.

### GenerationRng

Static utility for creating RNG sources:

```csharp
// From seed
var rng = GenerationRng.Create(12345);

// Time-based (-1 = auto)
var rng = GenerationRng.Create(-1);

// From nullable seed
int? maybeSeed = GetSeed();
var rng = GenerationRng.Create(maybeSeed);

// Use existing or create new
var rng = GenerationRng.CreateOrUse(existingRng, 123);
```

### SplitPositionStrategy

Enum for BSP split positioning:
- `Middle`: Split at center
- `Random`: Split at random position
- `GoldenRatio`: Split using golden ratio (φ = 2.618)

### BspUtilities

Common BSP calculations:

```csharp
// Calculate split position
float randomValue = rng.NextFloat();
float splitX = BspUtilities.CalculateSplitPosition(
    bounds.xMin, 
    bounds.width, 
    SplitPositionStrategy.Random, 
    randomValue
);

// Golden ratio constant
float phi = BspUtilities.GoldenRatio;  // 2.618
```

### IRngProvider

Interface implemented by all algorithms:

```csharp
public interface IRngProvider
{
    IRngSource RngSource { get; }
}

// Usage
if (algorithm is IRngProvider provider)
{
    var seed = provider.RngSource.Seed;
    var clone = provider.RngSource.Clone();
}
```

---

## Visualization Tools

Graph Grammar visualization system for Unity with interactive demos.

### Features

- **Real-time Visualization**: View graphs in Scene view
- **Multiple Layouts**: Grid, Circular, Force-Directed, Hierarchical
- **Interactive Demos**: Step-by-step generation
- **Color-coded Nodes**: Visual distinction by type
- **Edge Arrows**: Directional edges with labels

### Quick Setup

1. Create GameObject in scene
2. Add `GraphGrammarDemo` component
3. Choose demo type (Basic, Branching, Dungeon)
4. Press Play

### Controls

- **SPACE**: Generate one step
- **G**: Generate all steps (animated)
- **R**: Reset to initial state

### GraphGrammarVisualizer

Handles visual display of graphs.

**Settings:**
- `nodeSpacing`: Distance between nodes
- `nodeSize`: Sphere radius for nodes
- `edgeThickness`: Line width for edges
- `layoutType`: Choose layout algorithm

**Layout Algorithms:**

1. **Grid Layout**: Simple grid arrangement (fast)
2. **Circular Layout**: Nodes in circle (good for cycles)
3. **Force-Directed Layout** ⭐: Natural-looking (recommended)
4. **Hierarchical Layout**: Tree-like (good for DAGs)

### Layout Comparison

| Layout | Speed | Best For | Quality |
|--------|-------|----------|---------|
| Grid | ⚡⚡⚡ | Quick preview | ⭐⭐ |
| Circular | ⚡⚡⚡ | Cycles/rings | ⭐⭐⭐ |
| Force-Directed | ⚡⚡ | General graphs | ⭐⭐⭐⭐⭐ |
| Hierarchical | ⚡⚡⚡ | Trees/DAGs | ⭐⭐⭐⭐ |

### Using in Your Code

```csharp
using Shizounu.Library.GenerationAlgorithms.GraphGrammars.Visualization;

public class MyGenerator : MonoBehaviour
{
    [SerializeField] private GraphGrammarVisualizer visualizer;
    
    private void GenerateAndVisualize()
    {
        // Create and generate graph
        var graph = new Graph<string>();
        var engine = new GraphGrammarEngine<string>(graph, seed: 12345);
        // ... add rules ...
        engine.Generate(10);
        
        // Visualize
        visualizer.SetGraph(engine.Graph);
        visualizer.RefreshLayout();
    }
}
```

### Demo Types

**Basic Expansion**: Simple linear growth (Start → A → B)
**Branching** ⭐: Tree-like fractal growth (L-System style)
**Dungeon** 🏰: Procedural room layout with corridors

### Customization

**Custom Node Colors:**
Modify `DrawGraph()` in `GraphGrammarVisualizer.cs`

**Custom Layout:**
Add new case in `CalculateLayout()` and implement your algorithm

**Editor Features:**
Labels show in Scene view (editor only). Works at runtime without labels.

### Performance Tips

- Force-Directed: 50 iterations, O(n²), slow for 100+ nodes
- Hierarchical: O(n+e), good for any size
- Circular/Grid: O(n), instant, best for large graphs

### Scene Setup

```
Scene Hierarchy:
├── Main Camera
├── Directional Light
└── Graph Grammar Demo
    └── GraphGrammarVisualizer (auto-added)
```

**Recommended Settings:**

For tree structures (Branching):
```
Layout: Force-Directed
Use 3D Layout: ✓
Node Spacing: 2.5
```

For networks (Dungeon):
```
Layout: Hierarchical
Use 3D Layout: ✗
Node Spacing: 3.0
Show Edge Labels: ✓
```

---

## Migration Guide

### From Integer Seeds

All constructors still accept integer seeds for backwards compatibility:

```csharp
// Old way (still works)
var bsp = new BspBuilder(seed: 12345);
var ca = new CellularAutomata(100, 100, rules, seed: 12345);

// New way (recommended)
var rng = GenerationRng.Create(12345);
var bsp = new BspBuilder(rng);
var ca = new CellularAutomata(100, 100, rules, rng);
```

### Using Random System Context

For advanced features (history, snapshots):

```csharp
var context = new RngContext();
var generatorRng = context.GetOrCreateUser("level_gen");

var bsp = new BspBuilder(generatorRng.Source);
var ca = new CellularAutomata(100, 100, rules, generatorRng.Source);

// Full RNG tracking available
Debug.Log($"Total calls: {context.History.TotalCallCount}");
```

---

## Best Practices

### Reproducibility

Always use explicit seeds for debugging:

```csharp
// Good - reproducible
var rng = GenerationRng.Create(12345);

// Bad - changes every run
var rng = GenerationRng.Create(-1);
```

### Performance

- Keep graph grammar patterns small (2-5 nodes)
- Use simpler CA neighborhoods (Von Neumann vs Moore)
- Set `MaxIterations` limits
- Cache BSP trees for spatial queries

### Code Organization

```csharp
public class LevelGenerator
{
    private IRngSource _rng;
    
    public void GenerateLevel(int seed)
    {
        _rng = GenerationRng.Create(seed);
        
        // All algorithms share same RNG
        var bsp = new BspBuilder(_rng.Clone() as IRngSource);
        var ca = new CellularAutomata(100, 100, rules, _rng.Clone() as IRngSource);
    }
}
```

### Sharing RNG

Clone RNG for independent streams:

```csharp
var baseRng = GenerationRng.Create(123);

// Each algorithm independent but seeded
var bsp = new BspBuilder(baseRng.Clone() as IRngSource);
var ca = new CellularAutomata(w, h, rules, baseRng.Clone() as IRngSource);
```

---

## Troubleshooting

### Generation Not Reproducible

**Problem**: Same seed gives different results  
**Solution**: Ensure all algorithms use same RNG source, not System.Random

### Performance Issues

**Problem**: Generation too slow  
**Solution**: 
- Reduce grid sizes
- Use simpler neighborhoods (Von Neumann)
- Simplify graph grammar patterns
- Set iteration limits

### WFC Fails Frequently

**Problem**: Generate() returns false  
**Solution**: 
- Review tile compatibility rules
- Add more compatible tile pairs
- Try different seeds
- Reduce grid size

### Graph Grammar Rules Don't Apply

**Problem**: NoRulesApplicable returned immediately  
**Solution**:
- Check pattern matches initial graph
- Verify attribute matching (strict vs non-strict)
- Review predicates (may be failing)
- Debug pattern with smaller test graph

---

## Examples

Complete working examples available in:
- `Binary Space Partioning/` - BSP examples
- `Cellular Automata/` - CA examples  
- `Graph Grammars/Examples/` - Grammar examples
- `Graph Grammars/Visualization/` - Interactive demos
- `Wave Function Collapse/Example/` - WFC examples

---

## API Quick Reference

### BSP
```csharp
var builder = new BspBuilder(axisSelection, splitPosition, minNodeSize, rng);
BspNode root = builder.Generate(bounds);
BspNode leaf = root.FindLeafAt(point);
```

### Cellular Automata
```csharp
var automata = new CellularAutomata(width, height, rules, rng);
automata.RandomizeGrid(fillProbability);
automata.Generate(steps);
bool[][] grid = automata.GetGrid();
```

### Graph Grammars
```csharp
var engine = new GraphGrammarEngine<T>(initialGraph, rng);
engine.AddRule(rule);
engine.SelectionStrategy = RuleSelectionStrategy.Random;
var result = engine.Generate(steps);
```

### WFC
```csharp
var solver = new WfcSolver<T>(width, height, tileSet, rng);
bool success = solver.Generate();
T[][] grid = solver.GetGrid();
```

---

## Further Reading

- [Random System](Random-System.md) - RNG documentation
- [Examples and Tutorials](Examples-and-Tutorials.md) - Code samples
- [Best Practices](Best-Practices.md) - Design patterns
