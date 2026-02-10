using System;
using System.Collections.Generic;
using System.Linq;

namespace Shizounu.Library.GenerationAlgorithms.GraphGrammars
{
    /// <summary>
    /// Represents a graph production rule: L -> R
    /// Defines a pattern graph (L) that should be replaced with a replacement graph (R).
    /// </summary>
    /// <typeparam name="T">The type of data stored in nodes</typeparam>
    public class GraphProductionRule<T>
    {
        /// <summary>
        /// The left-hand side (pattern) graph to match.
        /// </summary>
        public Graph<T> Pattern { get; }

        /// <summary>
        /// The right-hand side (replacement) graph.
        /// </summary>
        public Graph<T> Replacement { get; }

        /// <summary>
        /// Name/identifier for this rule.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Priority for rule application (higher priority rules are tried first).
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        /// Optional predicate to determine if this rule can be applied in a specific context.
        /// </summary>
        public Func<Graph<T>, Dictionary<GraphNode<T>, GraphNode<T>>, bool> Predicate { get; set; }

        /// <summary>
        /// Optional callback to customize the replacement process.
        /// </summary>
        public Action<Graph<T>, Dictionary<GraphNode<T>, GraphNode<T>>> OnApply { get; set; }

        /// <summary>
        /// Nodes in the pattern that should be preserved (not deleted) during replacement.
        /// Maps pattern nodes to their roles in the replacement.
        /// </summary>
        public Dictionary<GraphNode<T>, GraphNode<T>> PreservedNodes { get; private set; }

        /// <summary>
        /// Creates a new production rule.
        /// </summary>
        public GraphProductionRule(Graph<T> pattern, Graph<T> replacement, string name = "")
        {
            Pattern = pattern ?? throw new ArgumentNullException(nameof(pattern));
            Replacement = replacement ?? throw new ArgumentNullException(nameof(replacement));
            Name = name ?? string.Empty;
            Priority = 0;
            PreservedNodes = new Dictionary<GraphNode<T>, GraphNode<T>>();
        }

        /// <summary>
        /// Marks a node from the pattern as preserved, mapping it to a node in the replacement.
        /// </summary>
        public void PreserveNode(GraphNode<T> patternNode, GraphNode<T> replacementNode)
        {
            if (patternNode == null || replacementNode == null)
                throw new ArgumentNullException();

            if (!Pattern.Nodes.Contains(patternNode))
                throw new ArgumentException("Pattern node not in pattern graph");

            if (!Replacement.Nodes.Contains(replacementNode))
                throw new ArgumentException("Replacement node not in replacement graph");

            PreservedNodes[patternNode] = replacementNode;
        }

        /// <summary>
        /// Checks if this rule can be applied given a match mapping.
        /// </summary>
        public bool CanApply(Graph<T> hostGraph, Dictionary<GraphNode<T>, GraphNode<T>> match)
        {
            if (Predicate == null)
                return true;

            return Predicate(hostGraph, match);
        }

        public override string ToString()
        {
            return $"Rule({Name}, Pattern: {Pattern.NodeCount}N/{Pattern.EdgeCount}E -> Replacement: {Replacement.NodeCount}N/{Replacement.EdgeCount}E)";
        }
    }

    /// <summary>
    /// Helper class for building production rules with a fluent interface.
    /// </summary>
    public class RuleBuilder<T>
    {
        private Graph<T> _pattern;
        private Graph<T> _replacement;
        private string _name;
        private int _priority;
        private readonly List<(GraphNode<T>, GraphNode<T>)> _preserved = new List<(GraphNode<T>, GraphNode<T>)>();
        private Func<Graph<T>, Dictionary<GraphNode<T>, GraphNode<T>>, bool> _predicate;

        public RuleBuilder<T> WithName(string name)
        {
            _name = name;
            return this;
        }

        public RuleBuilder<T> WithPriority(int priority)
        {
            _priority = priority;
            return this;
        }

        public RuleBuilder<T> WithPattern(Graph<T> pattern)
        {
            _pattern = pattern;
            return this;
        }

        public RuleBuilder<T> WithReplacement(Graph<T> replacement)
        {
            _replacement = replacement;
            return this;
        }

        public RuleBuilder<T> Preserve(GraphNode<T> patternNode, GraphNode<T> replacementNode)
        {
            _preserved.Add((patternNode, replacementNode));
            return this;
        }

        public RuleBuilder<T> WithPredicate(Func<Graph<T>, Dictionary<GraphNode<T>, GraphNode<T>>, bool> predicate)
        {
            _predicate = predicate;
            return this;
        }

        public GraphProductionRule<T> Build()
        {
            if (_pattern == null)
                throw new InvalidOperationException("Pattern graph must be set");
            if (_replacement == null)
                throw new InvalidOperationException("Replacement graph must be set");

            var rule = new GraphProductionRule<T>(_pattern, _replacement, _name)
            {
                Priority = _priority,
                Predicate = _predicate
            };

            foreach (var (patternNode, replacementNode) in _preserved)
            {
                rule.PreserveNode(patternNode, replacementNode);
            }

            return rule;
        }
    }
}
