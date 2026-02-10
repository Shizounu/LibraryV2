# Cellular Automata Map Generation

A flexible cellular automata implementation for generating 2D and 3D maps and dungeons in Unity. Supports multiple rule sets and neighborhood types for diverse generation patterns.

## Overview

Cellular automata are computational systems that evolve through discrete time steps based on local rules. Each cell's next state depends on its current state and the states of its neighbors. This system includes:

- **2D Generation**: `CellularAutomata` - for generating 2D maps
- **3D Generation**: `CellularAutomata3D` - for generating 3D dungeons and caves
- **Flexible Rules**: Support for B/S notation (e.g., "B3/S23")
- **Multiple Neighborhoods**: Moore (8-neighbor) and Von Neumann (4-neighbor)
- **Pre-built Presets**: Conway's Game of Life, Maze, HighLife, and Amoebas

## Quick Start

### Basic 2D Generation

```csharp
// Create rules using B/S notation
var rules = CellularAutomataRules.FromNotation("B3/S23");

// Create a cellular automata solver
var automata = new CellularAutomata(100, 100, rules, seed: 12345);

// Initialize with random cells (50% probability of being alive)
automata.RandomizeGrid(0.5f);

// Run simulation
automata.Simulate(5);

// Get the generated grid
bool[][] grid = automata.GetGrid();
```

### Basic 3D Generation

```csharp
// Create rules
var rules = CellularAutomataRules.FromNotation("B3/S23");

// Create 3D cellular automata
var automata3D = new CellularAutomata3D(50, 50, 50, rules, seed: 12345);

// Initialize and simulate
automata3D.RandomizeGrid(0.5f);
automata3D.Simulate(5);

// Get the 3D grid
bool[][][] grid = automata3D.GetGrid();
```

## Rule Notation

Rules are specified using **B/S notation**:
- **B** = Birth rules (neighbor counts that cause dead cells to become alive)
- **S** = Survival rules (neighbor counts that keep alive cells alive)

### Examples

- **B3/S23** (Conway's Game of Life): 
  - Birth: exactly 3 neighbors
  - Survive: 2 or 3 neighbors
  - Common for stable oscillating patterns

- **B3/S12345** (Maze):
  - Birth: exactly 3 neighbors
  - Survive: 1, 2, 3, 4, or 5 neighbors
  - Generates maze-like structures with corridors

- **B36/S23** (HighLife):
  - Birth: 3 or 6 neighbors
  - Survive: 2 or 3 neighbors
  - Creates replicating structures

- **B357/S1358** (Amoebas):
  - Birth: 3, 5, or 7 neighbors
  - Survive: 1, 3, 5, or 8 neighbors
  - Generates organic blob-like patterns

## Neighborhood Types

### Moore Neighborhood (8-neighbor)
Includes all 8 surrounding cells including diagonals. Standard for most cellular automata.

```
X X X
X O X
X X X
```

### Von Neumann Neighborhood (4-neighbor)
Includes only the 4 orthogonally adjacent cells (no diagonals).

```
  X
X O X
  X
```

## API Reference

### CellularAutomata (2D)

#### Constructor
```csharp
public CellularAutomata(int width, int height, CellularAutomataRules rules, int? seed = null)
```

#### Key Methods
- `RandomizeGrid(float fillProbability)` - Initialize with random cells
- `ClearGrid()` - Set all cells to dead
- `SetCell(int x, int y, bool alive)` - Set individual cell state
- `GetCell(int x, int y)` - Get individual cell state
- `Step()` - Perform one simulation step
- `Simulate(int steps)` - Perform multiple steps
- `GetGrid()` - Get a copy of the current grid
- `SetGrid(bool[][] grid)` - Load a grid from external source

#### Properties
- `Width` - Grid width
- `Height` - Grid height
- `Rules` - The rule set being used

### CellularAutomata3D

Same API as CellularAutomata, but for 3D grids with an additional Z dimension.

### CellularAutomataRules

#### Static Factory Methods
```csharp
public static CellularAutomataRules FromNotation(string notation, NeighborhoodType neighborhoodType = NeighborhoodType.Moore)
```

#### Preset Rules
```csharp
CellularAutomataRules.ConwaysGameOfLife  // B3/S23
CellularAutomataRules.Maze               // B3/S12345
CellularAutomataRules.HighLife           // B36/S23
CellularAutomataRules.Amoebas            // B357/S1358
```

#### Instance Methods
- `ShouldBeBorn(int aliveNeighbors)` - Check if cell should be born
- `ShouldSurvive(int aliveNeighbors)` - Check if cell should survive

## Practical Applications

### Cave Generation
Use rules like B3/S23 or B3/S12345 with high initial fill probability (60-70%) to generate natural-looking caves and tunnels.

```csharp
var rules = CellularAutomataRules.Maze;
var automata = new CellularAutomata(100, 100, rules, 12345);
automata.RandomizeGrid(0.65f);
automata.Simulate(3);
```

### Maze Generation
Maze rules create connected corridors with organic pathways.

```csharp
var rules = CellularAutomataRules.Maze;
var automata = new CellularAutomata(100, 100, rules, 12345);
automata.RandomizeGrid(0.5f);
automata.Simulate(5);
```

### Organic Terrain
Use different rules and neighborhood types to generate varied terrain:

```csharp
var rules = CellularAutomataRules.Amoebas;
var automata = new CellularAutomata(128, 128, rules);
automata.RandomizeGrid(0.45f);
automata.Simulate(8);
```

### 3D Dungeon Generation
Generate 3D dungeons and cave systems using the 3D variant:

```csharp
var rules = CellularAutomataRules.FromNotation("B3/S23");
var automata = new CellularAutomata3D(64, 64, 64, rules);
automata.RandomizeGrid(0.4f);
automata.Simulate(4);
```

## Example Behaviors

### CellularAutomataGeneratorBehaviour
Attached to a GameObject, this behaviour provides a quick way to generate 2D maps in the editor with serialized parameters:

- **Grid Width/Height**: Size of the generated map
- **Rules**: B/S notation string
- **Initial Fill Probability**: Percentage of cells initially alive
- **Simulation Steps**: Number of iterations to run
- **Random Seed**: For consistent generation

Call `GenerateMap()` to regenerate, or `Step()`/`Simulate(n)` to continue evolution.

### CellularAutomataGenerator3DBehaviour
Similar to the 2D version but generates 3D grids for dungeon/cave systems.

## Performance Considerations

- **Grid Size**: Larger grids require more computation per step. For real-time generation, start with 64x64 or 64x64x64.
- **Simulation Steps**: Each step requires checking all cells. More steps = more computation but better results.
- **Neighborhood Type**: Moore (8-neighbor) is slightly slower than Von Neumann (4-neighbor) but often produces better results.

## Tips for Best Results

1. **Experiment with fill probability**: 
   - 30-40%: More open, sparse environments
   - 50%: Balanced
   - 60-70%: Denser, more cave-like

2. **Use multiple rule iterations**: Run different rules sequentially:
   ```csharp
   automata.RandomizeGrid(0.5f);
   automata.Simulate(3);  // Initial smoothing
   ```

3. **Post-process**: After generation, apply flood-fill to clean up isolated regions

4. **Blend rules**: Generate with one rule, then manually tweak parameters and re-run for hybrid results

## Common Issues

**Generation looks random/too sparse**: Increase initial fill probability and simulation steps.

**Generation too solid**: Decrease initial fill probability or use different rules with higher birth thresholds.

**Performance slow**: Reduce grid size or simulation steps. Consider Von Neumann neighborhood instead of Moore.

## Further Reading

- [Wikipedia: Cellular Automaton](https://en.wikipedia.org/wiki/Cellular_automaton)
- [Conway's Game of Life](https://en.wikipedia.org/wiki/Conway%27s_Game_of_Life)
- [Cellular Automata for Procedural Generation](https://www.roguebasin.com/index.php/Cellular_Automata_Method_for_Generating_Random_Cave-Like_Levels)
