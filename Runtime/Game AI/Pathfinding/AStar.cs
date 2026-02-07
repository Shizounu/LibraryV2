using System;
using System.Collections.Generic;
using UnityEngine;
using Shizounu.Library.Utility;
namespace Shizounu.Library.GameAI.Pathfinding
{
    public static class AStar
    {
        /// <summary>
        /// Find a path from start to goal using A* algorithm.
        /// </summary>
        /// <param name="start">Starting tile</param>
        /// <param name="goal">Goal tile</param>
        /// <returns>List of tiles representing the path, or null if no path exists</returns>
        public static List<IPathfindingTile> FindPath(IPathfindingTile start, IPathfindingTile goal)
        {
            if (start == null || goal == null)
                return null;

            if (start == goal)
                return new List<IPathfindingTile> { start };

            // Priority queue for open set (tiles to be evaluated)
            var openSet = new PriorityQueue<PathNode>();

            // Hash set for fast lookup of tiles in open set
            var openSetLookup = new HashSet<IPathfindingTile>();

            // Closed set (tiles already evaluated)
            var closedSet = new HashSet<IPathfindingTile>();

            // Track g-scores and parent references
            var gScores = new Dictionary<IPathfindingTile, float>();
            var cameFrom = new Dictionary<IPathfindingTile, IPathfindingTile>();

            // Initialize start node
            gScores[start] = 0;
            float fScore = start.Heuristic(goal);
            openSet.Enqueue(new PathNode(start, 0, fScore));
            openSetLookup.Add(start);

            while (openSet.Count > 0)
            {
                // Get tile with lowest f-score
                PathNode current = openSet.Dequeue();
                IPathfindingTile currentTile = current.Tile;
                openSetLookup.Remove(currentTile);

                // Check if we reached the goal
                if (currentTile == goal)
                {
                    return ReconstructPath(cameFrom, currentTile);
                }

                closedSet.Add(currentTile);

                // Evaluate neighbors
                if (currentTile.Adjacencies == null || currentTile.Adjacencies.Count == 0)
                    continue;

                foreach (var neighbor in currentTile.Adjacencies)
                {
                    if (neighbor == null || closedSet.Contains(neighbor))
                        continue;

                    // Calculate tentative g-score
                    float tentativeGScore = gScores[currentTile] + neighbor.TraversalCost;

                    // Check if this path to neighbor is better than any previous one
                    bool isInOpenSet = openSetLookup.Contains(neighbor);

                    if (!gScores.ContainsKey(neighbor) || tentativeGScore < gScores[neighbor])
                    {
                        // This path is the best so far, record it
                        cameFrom[neighbor] = currentTile;
                        gScores[neighbor] = tentativeGScore;
                        float fScoreNeighbor = tentativeGScore + neighbor.Heuristic(goal);

                        if (!isInOpenSet)
                        {
                            openSet.Enqueue(new PathNode(neighbor, tentativeGScore, fScoreNeighbor));
                            openSetLookup.Add(neighbor);
                        }
                    }
                }
            }

            // No path found
            return null;
        }

        /// <summary>
        /// Reconstruct the path by following parent references.
        /// </summary>
        private static List<IPathfindingTile> ReconstructPath(Dictionary<IPathfindingTile, IPathfindingTile> cameFrom, IPathfindingTile current)
        {
            var path = new List<IPathfindingTile> { current };

            while (cameFrom.ContainsKey(current))
            {
                current = cameFrom[current];
                path.Add(current);
            }

            path.Reverse();
            return path;
        }

        /// <summary>
        /// Node wrapper for priority queue containing tile and scores.
        /// </summary>
        private class PathNode : IComparable<PathNode>
        {
            public IPathfindingTile Tile { get; }
            public float GScore { get; }
            public float FScore { get; }

            public PathNode(IPathfindingTile tile, float gScore, float fScore)
            {
                Tile = tile;
                GScore = gScore;
                FScore = fScore;
            }

            public int CompareTo(PathNode other)
            {
                // Lower f-score has higher priority
                return FScore.CompareTo(other.FScore);
            }
        }

    }
}