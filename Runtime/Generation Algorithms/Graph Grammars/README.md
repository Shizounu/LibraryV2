# Graph Grammars

A flexible graph grammar system for procedural generation using production rules to transform graphs.

## Overview

Graph grammars provide a powerful way to generate complex structures by applying transformation rules to graphs. This system allows you to define patterns (left-hand side) and their replacements (right-hand side), then automatically applies these rules to grow and transform graphs.

## 🎨 Visualization

**NEW**: Real-time Unity visualization with interactive demos! See `/Visualization/README.md` for details.

- **Multiple layouts**: Grid, Circular, Force-Directed, Hierarchical
- **Interactive controls**: Step through generation with keyboard
- **Color-coded nodes**: Visual distinction by type
- **Live updates**: Watch generation in Scene view

**Quick Start:**
1. Add `GraphGrammarDemo` component to a GameObject
2. Choose demo type (Branching/Dungeon)
3. Press Play and use SPACE to step, G to generate, R to reset

## Core Concepts

### Graphs
- **GraphNode<T>**: Nodes store typed data and optional attributes
- **GraphEdge<T>**: Directed edges with labels, weights, and attributes
- **Graph<T>**: Container managing nodes and edges with efficient lookups

### Production Rules
- **Pattern (L)**: The subgraph to search for
- **Replacement (R)**: The subgraph to insert when pattern is found
- **Preserved Nodes**: Nodes that are modified but not deleted
- **Predicates**: Additional conditions for rule application
- **Priority**: Controls rule selection order

### Grammar Engine
- Matches patterns in the current graph
- Applies production rules to transform the graph
- Supports multiple rule selection strategies
- Handles iterative generation with configurable limits

## Quick Start

```csharp
using Shizounu.Library.GenerationAlgorithms.GraphGrammars;

// Create initial graph with a single start node
var initialGraph = new Graph<string>();
var startNode = new GraphNode<string>("Start");
startNode.SetAttribute("type", "root");
initialGraph.AddNode(startNode);

// Create a production rule: Start -> A - B
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

// Create engine and generate
var engine = new GraphGrammarEngine<string>(initialGraph, seed: 12345);
engine.AddRule(rule);
engine.SelectionStrategy = RuleSelectionStrategy.FirstMatch;

var result = engine.Generate(iterations: 5);

// Access the generated graph
var finalGraph = engine.Graph;
Console.WriteLine($"Generated graph: {finalGraph.NodeCount} nodes, {finalGraph.EdgeCount} edges");
```

## Rule Selection Strategies

```csharp
// First matching rule (default)
engine.SelectionStrategy = RuleSelectionStrategy.FirstMatch;

// Random selection for varied output
engine.SelectionStrategy = RuleSelectionStrategy.Random;

// Highest priority rule
engine.SelectionStrategy = RuleSelectionStrategy.HighestPriority;

// Maximize or minimize graph growth
engine.SelectionStrategy = RuleSelectionStrategy.MaximizeGrowth;
engine.SelectionStrategy = RuleSelectionStrategy.MinimizeGrowth;
```

## Advanced Features

### Preserved Nodes

Keep nodes from the pattern and update them instead of deleting:

```csharp
var rule = new GraphProductionRule<string>(pattern, replacement);

// The pattern node maps to the replacement node
// Pattern node is updated with replacement node's data
rule.PreserveNode(patternNode, replacementNode);
```

### Rule Predicates

Add conditions for rule application:

```csharp
rule.Predicate = (graph, match) => 
{
    // Only apply if graph has fewer than 50 nodes
    return graph.NodeCount < 50;
};
```

### Custom Callbacks

Execute code after rule application:

```csharp
rule.OnApply = (graph, match) => 
{
    // Custom logic after rule is applied
    Debug.Log($"Rule applied! Graph now has {graph.NodeCount} nodes");
};
```

### Fluent Rule Building

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

## Pattern Matching

The engine performs subgraph isomorphism to find pattern matches:

1. **Node Matching**: Compares data and attributes
2. **Edge Matching**: Verifies connectivity and labels
3. **Constraint Propagation**: Uses edges to narrow candidate nodes
4. **Strict/Non-Strict**: Configure attribute matching behavior

```csharp
// Strict: all attributes must match exactly
engine.StrictAttributeMatching = true;

// Non-strict: pattern attributes must exist in target (allows extra attributes)
engine.StrictAttributeMatching = false;
```

## Use Cases

### Dungeon Generation
Define rooms as nodes and connections as edges. Rules expand rooms into corridors and additional rooms.

### Story Generation
Nodes represent story beats, edges represent transitions. Rules expand plot points into scenes.

### L-System-like Generation
Use string nodes with production rules to create branching structures, similar to L-systems but with full graph topology.

### City Layout
Nodes are city blocks or landmarks, edges are roads. Rules subdivide blocks and add infrastructure.

### Dialogue Trees
Generate conversational structures where nodes are dialogue states and edges are player choices.

## Performance Considerations

- **Pattern Complexity**: Larger patterns take longer to match (subgraph isomorphism is NP-complete)
- **Graph Size**: Matching time increases with host graph size
- **Rule Count**: More rules mean more patterns to try per iteration
- **Strategies**: `MaximizeGrowth` and `MinimizeGrowth` evaluate all rules per iteration

For best performance:
- Keep patterns small (2-5 nodes)
- Use attributes to narrow matches early
- Set reasonable `MaxIterations` limits
- Use predicates to fail fast on invalid contexts

## Integration with RNG System

Use the reproducible RNG system for deterministic generation:

```csharp
using Shizounu.Library.RandomSystem;

var context = new RngContext(seed: 12345);
var rng = context.CreateUser("GraphGrammar");

// Create System.Random wrapper using RNG user
var random = new RngSystemRandom(rng);
var engine = new GraphGrammarEngine<string>(initialGraph, random);

// Generation is now reproducible with the same seed
```

## API Reference

### GraphNode<T>
- `int Id`: Unique identifier
- `T Data`: Node data
- `Dictionary<string, string> Attributes`: Metadata
- `bool Matches(GraphNode<T> other, bool strict)`: Pattern matching

### GraphEdge<T>
- `GraphNode<T> From`: Source node
- `GraphNode<T> To`: Target node  
- `string Label`: Edge label
- `float Weight`: Edge weight
- `Dictionary<string, string> Attributes`: Metadata

### Graph<T>
- `AddNode(GraphNode<T>)`: Add node
- `RemoveNode(GraphNode<T>)`: Remove node and connected edges
- `AddEdge(GraphEdge<T>)`: Add edge (auto-adds nodes)
- `RemoveEdge(GraphEdge<T>)`: Remove edge
- `GetOutgoingEdges(GraphNode<T>)`: Find edges from node
- `GetIncomingEdges(GraphNode<T>)`: Find edges to node
- `GetNeighbors(GraphNode<T>)`: Adjacent nodes
- `Graph<T> Clone()`: Deep copy

### GraphProductionRule<T>
- `Graph<T> Pattern`: Left-hand side
- `Graph<T> Replacement`: Right-hand side
- `string Name`: Rule identifier
- `int Priority`: Selection priority
- `Func<...> Predicate`: Application condition
- `Action<...> OnApply`: Post-application callback
- `PreserveNode(pattern, replacement)`: Mark node as preserved

### GraphGrammarEngine<T>
- `Graph<T> Graph`: Current graph state
- `AddRule(GraphProductionRule<T>)`: Add rule
- `Generate()`: Generate until no rules apply
- `Generate(int iterations)`: Generate N iterations
- `ApplyOneRule()`: Apply one rule
- `RuleSelectionStrategy SelectionStrategy`: How to pick rules
- `int MaxIterations`: Safety limit
- `bool StrictAttributeMatching`: Matching mode
- `event Action<...> OnRuleApplied`: Event after each rule

## Examples

See `/Examples/GraphGrammarExample.cs` for comprehensive examples including:
- Basic graph expansion
- L-system-like branching
- Dungeon room generation
- Rule priorities and predicates
- Different selection strategies
