using System.Collections.Generic;
using UnityEngine;

namespace Shizounu.Library.GameAI.Pathfinding
{
    /// <summary>
    /// Basic MonoBehaviour implementation of IPathfindingTile.
    /// Assign adjacencies in the inspector to build a graph.
    /// </summary>
    public class PathfindingTileComponent : MonoBehaviour, IPathfindingTile
    {
        [SerializeField] private float traversalCost = 1f;
        [SerializeField] private List<PathfindingTileComponent> adjacencies = new List<PathfindingTileComponent>();

        public Vector3 Position => transform.position;
        public float TraversalCost => traversalCost;
        public List<IPathfindingTile> Adjacencies { get; set; }

        private void Awake()
        {
            if (Adjacencies == null)
                Adjacencies = new List<IPathfindingTile>();

            Adjacencies.Clear();
            for (int i = 0; i < adjacencies.Count; i++)
            {
                if (adjacencies[i] != null)
                    Adjacencies.Add(adjacencies[i]);
            }
        }

        public float Heuristic(IPathfindingTile goal)
        {
            if (goal == null)
                return 0f;

            return Vector3.Distance(Position, goal.Position);
        }
    }
}
