using System;
using System.Collections.Generic;
using System.Linq;

namespace Shizounu.Library.GenerationAlgorithms.GraphGrammars
{
    /// <summary>
    /// Represents a directed graph with nodes and edges.
    /// </summary>
    /// <typeparam name="T">The type of data stored in nodes</typeparam>
    public class Graph<T>
    {
        private readonly List<GraphNode<T>> _nodes = new List<GraphNode<T>>();
        private readonly List<GraphEdge<T>> _edges = new List<GraphEdge<T>>();
        private readonly Dictionary<int, List<GraphEdge<T>>> _outgoingEdges = new Dictionary<int, List<GraphEdge<T>>>();
        private readonly Dictionary<int, List<GraphEdge<T>>> _incomingEdges = new Dictionary<int, List<GraphEdge<T>>>();

        /// <summary>
        /// Gets all nodes in the graph.
        /// </summary>
        public IReadOnlyList<GraphNode<T>> Nodes => _nodes;

        /// <summary>
        /// Gets all edges in the graph.
        /// </summary>
        public IReadOnlyList<GraphEdge<T>> Edges => _edges;

        /// <summary>
        /// Gets the number of nodes in the graph.
        /// </summary>
        public int NodeCount => _nodes.Count;

        /// <summary>
        /// Gets the number of edges in the graph.
        /// </summary>
        public int EdgeCount => _edges.Count;

        /// <summary>
        /// Adds a node to the graph.
        /// </summary>
        public void AddNode(GraphNode<T> node)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            if (!_nodes.Contains(node))
            {
                _nodes.Add(node);
                _outgoingEdges[node.Id] = new List<GraphEdge<T>>();
                _incomingEdges[node.Id] = new List<GraphEdge<T>>();
            }
        }

        /// <summary>
        /// Removes a node from the graph along with all connected edges.
        /// </summary>
        public bool RemoveNode(GraphNode<T> node)
        {
            if (node == null || !_nodes.Contains(node))
                return false;

            // Remove all edges connected to this node
            var edgesToRemove = _edges.Where(e => e.From == node || e.To == node).ToList();
            foreach (var edge in edgesToRemove)
            {
                RemoveEdge(edge);
            }

            _nodes.Remove(node);
            _outgoingEdges.Remove(node.Id);
            _incomingEdges.Remove(node.Id);
            return true;
        }

        /// <summary>
        /// Adds an edge to the graph. Automatically adds nodes if they're not in the graph.
        /// </summary>
        public void AddEdge(GraphEdge<T> edge)
        {
            if (edge == null)
                throw new ArgumentNullException(nameof(edge));

            AddNode(edge.From);
            AddNode(edge.To);

            if (!_edges.Contains(edge))
            {
                _edges.Add(edge);
                _outgoingEdges[edge.From.Id].Add(edge);
                _incomingEdges[edge.To.Id].Add(edge);
            }
        }

        /// <summary>
        /// Removes an edge from the graph.
        /// </summary>
        public bool RemoveEdge(GraphEdge<T> edge)
        {
            if (edge == null || !_edges.Contains(edge))
                return false;

            _edges.Remove(edge);
            _outgoingEdges[edge.From.Id].Remove(edge);
            _incomingEdges[edge.To.Id].Remove(edge);
            return true;
        }

        /// <summary>
        /// Gets all outgoing edges from a node.
        /// </summary>
        public IReadOnlyList<GraphEdge<T>> GetOutgoingEdges(GraphNode<T> node)
        {
            if (node == null || !_outgoingEdges.ContainsKey(node.Id))
                return Array.Empty<GraphEdge<T>>();

            return _outgoingEdges[node.Id];
        }

        /// <summary>
        /// Gets all incoming edges to a node.
        /// </summary>
        public IReadOnlyList<GraphEdge<T>> GetIncomingEdges(GraphNode<T> node)
        {
            if (node == null || !_incomingEdges.ContainsKey(node.Id))
                return Array.Empty<GraphEdge<T>>();

            return _incomingEdges[node.Id];
        }

        /// <summary>
        /// Gets all neighbors of a node (nodes connected by outgoing edges).
        /// </summary>
        public IEnumerable<GraphNode<T>> GetNeighbors(GraphNode<T> node)
        {
            if (node == null || !_outgoingEdges.ContainsKey(node.Id))
                yield break;

            foreach (var edge in _outgoingEdges[node.Id])
            {
                yield return edge.To;
            }
        }

        /// <summary>
        /// Clears all nodes and edges from the graph.
        /// </summary>
        public void Clear()
        {
            _nodes.Clear();
            _edges.Clear();
            _outgoingEdges.Clear();
            _incomingEdges.Clear();
        }

        /// <summary>
        /// Creates a deep copy of this graph.
        /// </summary>
        public Graph<T> Clone()
        {
            var clonedGraph = new Graph<T>();
            var nodeMapping = new Dictionary<int, GraphNode<T>>();

            // Clone nodes
            foreach (var node in _nodes)
            {
                var clonedNode = new GraphNode<T>(node.Data, node.Attributes);
                clonedGraph.AddNode(clonedNode);
                nodeMapping[node.Id] = clonedNode;
            }

            // Clone edges
            foreach (var edge in _edges)
            {
                var clonedEdge = new GraphEdge<T>(
                    nodeMapping[edge.From.Id],
                    nodeMapping[edge.To.Id],
                    edge.Label,
                    edge.Weight
                );
                foreach (var attr in edge.Attributes)
                {
                    clonedEdge.SetAttribute(attr.Key, attr.Value);
                }
                clonedGraph.AddEdge(clonedEdge);
            }

            return clonedGraph;
        }

        public override string ToString()
        {
            return $"Graph(Nodes: {NodeCount}, Edges: {EdgeCount})";
        }
    }
}
