# Graph Grammars Quick Reference

## Core API

### Creating Graphs

```csharp
var graph = new Graph<string>();
var node = new GraphNode<string>("data");
node.SetAttribute("key", "value");
graph.AddNode(node);

var edge = new GraphEdge<string>(nodeA, nodeB, "label");
graph.AddEdge(edge);
```

### Creating Rules

```csharp
// Method 1: Direct construction
var pattern = new Graph<string>();
var replacement = new Graph<string>();
// ... populate graphs ...
var rule = new GraphProductionRule<string>(pattern, replacement, "Rule Name");
rule.Priority = 10;

// Method 2: Fluent builder
var rule = new RuleBuilder<string>()
    .WithName("My Rule")
    .WithPriority(5)
    .WithPattern(patternGraph)
    .WithReplacement(replacementGraph)
    .Preserve(patternNode, replacementNode)
    .WithPredicate((g, m) => g.NodeCount < 100)
    .Build();
```

### Running Generation

```csharp
var engine = new GraphGrammarEngine<string>(initialGraph, seed: 12345);
engine.AddRule(rule1);
engine.AddRule(rule2);
engine.SelectionStrategy = RuleSelectionStrategy.Random;
engine.MaxIterations = 100;

// Generate until no rules apply or max iterations
var result = engine.Generate();

// Generate N iterations
var result = engine.Generate(iterations: 10);

// Apply one rule at a time
bool applied = engine.ApplyOneRule();
```

### Integration with RNG System

```csharp
using Shizounu.Library.RandomSystem;

var context = new RngContext(seed: 42);
var rngUser = context.CreateUser("GraphGrammar");

// Wrap RngUser as System.Random for graph grammar engine
var random = new RngSystemRandom(rngUser);
var engine = new GraphGrammarEngine<string>(initialGraph, random);

// Now generation is reproducible
```

## Common Patterns

### Simple Expansion (A → A B)
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
rule.PreserveNode(a, newA); // Keep original node, add new one
```

### Branching (A → B ← A → C)
```csharp
var pattern = new Graph<T>();
var a = new GraphNode<T>(value);
pattern.AddNode(a);

var replacement = new Graph<T>();
var center = new GraphNode<T>(value);
var left = new GraphNode<T>(leftValue);
var right = new GraphNode<T>(rightValue);
replacement.AddNode(center);
replacement.AddNode(left);
replacement.AddNode(right);
replacement.AddEdge(new GraphEdge<T>(center, left, "left"));
replacement.AddEdge(new GraphEdge<T>(center, right, "right"));

var rule = new GraphProductionRule<T>(pattern, replacement);
rule.PreserveNode(a, center);
```

### Conditional Rule
```csharp
rule.Predicate = (graph, match) => 
{
    // Only apply if graph is small enough
    if (graph.NodeCount >= 50)
        return false;
    
    // Only apply if matched node has specific attribute
    var matchedNode = match[patternNode];
    return matchedNode.GetAttribute("expandable") == "true";
};
```

### Custom Post-Processing
```csharp
rule.OnApply = (graph, match) => 
{
    // Update node attributes after rule application
    foreach (var node in graph.Nodes)
    {
        if (node.Data == "NewRoom")
            node.SetAttribute("visited", "false");
    }
};
```

## Rule Selection Strategies

| Strategy | Behavior | Use Case |
|----------|----------|----------|
| `FirstMatch` | Select first matching rule by priority | Deterministic generation |
| `Random` | Random selection among matches | Varied output |
| `HighestPriority` | Always pick highest priority | Strict rule hierarchy |
| `MaximizeGrowth` | Pick rule that adds most nodes | Create large structures |
| `MinimizeGrowth` | Pick rule that adds fewest nodes | Create compact structures |

## Pattern Matching

### Node Attributes
```csharp
// Pattern node with attribute
var patternNode = new GraphNode<string>("Room");
patternNode.SetAttribute("type", "entrance");

// Matches any node with data="Room" and attribute type="entrance"
// Non-strict: target can have additional attributes
// Strict: target must have exactly these attributes
engine.StrictAttributeMatching = false; // or true
```

### Edge Labels
```csharp
var edge = new GraphEdge<T>(from, to, "corridor");
// Matches only edges with label "corridor"
```

### Preserved Nodes
```csharp
// Pattern node vs replacement node
rule.PreserveNode(patternNode, replacementNode);

// Effect:
// - Pattern node is NOT deleted
// - Pattern node's data/attributes updated from replacement node
// - Replacement node refers to the preserved node in new edges
```

## Result Codes

| Result | Meaning |
|--------|---------|
| `Success` | Generation completed requested iterations |
| `NoRulesApplicable` | No rules can match current graph |
| `MaxIterationsReached` | Hit iteration limit before all rules failed |
| `InvalidState` | No rules defined or invalid configuration |

## Performance Tips

1. **Keep patterns small** (2-5 nodes) - pattern matching is NP-complete
2. **Use attributes** to narrow matches early
3. **Set MaxIterations** to prevent infinite loops
4. **Use predicates** to fail fast on invalid contexts
5. **Prioritize rules** to control which are tried first
6. **Consider SelectionStrategy** - MaximizeGrowth/MinimizeGrowth evaluate all rules

## Common Use Cases

### Dungeon Generation
- Nodes: Rooms, corridors, doors
- Edges: Connections between areas
- Rules: Expand rooms, add corridors, place special rooms

### Story/Dialogue Trees
- Nodes: Story beats, dialogue states
- Edges: Transitions, player choices
- Rules: Expand plot points, add branches, create loops

### L-Systems
- Nodes: Symbols (F, +, -, [, ])
- Edges: Sequential connections, brackets for stacks
- Rules: Production rules (F → FF+[+F-F-F]-[-F+F+F])

### City/Road Networks
- Nodes: Intersections, buildings, landmarks
- Edges: Roads, paths
- Rules: Subdivide blocks, add infrastructure

### Neural Network Topologies
- Nodes: Layers, neurons
- Edges: Connections with weights
- Rules: Add layers, create skip connections, prune

## Error Handling

```csharp
var result = engine.Generate();

switch (result)
{
    case GraphGrammarResult.Success:
        // Normal completion
        break;
    
    case GraphGrammarResult.NoRulesApplicable:
        // Graph reached terminal state
        // This might be expected or indicate missing rules
        break;
    
    case GraphGrammarResult.MaxIterationsReached:
        // Safety limit hit - possibly infinite loop
        // Consider increasing MaxIterations or adding termination conditions
        break;
    
    case GraphGrammarResult.InvalidState:
        // No rules defined or null initial graph
        break;
}
```

## Events

```csharp
engine.OnRuleApplied += (rule, iteration) => 
{
    Debug.Log($"Applied {rule.Name} at iteration {iteration}");
    Debug.Log($"Graph now has {engine.Graph.NodeCount} nodes");
};
```

## Debugging

```csharp
// Print graph structure
foreach (var node in graph.Nodes)
{
    Console.WriteLine($"Node {node.Id}: {node.Data}");
    
    foreach (var edge in graph.GetOutgoingEdges(node))
    {
        Console.WriteLine($"  -> {edge.To.Id} ({edge.Label})");
    }
}

// Check if pattern exists in graph
var engine = new GraphGrammarEngine<T>(graph, random);
var dummyRule = new GraphProductionRule<T>(pattern, pattern);
// If rule never applies, pattern doesn't exist in graph
```
