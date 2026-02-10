using System;
using System.Collections.Generic;
using UnityEngine;

namespace Shizounu.Library.GenerationAlgorithms.WaveFunctionCollapse
{
    [CreateAssetMenu(menuName = "Shizounu/Wave Function Collapse/Tile Set", fileName = "new WfcTileSet")]
    public sealed class WfcTileSetAsset : ScriptableObject
    {
        [Serializable]
        public sealed class TileEntry
        {
            public UnityEngine.Object Tile;
            public float Weight = 1f;
        }

        [Serializable]
        public sealed class AdjacencyRule
        {
            public UnityEngine.Object Tile;
            public int DirectionMask;
            public bool Bidirectional = true;
            public List<UnityEngine.Object> AllowedNeighbors = new List<UnityEngine.Object>();
        }

        [SerializeField] private List<TileEntry> tiles = new List<TileEntry>();
        [SerializeField] private List<AdjacencyRule> adjacencyRules = new List<AdjacencyRule>();
        [SerializeField] private int directionCount = 4;

        public WfcTileSet<UnityEngine.Object> BuildTileSet()
        {
            if (tiles == null || tiles.Count == 0)
                throw new InvalidOperationException("Tile set asset has no tiles");

            var set = new WfcTileSet<UnityEngine.Object>(directionCount);

            for (int i = 0; i < tiles.Count; i++)
            {
                TileEntry entry = tiles[i];
                if (entry == null || entry.Tile == null)
                    throw new InvalidOperationException("Tile entry is null or missing tile reference");
                if (entry.Weight <= 0f)
                    throw new InvalidOperationException("Tile weight must be greater than zero");

                set.AddTile(entry.Tile, entry.Weight);
            }

            if (adjacencyRules != null)
            {
                for (int i = 0; i < adjacencyRules.Count; i++)
                {
                    AdjacencyRule rule = adjacencyRules[i];
                    if (rule == null || rule.Tile == null)
                        throw new InvalidOperationException("Adjacency rule is null or missing tile reference");

                    if (rule.AllowedNeighbors == null || rule.AllowedNeighbors.Count == 0)
                        continue;

                    int directionMask = ClampMaskToDirectionCount(rule.DirectionMask, directionCount);
                    if (directionMask == 0)
                        continue;

                    for (int directionIndex = 0; directionIndex < directionCount; directionIndex++)
                    {
                        if ((directionMask & (1 << directionIndex)) == 0)
                            continue;

                        for (int n = 0; n < rule.AllowedNeighbors.Count; n++)
                        {
                            UnityEngine.Object neighbor = rule.AllowedNeighbors[n];
                            if (neighbor == null)
                                continue;

                            if (rule.Bidirectional)
                            {
                                int oppositeIndex = GetOppositeDirectionIndex(directionIndex, directionCount);
                                set.AllowAdjacencyBidirectional(set.GetIndex(rule.Tile), directionIndex, set.GetIndex(neighbor), oppositeIndex);
                            }
                            else
                            {
                                set.AllowAdjacency(set.GetIndex(rule.Tile), directionIndex, set.GetIndex(neighbor));
                            }
                        }
                    }
                }
            }

            return set;
        }

        private static int ClampMaskToDirectionCount(int mask, int directionCount)
        {
            if (directionCount >= 31)
                return mask;

            int maxMask = (1 << directionCount) - 1;
            return mask & maxMask;
        }

        private static int GetOppositeDirectionIndex(int directionIndex, int directionCount)
        {
            if (directionCount == 4)
                return (int)Direction2DUtility.Opposite((Direction2D)directionIndex);

            if (directionCount == 6)
                return (int)Direction3DUtility.Opposite((Direction3D)directionIndex);

            throw new InvalidOperationException("Opposite direction is only defined for 4 or 6 directions");
        }
    }
}
