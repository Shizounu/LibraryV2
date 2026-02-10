using System.Collections.Generic;
using UnityEngine;
using Shizounu.Library.GenerationAlgorithms.GraphGrammars;

namespace Shizounu.Library.GenerationAlgorithms.GraphGrammars.Visualization
{
    /// <summary>
    /// Visualizes a graph grammar in Unity's Scene view and Game view.
    /// Supports both runtime and editor visualization with Gizmos.
    /// </summary>
    public class GraphGrammarVisualizer : MonoBehaviour
    {
        [Header("Visualization Settings")]
        [Tooltip("The graph to visualize")]
        public Graph<string> Graph;

        [Tooltip("Spacing between nodes for layout")]
        [SerializeField] private float nodeSpacing = 2f;

        [Tooltip("Size of node spheres")]
        [SerializeField] private float nodeSize = 0.3f;

        [Tooltip("Thickness of edge lines")]
        [SerializeField] private float edgeThickness = 0.05f;

        [Tooltip("Color for normal nodes")]
        [SerializeField] private Color normalNodeColor = Color.cyan;

        [Tooltip("Color for special nodes (entrance, treasure, etc.)")]
        [SerializeField] private Color specialNodeColor = Color.yellow;

        [Tooltip("Color for edges")]
        [SerializeField] private Color edgeColor = Color.white;

        [Tooltip("Show node labels in Scene view")]
        [SerializeField] private bool showLabels = true;

        [Tooltip("Show node IDs")]
        [SerializeField] private bool showNodeIds = true;

        [Tooltip("Show edge labels")]
        [SerializeField] private bool showEdgeLabels = true;

        [Header("Layout Settings")]
        [Tooltip("Layout algorithm to use")]
        [SerializeField] private LayoutType layoutType = LayoutType.ForceDirected;

        [Tooltip("Center position for the graph")]
        [SerializeField] private Vector3 centerPosition = Vector3.zero;

        [Tooltip("Use 3D layout (otherwise 2D in XY plane)")]
        [SerializeField] private bool use3DLayout = false;

        // Node positions for layout
        private Dictionary<int, Vector3> _nodePositions = new Dictionary<int, Vector3>();
        private bool _layoutDirty = true;

        public enum LayoutType
        {
            Grid,
            Circular,
            ForceDirected,
            Hierarchical
        }

        private void OnValidate()
        {
            _layoutDirty = true;
        }

        /// <summary>
        /// Updates the graph to visualize.
        /// </summary>
        public void SetGraph(Graph<string> graph)
        {
            Graph = graph;
            _layoutDirty = true;
        }

        /// <summary>
        /// Forces a recalculation of the layout.
        /// </summary>
        public void RefreshLayout()
        {
            _layoutDirty = true;
        }

        private void Update()
        {
            if (_layoutDirty && Graph != null)
            {
                CalculateLayout();
                _layoutDirty = false;
            }
        }

        private void OnDrawGizmos()
        {
            if (Graph == null || Graph.NodeCount == 0)
                return;

            if (_layoutDirty)
            {
                CalculateLayout();
                _layoutDirty = false;
            }

            DrawGraph();
        }

        private void CalculateLayout()
        {
            if (Graph == null || Graph.NodeCount == 0)
                return;

            _nodePositions.Clear();

            switch (layoutType)
            {
                case LayoutType.Grid:
                    CalculateGridLayout();
                    break;
                case LayoutType.Circular:
                    CalculateCircularLayout();
                    break;
                case LayoutType.ForceDirected:
                    CalculateForceDirectedLayout();
                    break;
                case LayoutType.Hierarchical:
                    CalculateHierarchicalLayout();
                    break;
            }
        }

        private void CalculateGridLayout()
        {
            int gridSize = Mathf.CeilToInt(Mathf.Sqrt(Graph.NodeCount));
            int index = 0;

            foreach (var node in Graph.Nodes)
            {
                int x = index % gridSize;
                int y = index / gridSize;
                
                Vector3 pos = centerPosition + new Vector3(
                    (x - gridSize / 2f) * nodeSpacing,
                    use3DLayout ? 0 : (y - gridSize / 2f) * nodeSpacing,
                    use3DLayout ? (y - gridSize / 2f) * nodeSpacing : 0
                );

                _nodePositions[node.Id] = pos;
                index++;
            }
        }

        private void CalculateCircularLayout()
        {
            int count = Graph.NodeCount;
            float angleStep = 360f / count;
            float radius = nodeSpacing * count / (2f * Mathf.PI);

            int index = 0;
            foreach (var node in Graph.Nodes)
            {
                float angle = angleStep * index * Mathf.Deg2Rad;
                
                Vector3 pos = centerPosition + new Vector3(
                    Mathf.Cos(angle) * radius,
                    use3DLayout ? 0 : Mathf.Sin(angle) * radius,
                    use3DLayout ? Mathf.Sin(angle) * radius : 0
                );

                _nodePositions[node.Id] = pos;
                index++;
            }
        }

        private void CalculateForceDirectedLayout()
        {
            // Simple spring-embedder algorithm
            var nodes = Graph.Nodes;
            if (nodes.Count == 0) return;

            // Initialize random positions if needed
            foreach (var node in nodes)
            {
                if (!_nodePositions.ContainsKey(node.Id))
                {
                    Vector3 randomPos = centerPosition + Random.insideUnitSphere * nodeSpacing * 2;
                    if (!use3DLayout)
                        randomPos.z = 0;
                    _nodePositions[node.Id] = randomPos;
                }
            }

            // Run iterations
            for (int iter = 0; iter < 50; iter++)
            {
                var forces = new Dictionary<int, Vector3>();
                
                // Initialize forces
                foreach (var node in nodes)
                    forces[node.Id] = Vector3.zero;

                // Repulsive forces between all nodes
                for (int i = 0; i < nodes.Count; i++)
                {
                    for (int j = i + 1; j < nodes.Count; j++)
                    {
                        var nodeA = nodes[i];
                        var nodeB = nodes[j];
                        
                        Vector3 delta = _nodePositions[nodeA.Id] - _nodePositions[nodeB.Id];
                        float distance = delta.magnitude;
                        
                        if (distance < 0.01f)
                            delta = Random.insideUnitSphere * 0.1f;
                        
                        if (distance > 0)
                        {
                            Vector3 force = delta.normalized * (nodeSpacing * nodeSpacing / distance);
                            forces[nodeA.Id] += force;
                            forces[nodeB.Id] -= force;
                        }
                    }
                }

                // Attractive forces for edges
                foreach (var edge in Graph.Edges)
                {
                    Vector3 delta = _nodePositions[edge.To.Id] - _nodePositions[edge.From.Id];
                    float distance = delta.magnitude;
                    
                    Vector3 force = delta.normalized * (distance - nodeSpacing) * 0.5f;
                    forces[edge.From.Id] += force;
                    forces[edge.To.Id] -= force;
                }

                // Apply forces with damping
                float damping = 0.85f;
                foreach (var node in nodes)
                {
                    Vector3 newPos = _nodePositions[node.Id] + forces[node.Id] * 0.01f;
                    if (!use3DLayout)
                        newPos.z = 0;
                    
                    // Pull toward center
                    Vector3 toCenter = (centerPosition - newPos) * 0.05f;
                    newPos += toCenter;
                    
                    _nodePositions[node.Id] = Vector3.Lerp(_nodePositions[node.Id], newPos, damping);
                }
            }
        }

        private void CalculateHierarchicalLayout()
        {
            // Find root nodes (no incoming edges)
            var roots = new List<GraphNode<string>>();
            foreach (var node in Graph.Nodes)
            {
                if (Graph.GetIncomingEdges(node).Count == 0)
                    roots.Add(node);
            }

            if (roots.Count == 0 && Graph.NodeCount > 0)
                roots.Add(Graph.Nodes[0]);

            var levels = new Dictionary<int, int>();
            var visited = new HashSet<int>();
            
            // BFS to assign levels
            var queue = new Queue<(GraphNode<string> node, int level)>();
            foreach (var root in roots)
            {
                queue.Enqueue((root, 0));
                visited.Add(root.Id);
            }

            int maxLevel = 0;
            while (queue.Count > 0)
            {
                var (node, level) = queue.Dequeue();
                levels[node.Id] = level;
                maxLevel = Mathf.Max(maxLevel, level);

                foreach (var edge in Graph.GetOutgoingEdges(node))
                {
                    if (!visited.Contains(edge.To.Id))
                    {
                        visited.Add(edge.To.Id);
                        queue.Enqueue((edge.To, level + 1));
                    }
                }
            }

            // Count nodes per level
            var nodesPerLevel = new Dictionary<int, int>();
            for (int i = 0; i <= maxLevel; i++)
                nodesPerLevel[i] = 0;

            foreach (var node in Graph.Nodes)
            {
                if (levels.ContainsKey(node.Id))
                    nodesPerLevel[levels[node.Id]]++;
            }

            // Position nodes
            var indexPerLevel = new Dictionary<int, int>();
            for (int i = 0; i <= maxLevel; i++)
                indexPerLevel[i] = 0;

            foreach (var node in Graph.Nodes)
            {
                if (!levels.ContainsKey(node.Id))
                    levels[node.Id] = maxLevel + 1;

                int level = levels[node.Id];
                int index = indexPerLevel[level];
                int total = nodesPerLevel[level];

                float x = (index - total / 2f) * nodeSpacing;
                float y = -level * nodeSpacing;

                Vector3 pos = centerPosition + new Vector3(
                    x,
                    use3DLayout ? 0 : y,
                    use3DLayout ? y : 0
                );

                _nodePositions[node.Id] = pos;
                indexPerLevel[level]++;
            }
        }

        private void DrawGraph()
        {
            // Draw edges first (so they appear behind nodes)
            foreach (var edge in Graph.Edges)
            {
                if (_nodePositions.TryGetValue(edge.From.Id, out var fromPos) &&
                    _nodePositions.TryGetValue(edge.To.Id, out var toPos))
                {
                    Gizmos.color = edgeColor;
                    
                    // Draw arrow
                    Vector3 direction = (toPos - fromPos).normalized;
                    Vector3 arrowStart = fromPos + direction * nodeSize;
                    Vector3 arrowEnd = toPos - direction * nodeSize;
                    
                    Gizmos.DrawLine(arrowStart, arrowEnd);
                    
                    // Draw arrowhead
                    Vector3 arrowTip = arrowEnd;
                    Vector3 perpendicular = Vector3.Cross(direction, use3DLayout ? Vector3.up : Vector3.forward);
                    Vector3 arrowBase = arrowTip - direction * 0.2f;
                    
                    Gizmos.DrawLine(arrowTip, arrowBase + perpendicular * 0.1f);
                    Gizmos.DrawLine(arrowTip, arrowBase - perpendicular * 0.1f);

                    // Draw edge label
                    if (showEdgeLabels && !string.IsNullOrEmpty(edge.Label))
                    {
                        Vector3 midPoint = (fromPos + toPos) / 2f;
                        DrawLabel(midPoint + Vector3.up * 0.3f, edge.Label, Color.gray);
                    }
                }
            }

            // Draw nodes
            foreach (var node in Graph.Nodes)
            {
                if (_nodePositions.TryGetValue(node.Id, out var pos))
                {
                    // Determine color based on attributes
                    Color nodeColor = normalNodeColor;
                    string nodeType = node.GetAttribute("type");
                    
                    if (nodeType == "entrance" || nodeType == "treasure" || nodeType == "boss")
                        nodeColor = specialNodeColor;
                    else if (node.Data == "Corridor")
                        nodeColor = Color.gray;

                    Gizmos.color = nodeColor;
                    Gizmos.DrawSphere(pos, nodeSize);

                    // Draw outline
                    Gizmos.color = Color.black;
                    Gizmos.DrawWireSphere(pos, nodeSize + 0.02f);

                    // Draw label
                    if (showLabels)
                    {
                        string label = node.Data;
                        if (showNodeIds)
                            label += $" ({node.Id})";
                        
                        if (!string.IsNullOrEmpty(nodeType))
                            label += $"\n[{nodeType}]";

                        DrawLabel(pos + Vector3.up * (nodeSize + 0.2f), label, nodeColor);
                    }
                }
            }
        }

        private void DrawLabel(Vector3 position, string text, Color color)
        {
#if UNITY_EDITOR
            var style = new GUIStyle();
            style.normal.textColor = color;
            style.alignment = TextAnchor.MiddleCenter;
            style.fontSize = 10;
            UnityEditor.Handles.Label(position, text, style);
#endif
        }

        /// <summary>
        /// Gets the world position of a node for external use.
        /// </summary>
        public Vector3 GetNodePosition(int nodeId)
        {
            return _nodePositions.TryGetValue(nodeId, out var pos) ? pos : centerPosition;
        }
    }
}
