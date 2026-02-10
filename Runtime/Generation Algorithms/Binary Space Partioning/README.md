# Binary Space Partitioning (BSP) Algorithm

A flexible, reusable Binary Space Partitioning implementation for both 2D and 3D space partitioning in Unity. This algorithm recursively divides space into two half-spaces using axis-aligned planes, creating a hierarchical spatial data structure useful for collision detection, spatial queries, dungeon generation, and more.

## Features

- **2D Support**: Partition 2D rectangular areas with alternating vertical/horizontal splits
- **3D Support**: Partition 3D volumes with configurable axis selection strategies
- **Multiple Split Strategies**:
  - **Axis Selection**: Alternating, Longest Side, or Always X/Y/Z
  - **Split Position**: Middle, Random, or Golden Ratio divisions
- **Spatial Queries**: Fast point lookup to find which leaf contains a given coordinate
- **Flexible Configuration**: Control minimum node size, random seeds, and target leaf counts
- **Tree Traversal**: Get all leaves, measure depth, count nodes, and store custom data

## Quick Start

### 2D Example

```csharp
using Shizounu.Library.GenerationAlgorithms.BSP;
using UnityEngine;

// Create a builder for 100x100 area
var bounds = new Rect(0, 0, 100, 100);
var builder = new BspBuilder(
    axisSelection: BspBuilder.AxisSelection.Alternating,
    splitPosition: BspBuilder.SplitPosition.Middle,
    minNodeSize: 10
);

// Build the tree
BspNode root = builder.Build(bounds);

// Query which leaf contains a point
Vector2 testPoint = new Vector2(45, 67);
BspNode leaf = root.FindLeafAt(testPoint);

// Get statistics
int leafCount = root.CountLeaves();
int depth = root.GetDepth();
```

### 3D Example

```csharp
using Shizounu.Library.GenerationAlgorithms.BSP;
using UnityEngine;

// Create a builder for a 100x100x100 volume
var bounds = new Bounds(Vector3.one * 50, Vector3.one * 100);
var builder = new BspBuilder3D(
    axisSelection: BspBuilder3D.AxisSelection.Cycling,
    splitPosition: BspBuilder3D.SplitPosition.Middle,
    minNodeSize: 5f
);

// Build the tree
BspNode3D root = builder.Build(bounds);

// Query which leaf contains a point
Vector3 testPoint = new Vector3(45, 67, 30);
BspNode3D leaf = root.FindLeafAt(testPoint);

// Get all leaves for iteration
var leaves = new System.Collections.Generic.List<BspNode3D>();
root.GetAllLeaves(leaves);
```

## Core Classes

### BspNode (2D)

Represents a single node in a 2D BSP tree.

**Key Properties:**
- `Bounds`: The rectangular area this node covers
- `Axis`: The split axis (X or Y)
- `SplitPosition`: The coordinate where the split occurs
- `IsLeaf`: Whether this is a leaf node (no children)
- `LeftChild` / `RightChild`: Child nodes
- `UserData`: Custom user-defined data (for leaf nodes)

**Key Methods:**
- `FindLeafAt(Vector2)`: Find the leaf containing a point
- `GetAllLeaves(List<BspNode>)`: Collect all leaf nodes
- `GetDepth()`: Get tree depth from this node
- `CountLeaves()`: Count leaf nodes in subtree

### BspNode3D (3D)

3D equivalent of BspNode, using `Bounds` instead of `Rect` and supporting Z-axis splits.

### BspBuilder (2D)

Builds 2D BSP trees with configurable splitting strategies.

**Constructor Parameters:**
- `axisSelection`: How to choose split axes
  - `Alternating`: X at even depths, Y at odd (fastest, most balanced)
  - `LongestSide`: Always split the longest dimension
  - `AlwaysX` / `AlwaysY`: Always split same axis
- `splitPosition`: Where to place splits
  - `Middle`: Center of space (default, most balanced)
  - `Random`: Random position in space
  - `GoldenRatio`: 1/φ ratio split
- `minNodeSize`: Minimum size (width/height) before stopping subdivision (default: 1)
- `seed`: Random seed for reproducibility (default: random)

**Methods:**
- `Build(Rect)`: Build a standard BSP tree
- `BuildToTargetLeaves(Rect, int)`: Build until approximately reaching target leaf count

### BspBuilder3D (3D)

3D equivalent with similar options but supporting cycling through X/Y/Z axes.

## Use Cases

### Dungeon Generation
```csharp
// Divide dungeon area into rooms
var dungeon = new Rect(0, 0, 1000, 1000);
var builder = new BspBuilder(
    BspBuilder.AxisSelection.Alternating,
    BspBuilder.SplitPosition.Random,
    minNodeSize: 50,
    seed: Random.Range(0, int.MaxValue)
);

BspNode root = builder.Build(dungeon);
var roomList = new System.Collections.Generic.List<BspNode>();
root.GetAllLeaves(roomList);

// Each leaf becomes a room
foreach (var room in roomList)
{
    CreateRoom(room.Bounds);
}
```

### Collision Spatial Index
```csharp
// Partition space for fast collision queries
var world = new Bounds(Vector3.zero, new Vector3(1000, 1000, 1000));
var builder = new BspBuilder3D(
    BspBuilder3D.AxisSelection.LongestSide,
    BspBuilder3D.SplitPosition.Middle,
    minNodeSize: 50f
);

BspNode3D root = builder.Build(world);

// For each object, store it in the appropriate leaf
foreach (var obj in objects)
{
    var leaf = root.FindLeafAt(obj.position);
    if (leaf != null)
    {
        leaf.UserData = obj;  // Store reference
    }
}
```

### Aesthetic Layouts
```csharp
// Golden ratio splits create visually pleasing divisions
var canvas = new Rect(0, 0, 1920, 1080);
var builder = new BspBuilder(
    BspBuilder.AxisSelection.LongestSide,
    BspBuilder.SplitPosition.GoldenRatio
);

BspNode root = builder.Build(canvas);
// Leaves represent naturally proportioned sub-regions
```

## Namespace

All BSP classes are in the `Shizounu.Library.GenerationAlgorithms.BSP` namespace.

## Examples

Demo scenes are provided:
- `BspExample2D.cs`: Full working example with 2D visualization
- `BspExample3D.cs`: Full working example with 3D gizmo visualization

## Performance Characteristics

- **Build Time**: O(n) where n is the number of leaf nodes
- **Point Query**: O(log n) average, O(n) worst case
- **Memory**: O(n) for n leaf nodes
- **Tree Depth**: O(log n) with balanced splits, O(n) worst case

## Notes

- All splits are **axis-aligned** (parallel to X, Y, or Z axes)
- Leaf nodes have no children and can store custom data
- The tree is **immutable** after creation (no dynamic insertion/deletion)
- Split positions can be customized via subclassing the builders
