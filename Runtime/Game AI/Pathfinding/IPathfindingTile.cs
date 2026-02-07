using System.Collections.Generic;
using UnityEngine;

public interface IPathfindingTile
{
    Vector3 Position { get; }
    float TraversalCost { get; }
    List<IPathfindingTile> Adjacencies { get; set; }
    float Heuristic(IPathfindingTile goal);
}
