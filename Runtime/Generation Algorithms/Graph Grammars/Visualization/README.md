# Graph Grammar Visualization

Unity visualization system for Graph Grammars with interactive demos and multiple layout algorithms.

## Features

✅ **Real-time Visualization** - View graphs in Scene view with Gizmos  
✅ **Multiple Layouts** - Grid, Circular, Force-Directed, Hierarchical  
✅ **Interactive Demos** - Step-by-step generation with keyboard controls  
✅ **Color-coded Nodes** - Visual distinction for different node types  
✅ **Edge Arrows** - Directional edges with labels  
✅ **Live Updates** - Watch generation happen in real-time  

## Quick Setup

1. **Create a new GameObject** in your scene
2. **Add the `GraphGrammarDemo` component**
3. **Configure the demo**:
   - Choose demo type (Basic Expansion, Branching, or Dungeon)
   - Set seed for reproducibility
   - Adjust step delay and max iterations
4. **Press Play** and watch the generation!

## Controls

| Key | Action |
|-----|--------|
| **SPACE** | Generate one step |
| **G** | Generate all steps (animated) |
| **R** | Reset to initial state |

## Components

### GraphGrammarVisualizer

Handles the visual display of graphs.

**Settings:**
- **Node Spacing**: Distance between nodes in layout
- **Node Size**: Sphere radius for nodes
- **Edge Thickness**: Line width for edges
- **Colors**: Customize node and edge colors
- **Layout Type**: Choose layout algorithm

**Layout Algorithms:**

1. **Grid Layout**
   - Simple grid arrangement
   - Good for small graphs
   - Fast and predictable

2. **Circular Layout**
   - Nodes arranged in a circle
   - Good for cyclic structures
   - Clear edge visibility

3. **Force-Directed Layout** ⭐
   - Spring-embedder algorithm
   - Natural-looking layouts
   - Best for general use
   - Automatically separates nodes

4. **Hierarchical Layout**
   - Tree-like arrangement
   - Shows parent-child relationships
   - Good for directed acyclic graphs

### GraphGrammarDemo

Interactive demo showcasing different grammar types.

**Demo Types:**

1. **Basic Expansion**
   - Simple linear growth
   - Start → A → B

2. **Branching** ⭐
   - Tree-like structures
   - L-System style growth
   - Creates fractal-like patterns

3. **Dungeon**
   - Room and corridor generation
   - Entrance, normal, and treasure rooms
   - Demonstrates complex rules with attributes

## Using in Your Own Scripts

```csharp
using Shizounu.Library.GenerationAlgorithms.GraphGrammars.Visualization;

public class MyGenerator : MonoBehaviour
{
    [SerializeField] private GraphGrammarVisualizer visualizer;
    
    private void GenerateAndVisualize()
    {
        // Create your graph
        var graph = new Graph<string>();
        // ... add nodes and edges ...
        
        // Create engine and rules
        var engine = new GraphGrammarEngine<string>(graph, seed: 12345);
        // ... add rules ...
        
        // Generate
        engine.Generate(iterations: 10);
        
        // Visualize
        visualizer.SetGraph(engine.Graph);
        visualizer.RefreshLayout();
    }
}
```

## Customization

### Custom Node Colors

The visualizer automatically colors nodes based on attributes:

- **Normal nodes**: Cyan
- **Special nodes** (entrance, treasure, boss): Yellow
- **Corridors**: Gray

To customize, modify the `DrawGraph()` method in `GraphGrammarVisualizer.cs`.

### Custom Layout Algorithm

Implement your own layout by adding a new case in `CalculateLayout()`:

```csharp
case LayoutType.MyCustomLayout:
    CalculateMyCustomLayout();
    break;
```

Then implement your layout logic:

```csharp
private void CalculateMyCustomLayout()
{
    foreach (var node in Graph.Nodes)
    {
        // Calculate position for node
        Vector3 pos = CalculatePositionFor(node);
        _nodePositions[node.Id] = pos;
    }
}
```

### Editor-Only Features

Some features require Unity Editor (like text labels in scene view). The visualizer gracefully degrades in builds:

- Labels only show in editor (via `#if UNITY_EDITOR`)
- Gizmos work in both editor and game view
- All core functionality works at runtime

## Performance Considerations

**Force-Directed Layout:**
- Runs 50 iterations per update
- O(n²) complexity for repulsion
- May be slow for graphs with 100+ nodes
- Consider caching or using simpler layouts for large graphs

**Hierarchical Layout:**
- BFS traversal: O(n + e)
- Good for any graph size
- Best for tree-like structures

**Circular/Grid Layout:**
- O(n) complexity
- Instant calculation
- Best for large graphs

## Tips

1. **Start Small**: Test with small graphs (5-10 nodes) first
2. **Use Hierarchical Layout**: For directed graphs like dungeons
3. **Use Force-Directed Layout**: For general graphs with natural clustering
4. **Adjust Node Spacing**: Increase for larger graphs
5. **Slow Down Step Delay**: To better observe generation process
6. **Use Different Seeds**: Get varied output from same rules

## Example Scenarios

### Visualizing L-System Growth

```csharp
var demo = GetComponent<GraphGrammarDemo>();
demo.demoType = GraphGrammarDemo.DemoType.Branching;
demo.maxIterations = 5; // Keep it small for visibility
demo.stepDelay = 1f; // Slow for observation
demo.StartGeneration();

// Visualizer settings
visualizer.layoutType = GraphGrammarVisualizer.LayoutType.ForceDirected;
visualizer.use3DLayout = true; // 3D tree!
```

### Procedural Dungeon Preview

```csharp
var demo = GetComponent<GraphGrammarDemo>();
demo.demoType = GraphGrammarDemo.DemoType.Dungeon;
demo.maxIterations = 15;
demo.StartGeneration();

// Visualizer settings
visualizer.layoutType = GraphGrammarVisualizer.LayoutType.Hierarchical;
visualizer.showEdgeLabels = true; // See connections
```

## Troubleshooting

**Labels not showing:**
- Labels only appear in Scene view while in editor
- Make sure "showLabels" is enabled on the visualizer

**Layout looks cramped:**
- Increase `nodeSpacing` on the visualizer
- Try a different layout algorithm

**Generation not starting:**
- Check that `autoStart` is enabled on demo component
- Ensure seed is set (non-zero)
- Verify rules are properly configured

**Nodes overlapping:**
- Use Force-Directed layout for automatic spacing
- Increase `nodeSpacing`
- Increase force-directed iterations (edit code)

## Integration with RNG System

The demo uses the reproducible RNG system:

```csharp
var context = new RngContext();
var rngUser = context.GetOrCreateUser("GraphGrammar", seed: 42);
var random = new RngSystemRandom(rngUser);
var engine = new GraphGrammarEngine<string>(graph, random);
```

This ensures:
- **Same seed = same graph**
- **Can snapshot and restore**
- **Full history tracking**
- **Compare different branches**

## API Reference

### GraphGrammarVisualizer

| Property | Type | Description |
|----------|------|-------------|
| `Graph` | `Graph<string>` | The graph to visualize |
| `nodeSpacing` | `float` | Distance between nodes |
| `nodeSize` | `float` | Sphere radius |
| `layoutType` | `LayoutType` | Layout algorithm |
| `use3DLayout` | `bool` | Use 3D or 2D layout |

| Method | Description |
|--------|-------------|
| `SetGraph(graph)` | Update the visualized graph |
| `RefreshLayout()` | Recalculate node positions |
| `GetNodePosition(nodeId)` | Get world position of node |

### GraphGrammarDemo

| Property | Type | Description |
|----------|------|-------------|
| `demoType` | `DemoType` | Which demo to run |
| `seed` | `int` | Random seed |
| `autoStart` | `bool` | Start on play |
| `stepDelay` | `float` | Delay between steps |
| `maxIterations` | `int` | Maximum steps |

| Method | Description |
|--------|-------------|
| `StartGeneration()` | Begin animated generation |
| `ResetGeneration()` | Reset to initial state |
| `GenerateOneStep()` | Apply one rule |
