using System.Collections.Generic;
using UnityEngine;

namespace Shizounu.Library.GameAI.Pathfinding
{
    /// <summary>
    /// Helper to provide nearest tiles for pathfinding queries.
    /// </summary>
    public class PathfindingTileSet : MonoBehaviour
    {
        [SerializeField] private List<PathfindingTileComponent> tiles = new List<PathfindingTileComponent>();

        public IPathfindingTile GetClosestTile(Vector3 position)
        {
            if (tiles == null || tiles.Count == 0)
                return null;

            IPathfindingTile closest = null;
            float bestDist = float.MaxValue;

            for (int i = 0; i < tiles.Count; i++)
            {
                var tile = tiles[i];
                if (tile == null)
                    continue;

                float dist = Vector3.Distance(position, tile.Position);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    closest = tile;
                }
            }

            return closest;
        }
    }
}
