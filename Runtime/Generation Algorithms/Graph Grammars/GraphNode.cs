using System;
using System.Collections.Generic;

namespace Shizounu.Library.GenerationAlgorithms.GraphGrammars
{
    /// <summary>
    /// Represents a node in a graph with optional typed data and attributes.
    /// </summary>
    /// <typeparam name="T">The type of data stored in the node</typeparam>
    public class GraphNode<T>
    {
        private static int _nextId = 0;

        /// <summary>
        /// Unique identifier for this node.
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// The data/label stored in this node.
        /// </summary>
        public T Data { get; set; }

        /// <summary>
        /// Optional string attributes for matching and metadata.
        /// </summary>
        public Dictionary<string, string> Attributes { get; private set; }

        /// <summary>
        /// Creates a new graph node with the specified data.
        /// </summary>
        public GraphNode(T data)
        {
            Id = _nextId++;
            Data = data;
            Attributes = new Dictionary<string, string>();
        }

        /// <summary>
        /// Creates a new graph node with data and attributes.
        /// </summary>
        public GraphNode(T data, Dictionary<string, string> attributes)
        {
            Id = _nextId++;
            Data = data;
            Attributes = new Dictionary<string, string>(attributes);
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
        /// Checks if this node matches another based on data and attributes.
        /// </summary>
        public bool Matches(GraphNode<T> other, bool strictAttributes = false)
        {
            if (other == null)
                return false;

            // Check data equality
            if (!EqualityComparer<T>.Default.Equals(Data, other.Data))
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
                // Non-strict: other must have all attributes of this node
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
            return $"Node({Id}, {Data})";
        }
    }
}
