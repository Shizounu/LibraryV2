using System;
using System.Collections.Generic;
using System.Linq;
using Shizounu.Library.RandomSystem;
using Shizounu.Library.GenerationAlgorithms.Shared;

namespace Shizounu.Library.GenerationAlgorithms.GraphGrammars
{
    /// <summary>
    /// Result codes for graph grammar generation.
    /// </summary>
    public enum GraphGrammarResult
    {
        /// <summary>Generation completed successfully.</summary>
        Success,
        /// <summary>No rules could be applied to continue generation.</summary>
        NoRulesApplicable,
        /// <summary>Maximum iterations reached.</summary>
        MaxIterationsReached,
        /// <summary>Invalid initial state or rules.</summary>
        InvalidState
    }

    /// <summary>
    /// Strategy for selecting which rule to apply when multiple rules match.
    /// </summary>
    public enum RuleSelectionStrategy
    {
        /// <summary>Select the first matching rule (ordered by priority).</summary>
        FirstMatch,
        /// <summary>Select a random matching rule.</summary>
        Random,
        /// <summary>Select the highest priority rule.</summary>
        HighestPriority,
        /// <summary>Try all applicable rules and select the one that produces the most nodes.</summary>
        MaximizeGrowth,
        /// <summary>Try all applicable rules and select the one that produces the fewest nodes.</summary>
        MinimizeGrowth
    }

    /// <summary>
    /// Main engine for applying graph grammar production rules to generate graphs.
    /// </summary>
    /// <typeparam name="T">The type of data stored in nodes</typeparam>
    public class GraphGrammarEngine<T> : IRngProvider
    {
        private readonly List<GraphProductionRule<T>> _rules = new List<GraphProductionRule<T>>();
        private readonly IRngSource _rngSource;

        /// <summary>
        /// The current state of the graph being generated.
        /// </summary>
        public Graph<T> Graph { get; private set; }

        /// <summary>
        /// All production rules in priority order.
        /// </summary>
        public IReadOnlyList<GraphProductionRule<T>> Rules => _rules;

        /// <summary>
        /// Gets the RNG source used by this engine.
        /// </summary>
        public IRngSource RngSource => _rngSource;

        /// <summary>
        /// Number of rule applications performed.
        /// </summary>
        public int IterationCount { get; private set; }

        /// <summary>
        /// Strategy for selecting rules when multiple match.
        /// </summary>
        public RuleSelectionStrategy SelectionStrategy { get; set; }

        /// <summary>
        /// Maximum number of iterations before stopping generation.
        /// </summary>
        public int MaxIterations { get; set; }

        /// <summary>
        /// If true, uses strict attribute matching for patterns.
        /// </summary>
        public bool StrictAttributeMatching { get; set; }

        /// <summary>
        /// Event raised after each rule application.
        /// </summary>
        public event Action<GraphProductionRule<T>, int> OnRuleApplied;

        /// <summary>
        /// Creates a new graph grammar engine with an initial graph.
        /// </summary>
        public GraphGrammarEngine(Graph<T> initialGraph, IRngSource rngSource)
        {
            Graph = initialGraph ?? throw new ArgumentNullException(nameof(initialGraph));
            _rngSource = rngSource ?? throw new ArgumentNullException(nameof(rngSource));
            
            SelectionStrategy = RuleSelectionStrategy.FirstMatch;
            MaxIterations = 1000;
            StrictAttributeMatching = false;
            IterationCount = 0;
        }

        /// <summary>
        /// Creates a new graph grammar engine with a seed for randomness.
        /// </summary>
        public GraphGrammarEngine(Graph<T> initialGraph, int seed)
            : this(initialGraph, GenerationRng.Create(seed))
        {
        }

        /// <summary>
        /// Adds a production rule to the grammar.
        /// </summary>
        public void AddRule(GraphProductionRule<T> rule)
        {
            if (rule == null)
                throw new ArgumentNullException(nameof(rule));

            _rules.Add(rule);
            _rules.Sort((a, b) => b.Priority.CompareTo(a.Priority)); // Sort by priority descending
        }

        /// <summary>
        /// Removes a production rule from the grammar.
        /// </summary>
        public bool RemoveRule(GraphProductionRule<T> rule)
        {
            return _rules.Remove(rule);
        }

        /// <summary>
        /// Clears all production rules.
        /// </summary>
        public void ClearRules()
        {
            _rules.Clear();
        }

        /// <summary>
        /// Generates a graph by repeatedly applying production rules.
        /// </summary>
        public GraphGrammarResult Generate()
        {
            if (_rules.Count == 0)
                return GraphGrammarResult.InvalidState;

            IterationCount = 0;

            while (IterationCount < MaxIterations)
            {
                var applied = Step();
                if (!applied)
                    return GraphGrammarResult.NoRulesApplicable;

                IterationCount++;
            }

            return GraphGrammarResult.MaxIterationsReached;
        }

        /// <summary>
        /// Generates a graph for a specific number of steps.
        /// </summary>
        public GraphGrammarResult Generate(int steps)
        {
            if (_rules.Count == 0)
                return GraphGrammarResult.InvalidState;

            for (int i = 0; i < steps; i++)
            {
                var applied = Step();
                if (!applied)
                    return GraphGrammarResult.NoRulesApplicable;

                IterationCount++;
            }

            return GraphGrammarResult.Success;
        }

        /// <summary>
        /// Attempts to apply one production rule to the graph.
        /// </summary>
        public bool Step()
        {
            // Find all applicable rules with their matches
            var applicableRules = new List<(GraphProductionRule<T> rule, List<Dictionary<GraphNode<T>, GraphNode<T>>> matches)>();

            foreach (var rule in _rules)
            {
                var matches = FindMatches(rule.Pattern);
                if (matches.Count > 0)
                {
                    // Filter by predicate
                    var validMatches = matches.Where(m => rule.CanApply(Graph, m)).ToList();
                    if (validMatches.Count > 0)
                    {
                        applicableRules.Add((rule, validMatches));
                    }
                }
            }

            if (applicableRules.Count == 0)
                return false;

            // Select rule based on strategy
            var (selectedRule, selectedMatches) = SelectRule(applicableRules);
            
            // Select a random match for the rule
            var match = selectedMatches[_rngSource.NextInt(selectedMatches.Count)];

            // Apply the rule
            ApplyRule(selectedRule, match);

            OnRuleApplied?.Invoke(selectedRule, IterationCount);

            return true;
        }

        /// <summary>
        /// Finds all subgraph matches of a pattern in the current graph.
        /// </summary>
        private List<Dictionary<GraphNode<T>, GraphNode<T>>> FindMatches(Graph<T> pattern)
        {
            var matches = new List<Dictionary<GraphNode<T>, GraphNode<T>>>();

            if (pattern.NodeCount == 0)
                return matches;

            // Start with first pattern node
            var patternNodes = pattern.Nodes.ToList();
            var firstPatternNode = patternNodes[0];

            // Find all potential starting nodes in the host graph
            foreach (var hostNode in Graph.Nodes)
            {
                if (firstPatternNode.Matches(hostNode, StrictAttributeMatching))
                {
                    var mapping = new Dictionary<GraphNode<T>, GraphNode<T>>();
                    mapping[firstPatternNode] = hostNode;

                    // Try to match the rest of the pattern
                    if (TryMatchPattern(pattern, patternNodes, 1, mapping))
                    {
                        matches.Add(new Dictionary<GraphNode<T>, GraphNode<T>>(mapping));
                    }
                }
            }

            return matches;
        }

        /// <summary>
        /// Recursively attempts to match a pattern starting from a partial mapping.
        /// </summary>
        private bool TryMatchPattern(Graph<T> pattern, List<GraphNode<T>> patternNodes, int index, Dictionary<GraphNode<T>, GraphNode<T>> mapping)
        {
            if (index >= patternNodes.Count)
            {
                // All nodes matched, now verify edges
                return VerifyEdgeMapping(pattern, mapping);
            }

            var patternNode = patternNodes[index];

            // Check if this pattern node is constrained by edges to already-mapped nodes
            var constraints = pattern.Edges
                .Where(e => (e.From == patternNode && mapping.ContainsKey(e.To)) ||
                           (e.To == patternNode && mapping.ContainsKey(e.From)))
                .ToList();

            if (constraints.Count > 0)
            {
                // Find candidate host nodes that satisfy edge constraints
                var candidates = new HashSet<GraphNode<T>>(Graph.Nodes.Where(n => 
                    patternNode.Matches(n, StrictAttributeMatching) && !mapping.ContainsValue(n)));

                foreach (var constraint in constraints)
                {
                    var constraintCandidates = new HashSet<GraphNode<T>>();

                    if (constraint.From == patternNode)
                    {
                        // patternNode -> mappedNode
                        var mappedTo = mapping[constraint.To];
                        foreach (var edge in Graph.Edges)
                        {
                            if (edge.To == mappedTo && constraint.Matches(edge, StrictAttributeMatching))
                            {
                                constraintCandidates.Add(edge.From);
                            }
                        }
                    }
                    else
                    {
                        // mappedNode -> patternNode
                        var mappedFrom = mapping[constraint.From];
                        foreach (var edge in Graph.Edges)
                        {
                            if (edge.From == mappedFrom && constraint.Matches(edge, StrictAttributeMatching))
                            {
                                constraintCandidates.Add(edge.To);
                            }
                        }
                    }

                    candidates.IntersectWith(constraintCandidates);
                }

                // Try each candidate
                foreach (var candidate in candidates)
                {
                    mapping[patternNode] = candidate;
                    if (TryMatchPattern(pattern, patternNodes, index + 1, mapping))
                        return true;
                    mapping.Remove(patternNode);
                }

                return false;
            }
            else
            {
                // No constraints, try all unused nodes that match
                foreach (var hostNode in Graph.Nodes)
                {
                    if (patternNode.Matches(hostNode, StrictAttributeMatching) && !mapping.ContainsValue(hostNode))
                    {
                        mapping[patternNode] = hostNode;
                        if (TryMatchPattern(pattern, patternNodes, index + 1, mapping))
                            return true;
                        mapping.Remove(patternNode);
                    }
                }

                return false;
            }
        }

        /// <summary>
        /// Verifies that all edges in the pattern match edges in the host graph given a node mapping.
        /// </summary>
        private bool VerifyEdgeMapping(Graph<T> pattern, Dictionary<GraphNode<T>, GraphNode<T>> mapping)
        {
            foreach (var patternEdge in pattern.Edges)
            {
                var hostFrom = mapping[patternEdge.From];
                var hostTo = mapping[patternEdge.To];

                bool found = false;
                foreach (var hostEdge in Graph.Edges)
                {
                    if (hostEdge.From == hostFrom && hostEdge.To == hostTo && 
                        patternEdge.Matches(hostEdge, StrictAttributeMatching))
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Selects which rule to apply based on the selection strategy.
        /// </summary>
        private (GraphProductionRule<T>, List<Dictionary<GraphNode<T>, GraphNode<T>>>) SelectRule(
            List<(GraphProductionRule<T> rule, List<Dictionary<GraphNode<T>, GraphNode<T>>> matches)> applicableRules)
        {
            switch (SelectionStrategy)
            {
                case RuleSelectionStrategy.FirstMatch:
                    return applicableRules[0];

                case RuleSelectionStrategy.Random:
                    return applicableRules[_rngSource.NextInt(applicableRules.Count)];

                case RuleSelectionStrategy.HighestPriority:
                    return applicableRules.OrderByDescending(r => r.rule.Priority).First();

                case RuleSelectionStrategy.MaximizeGrowth:
                    return applicableRules.OrderByDescending(r => r.rule.Replacement.NodeCount - r.rule.Pattern.NodeCount).First();

                case RuleSelectionStrategy.MinimizeGrowth:
                    return applicableRules.OrderBy(r => r.rule.Replacement.NodeCount - r.rule.Pattern.NodeCount).First();

                default:
                    return applicableRules[0];
            }
        }

        /// <summary>
        /// Applies a production rule to the graph given a match mapping.
        /// </summary>
        private void ApplyRule(GraphProductionRule<T> rule, Dictionary<GraphNode<T>, GraphNode<T>> match)
        {
            // Create new nodes from replacement graph
            var replacementMapping = new Dictionary<GraphNode<T>, GraphNode<T>>();

            // Handle preserved nodes first
            foreach (var kvp in rule.PreservedNodes)
            {
                var patternNode = kvp.Key;
                var replacementNode = kvp.Value;
                var hostNode = match[patternNode];

                // Update host node with replacement node's data
                hostNode.Data = replacementNode.Data;
                foreach (var attr in replacementNode.Attributes)
                {
                    hostNode.SetAttribute(attr.Key, attr.Value);
                }

                replacementMapping[replacementNode] = hostNode;
            }

            // Create new nodes for non-preserved nodes
            foreach (var replacementNode in rule.Replacement.Nodes)
            {
                if (!replacementMapping.ContainsKey(replacementNode))
                {
                    var newNode = new GraphNode<T>(replacementNode.Data, replacementNode.Attributes);
                    Graph.AddNode(newNode);
                    replacementMapping[replacementNode] = newNode;
                }
            }

            // Remove matched nodes that aren't preserved
            foreach (var kvp in match)
            {
                if (!rule.PreservedNodes.ContainsKey(kvp.Key))
                {
                    Graph.RemoveNode(kvp.Value);
                }
            }

            // Add edges from replacement graph
            foreach (var replacementEdge in rule.Replacement.Edges)
            {
                var newFrom = replacementMapping[replacementEdge.From];
                var newTo = replacementMapping[replacementEdge.To];
                var newEdge = new GraphEdge<T>(newFrom, newTo, replacementEdge.Label, replacementEdge.Weight);
                foreach (var attr in replacementEdge.Attributes)
                {
                    newEdge.SetAttribute(attr.Key, attr.Value);
                }
                Graph.AddEdge(newEdge);
            }

            // Call custom callback if provided
            rule.OnApply?.Invoke(Graph, match);
        }

        /// <summary>
        /// Resets the engine with a new initial graph.
        /// </summary>
        public void Reset(Graph<T> newInitialGraph)
        {
            Graph = newInitialGraph ?? throw new ArgumentNullException(nameof(newInitialGraph));
            IterationCount = 0;
        }
    }
}
