using System;
using System.Collections.Generic;

namespace Shizounu.Library.GenerationAlgorithms.GraphGrammars
{
    /// <summary>
    /// Represents a directed edge between two nodes in a graph.
    /// </summary>
    /// <typeparam name="T">The type of data stored in nodes</typeparam>
    public class GraphEdge<T>
    {
        /// <summary>
        /// The source node of this edge.
        /// </summary>
        public GraphNode<T> From { get; }

        /// <summary>
        /// The target node of this edge.
        /// </summary>
        public GraphNode<T> To { get; }

        /// <summary>
        /// Optional label for this edge (e.g., "parent", "connects_to").
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// Optional numeric weight for this edge.
        /// </summary>
        public float Weight { get; set; }

        /// <summary>
        /// Optional attributes for matching and metadata.
        /// </summary>
        public Dictionary<string, string> Attributes { get; private set; }

        /// <summary>
        /// Creates a new edge between two nodes.
        /// </summary>
        public GraphEdge(GraphNode<T> from, GraphNode<T> to, string label = "", float weight = 1.0f)
        {
            From = from ?? throw new ArgumentNullException(nameof(from));
            To = to ?? throw new ArgumentNullException(nameof(to));
            Label = label ?? string.Empty;
            Weight = weight;
            Attributes = new Dictionary<string, string>();
        }

        /// <summary>
        /// Sets an attribute value.
        /// </summary>
        public void SetAttribute(string key, string value)
        {
            Attributes[key] = value;
        }

        /// <summary>
        /// Gets an attribute value, or null if not found.
        /// </summary>
        public string GetAttribute(string key)
        {
            return Attributes.TryGetValue(key, out var value) ? value : null;
        }

        /// <summary>
        /// Checks if this edge matches another based on label and attributes.
        /// </summary>
        public bool Matches(GraphEdge<T> other, bool strictAttributes = false)
        {
            if (other == null)
                return false;

            if (Label != other.Label)
                return false;

            // Check attributes
            if (strictAttributes)
            {
                if (Attributes.Count != other.Attributes.Count)
                    return false;

                foreach (var kvp in Attributes)
                {
                    if (!other.Attributes.TryGetValue(kvp.Key, out var value) || value != kvp.Value)
                        return false;
                }
            }
            else
            {
                // Non-strict: other must have all attributes of this edge
                foreach (var kvp in Attributes)
                {
                    if (!other.Attributes.TryGetValue(kvp.Key, out var value) || value != kvp.Value)
                        return false;
                }
            }

            return true;
        }

        public override string ToString()
        {
            return $"Edge({From.Id} -> {To.Id}, {Label})";
        }
    }
}
