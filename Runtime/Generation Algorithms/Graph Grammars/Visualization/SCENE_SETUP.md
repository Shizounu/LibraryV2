# Unity Scene Setup Guide

Quick guide to set up graph grammar visualization in Unity.

## Method 1: Automated Setup (Recommended)

1. **Create Empty GameObject**
   - Right-click in Hierarchy → Create Empty
   - Name it "Graph Grammar Demo"

2. **Add Demo Component**
   - Select the GameObject
   - Add Component → Search "GraphGrammarDemo"
   - Component automatically adds GraphGrammarVisualizer

3. **Configure Settings**
   ```
   Demo Settings:
   - Demo Type: Branching (or your choice)
   - Seed: 42
   - Auto Start: ✓
   - Step Delay: 0.5s
   - Max Iterations: 15
   ```

4. **Press Play!**
   - Generation starts automatically
   - Use SPACE to step, G to continue, R to reset

## Method 2: Manual Setup

1. **Create GameObject**
   - Right-click in Hierarchy → Create Empty
   - Name it "Graph Visualizer"

2. **Add Visualizer**
   - Add Component → GraphGrammarVisualizer
   - Configure layout (Force-Directed recommended)
   - Set colors and spacing

3. **Create Your Generation Script**
   ```csharp
   using UnityEngine;
   using Shizounu.Library.GenerationAlgorithms.GraphGrammars;
   using Shizounu.Library.GenerationAlgorithms.GraphGrammars.Visualization;
   using Shizounu.Library.RandomSystem;

   public class MyGraphGenerator : MonoBehaviour
   {
       [SerializeField] private GraphGrammarVisualizer visualizer;
       
       void Start()
       {
           // Create graph and rules
           var graph = new Graph<string>();
           graph.AddNode(new GraphNode<string>("Start"));
           
           // Create engine with rules
           var context = new RngContext();
           var rng = context.GetOrCreateUser("Generator", seed: 42);
           var random = new RngSystemRandom(rng);
           var engine = new GraphGrammarEngine<string>(graph, random);
           
           // Add your rules here
           // ...
           
           // Generate and visualize
           engine.Generate(10);
           visualizer.SetGraph(engine.Graph);
       }
   }
   ```

4. **Assign Reference**
   - Drag visualizer GameObject to script's visualizer field

## Scene View Tips

### Better Visualization

1. **Adjust Scene Camera**
   - Position camera to see center (0, 0, 0)
   - Zoom out to see full graph

2. **Enable Gizmos**
   - Scene view toolbar → Gizmos button (on)
   - Labels appear automatically in editor

3. **Lighting** (for runtime)
   - Add Directional Light for better node visibility
   - Nodes are drawn as Gizmo spheres (always visible)

### Layout Configuration

**For Tree Structures** (Branching Demo):
```
Layout Type: Force Directed or Hierarchical
Node Spacing: 2.5
Use 3D Layout: ✓ (for cool 3D trees!)
```

**For Networks** (Dungeon Demo):
```
Layout Type: Hierarchical
Node Spacing: 3.0
Use 3D Layout: ✗ (clearer in 2D)
Show Edge Labels: ✓
```

**For Large Graphs** (100+ nodes):
```
Layout Type: Circular or Grid
Node Spacing: 1.5
Show Labels: ✗ (reduce clutter)
```

## Demo Types Explained

### Basic Expansion
- **What**: Simple linear growth (Start → A → B)
- **Best Layout**: Any
- **Iterations**: 1-3
- **Use Case**: Understanding rules

### Branching 🌳
- **What**: Tree-like fractal growth (F → F[+F]F[-F])
- **Best Layout**: Force-Directed (3D)
- **Iterations**: 4-6 (grows exponentially!)
- **Use Case**: L-Systems, plant growth

### Dungeon 🏰
- **What**: Procedural room layout with corridors
- **Best Layout**: Hierarchical
- **Iterations**: 10-20
- **Use Case**: Level generation, architecture

## Keyboard Controls

| Key | Action | Description |
|-----|--------|-------------|
| **SPACE** | Step Once | Apply one rule and update |
| **G** | Generate | Animate full generation |
| **R** | Reset | Start over with same seed |

Change seed in Inspector to get different results!

## Common Issues

### "No Visualizer Found"
**Solution**: GraphGrammarDemo automatically adds visualizer if missing. If you see this warning, just press Play again.

### Labels Not Showing
**Solution**: Labels only show in Scene view in Editor. They use `UnityEditor.Handles` API which isn't available in builds.

### Nodes Overlapping
**Solution**: 
- Increase Node Spacing
- Use Force-Directed layout
- Reduce graph size (lower Max Iterations)

### Generation Too Fast
**Solution**: Increase Step Delay in demo component (try 1.0 or 2.0 seconds)

### Graph Not Updating
**Solution**: Call `visualizer.RefreshLayout()` after modifying graph

## Advanced: Custom Demo

Create your own demo type:

```csharp
// In GraphGrammarDemo.cs, add to CreateXXXDemo methods:

private Graph<string> CreateMyCustomDemo(out GraphGrammarEngine<string> engine)
{
    var initialGraph = new Graph<string>();
    // Your initial graph setup
    
    // Your rules
    var rule1 = new RuleBuilder<string>()
        .WithName("My Rule")
        .WithPattern(myPattern)
        .WithReplacement(myReplacement)
        .Build();
    
    var random = new RngSystemRandom(_rngUser);
    engine = new GraphGrammarEngine<string>(initialGraph, random);
    engine.AddRule(rule1);
    engine.SelectionStrategy = RuleSelectionStrategy.Random;
    
    return initialGraph;
}
```

Then add to the `DemoType` enum and switch statement!

## Performance Tips

1. **Keep iterations low** (< 20 for interactive demos)
2. **Use simpler layouts** for large graphs (Grid > Circular > Force-Directed)
3. **Disable labels** for graphs with 50+ nodes
4. **Increase step delay** to see what's happening
5. **Use predicates** to limit graph size

## Next Steps

- Read `/Visualization/README.md` for full API reference
- Check `/Examples/GraphGrammarExample.cs` for code examples
- Experiment with different seeds and rules
- Try creating custom node colors based on attributes
- Implement your own layout algorithm

## Example Scene Layout

```
Scene Hierarchy:
├── Main Camera
├── Directional Light
└── Graph Grammar Demo
    └── GraphGrammarVisualizer (auto-added)
```

Perfect for testing and iteration!
