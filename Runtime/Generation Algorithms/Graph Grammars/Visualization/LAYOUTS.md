# Layout Examples

Visual reference for choosing the right layout algorithm.

## Grid Layout
```
Best for: Small graphs, uniform structure
Speed: ⚡⚡⚡ Instant
Quality: ⭐⭐ Functional

    [A]---[B]---[C]
     |     |     |
    [D]---[E]---[F]
     |     |     |
    [G]---[H]---[I]
```

**Pros:**
- Very fast (O(n))
- Predictable positions
- Easy to read small graps

**Cons:**
- Ignores graph structure
- Wastes space for sparse graphs
- No semantic meaning

**Use When:**
- Quick preview needed
- Graph has no interesting structure
- Performance critical (100+ nodes)

---

## Circular Layout
```
Best for: Cyclic graphs, equal importance nodes
Speed: ⚡⚡⚡ Instant
Quality: ⭐⭐⭐ Good for cycles

         [A]
     [H]     [B]
            
  [G]           [C]
            
     [F]     [D]
         [E]

(All nodes equidistant from center)
```

**Pros:**
- Fast (O(n))
- Clear edge visibility
- Good for cyclic structures
- No overlapping nodes

**Cons:**
- Long edges for nearby nodes
- Doesn't show hierarchy
- Can look messy with many edges

**Use When:**
- State machines
- Circular dependencies
- Nodes have equal importance
- Want to see all edges clearly

---

## Force-Directed Layout ⭐ Recommended
```
Best for: General graphs, natural clustering
Speed: ⚡⚡ Medium (50 iterations)
Quality: ⭐⭐⭐⭐⭐ Excellent

        [A]
       / | \
      /  |  \
    [B][C][D]
     |   |
    [E] [F]-[G]
             |
            [H]

(Nodes naturally space themselves)
```

**Pros:**
- Natural-looking layouts
- Shows clusters and communities
- Short edges for connected nodes
- Aesthetically pleasing
- Works for most graph types

**Cons:**
- Slower (O(n²) per iteration)
- Non-deterministic (varies slightly)
- May need tuning for optimal results

**Use When:**
- Default choice for most graphs
- Want professional-looking results
- Graph has interesting structure
- Size < 100 nodes

---

## Hierarchical Layout
```
Best for: Trees, DAGs, directed graphs
Speed: ⚡⚡⚡ Fast (O(n+e))
Quality: ⭐⭐⭐⭐ Excellent for trees

          [Root]
         /   |   \
        /    |    \
     [A]   [B]    [C]
     / \    |    / | \
   [D][E]  [F] [G][H][I]

(Clear parent-child relationships)
```

**Pros:**
- Shows hierarchy clearly
- Fast (BFS-based)
- Perfect for trees/DAGs
- Easy to trace paths

**Cons:**
- Only works well for directed graphs
- Cycles handled poorly
- Wide graphs need lots of space

**Use When:**
- Dungeon generation (entrance → rooms)
- Decision trees
- Organization charts
- Any directed acyclic graph
- Story/dialogue flow

---

## Layout Comparison

| Layout | Speed | Trees | Networks | Cycles | Large Graphs |
|--------|-------|-------|----------|--------|--------------|
| **Grid** | ⚡⚡⚡ | ⭐ | ⭐ | ⭐ | ⭐⭐⭐ |
| **Circular** | ⚡⚡⚡ | ⭐⭐ | ⭐⭐⭐ | ⭐⭐⭐⭐ | ⭐⭐⭐ |
| **Force-Directed** | ⚡⚡ | ⭐⭐⭐⭐ | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐ | ⭐⭐ |
| **Hierarchical** | ⚡⚡⚡ | ⭐⭐⭐⭐⭐ | ⭐⭐⭐ | ⭐ | ⭐⭐⭐ |

---

## Real Examples from Demos

### Branching Demo (L-System Tree)

**Initial:**
```
[F]
```

**After 3 iterations (Force-Directed 3D):**
```
          [F]
         / \
      [F]   [F]
     / \    / \
   [F][F] [F][F]
```

**Recommended Settings:**
```
Layout: Force-Directed
Use 3D Layout: ✓
Node Spacing: 2.5
Iterations: 4-6 (grows fast!)
```

---

### Dungeon Demo

**Initial:**
```
[Entrance]
```

**After 10 iterations (Hierarchical):**
```
         [Entrance]
              |
         [Corridor]
              |
           [Room]--------[Corridor]--------[Treasure]
         /        \
   [Corridor]  [Corridor]
       |            |
    [Room]       [Room]
```

**Recommended Settings:**
```
Layout: Hierarchical
Use 3D Layout: ✗ (2D clearer)
Show Edge Labels: ✓
Node Spacing: 3.0
```

---

## Tips for Each Layout

### Grid Layout
```csharp
visualizer.layoutType = LayoutType.Grid;
visualizer.nodeSpacing = 1.5f; // Compact
visualizer.showLabels = false; // Less clutter
```

### Circular Layout
```csharp
visualizer.layoutType = LayoutType.Circular;
visualizer.nodeSpacing = 2.0f; // Controls radius
visualizer.showEdgeLabels = true; // Edges visible
```

### Force-Directed Layout
```csharp
visualizer.layoutType = LayoutType.ForceDirected;
visualizer.nodeSpacing = 2.5f; // Desired edge length
visualizer.use3DLayout = true; // For 3D structures
// Note: Runs 50 iterations automatically
```

### Hierarchical Layout
```csharp
visualizer.layoutType = LayoutType.Hierarchical;
visualizer.nodeSpacing = 3.0f; // Vertical/horizontal spacing
visualizer.showNodeIds = true; // Track generations
```

---

## Switching Layouts at Runtime

```csharp
// Try different layouts to find the best one
void Update()
{
    if (Input.GetKeyDown(KeyCode.Alpha1))
        visualizer.layoutType = LayoutType.Grid;
    
    if (Input.GetKeyDown(KeyCode.Alpha2))
        visualizer.layoutType = LayoutType.Circular;
    
    if (Input.GetKeyDown(KeyCode.Alpha3))
        visualizer.layoutType = LayoutType.ForceDirected;
    
    if (Input.GetKeyDown(KeyCode.Alpha4))
        visualizer.layoutType = LayoutType.Hierarchical;
    
    if (Input.GetKeyDown(KeyCode.Alpha1, Alpha2, Alpha3, Alpha4))
        visualizer.RefreshLayout();
}
```

---

## Custom Layout Algorithm

Want to implement your own? Here's the structure:

```csharp
private void CalculateMyCustomLayout()
{
    // For each node in graph
    foreach (var node in Graph.Nodes)
    {
        // Calculate position based on your algorithm
        Vector3 position = CalculatePositionFor(node);
        
        // Account for center offset
        position += centerPosition;
        
        // Handle 2D vs 3D
        if (!use3DLayout)
            position.z = 0;
        
        // Store position
        _nodePositions[node.Id] = position;
    }
}
```

Common approaches:
- **Energy minimization**: Spring forces, repulsion
- **Spectral layout**: Eigenvectors of Laplacian matrix
- **Stress minimization**: Distance-based optimization
- **Layer-based**: Assign layers, minimize crossings

---

## Performance Benchmarks

Tested on graph with 50 nodes, 100 edges:

| Layout | Time | Quality | Recommendation |
|--------|------|---------|----------------|
| Grid | < 1ms | Basic | Quick preview |
| Circular | < 1ms | Good | Cycles/rings |
| Force-Directed | ~50ms | Excellent | Default choice |
| Hierarchical | ~5ms | Excellent* | Trees/DAGs only |

*Quality excellent for directed graphs, poor for general graphs

---

## Summary

**Default recommendation:** Force-Directed with `nodeSpacing = 2.5`

**Choose differently if:**
- Trees/hierarchies → **Hierarchical**
- Cycles/state machines → **Circular**
- Very large graphs (100+) → **Grid** or **Circular**
- Need instant layout → **Grid** or **Circular**
- Want 3D trees → **Force-Directed with use3DLayout**

**Pro tip:** Try Force-Directed first, then switch to Hierarchical if graph is tree-like!
