using System;
using System.Collections.Generic;

namespace Shizounu.Library.GenerationAlgorithms.WaveFunctionCollapse
{
    public sealed class WfcTileSet<T>
    {
        private readonly List<T> _tiles = new List<T>();
        private readonly List<float> _weights = new List<float>();
        private readonly List<HashSet<int>[]> _allowed = new List<HashSet<int>[]>();
        private readonly Dictionary<T, int> _indices;

        private readonly int _directionCount;

        public WfcTileSet() : this(4, null)
        {
        }

        public WfcTileSet(int directionCount) : this(directionCount, null)
        {
        }

        public WfcTileSet(IEqualityComparer<T> comparer) : this(4, comparer)
        {
        }

        public WfcTileSet(int directionCount, IEqualityComparer<T> comparer)
        {
            if (directionCount < 1)
                throw new ArgumentOutOfRangeException(nameof(directionCount), "Direction count must be at least 1");

            _directionCount = directionCount;
            _indices = new Dictionary<T, int>(comparer ?? EqualityComparer<T>.Default);
        }

        public int Count => _tiles.Count;
        public IReadOnlyList<T> Tiles => _tiles;
        public IReadOnlyList<float> Weights => _weights;
        internal IReadOnlyList<HashSet<int>[]> Allowed => _allowed;
        public int DirectionCount => _directionCount;

        public int AddTile(T tile, float weight = 1f)
        {
            if (_indices.ContainsKey(tile))
                throw new ArgumentException("Tile already exists in set", nameof(tile));

            if (weight <= 0f)
                throw new ArgumentOutOfRangeException(nameof(weight), "Weight must be greater than zero");

            int index = _tiles.Count;
            _tiles.Add(tile);
            _weights.Add(weight);
            _indices.Add(tile, index);

            HashSet<int>[] perDirection = new HashSet<int>[_directionCount];
            for (int i = 0; i < _directionCount; i++)
                perDirection[i] = new HashSet<int>();

            _allowed.Add(perDirection);

            return index;
        }

        public int GetIndex(T tile)
        {
            if (!_indices.TryGetValue(tile, out int index))
                throw new KeyNotFoundException("Tile not found in set");

            return index;
        }

        public void AllowAdjacency(int tileIndex, Direction2D direction, int neighborIndex)
        {
            ValidateIndex(tileIndex);
            ValidateIndex(neighborIndex);
            _allowed[tileIndex][(int)direction].Add(neighborIndex);
        }

        public void AllowAdjacency(int tileIndex, int directionIndex, int neighborIndex)
        {
            ValidateIndex(tileIndex);
            ValidateIndex(neighborIndex);
            ValidateDirectionIndex(directionIndex);
            _allowed[tileIndex][directionIndex].Add(neighborIndex);
        }

        public void AllowAdjacency(T tile, Direction2D direction, T neighbor)
        {
            AllowAdjacency(GetIndex(tile), direction, GetIndex(neighbor));
        }

        public void AllowAdjacencyBidirectional(T tile, Direction2D direction, T neighbor)
        {
            int tileIndex = GetIndex(tile);
            int neighborIndex = GetIndex(neighbor);
            AllowAdjacency(tileIndex, direction, neighborIndex);
            AllowAdjacency(neighborIndex, Direction2DUtility.Opposite(direction), tileIndex);
        }

        public void AllowAdjacencyBidirectional(int tileIndex, int directionIndex, int neighborIndex, int oppositeDirectionIndex)
        {
            AllowAdjacency(tileIndex, directionIndex, neighborIndex);
            AllowAdjacency(neighborIndex, oppositeDirectionIndex, tileIndex);
        }

        private void ValidateIndex(int index)
        {
            if (index < 0 || index >= _tiles.Count)
                throw new ArgumentOutOfRangeException(nameof(index));
        }

        private void ValidateDirectionIndex(int directionIndex)
        {
            if (directionIndex < 0 || directionIndex >= _directionCount)
                throw new ArgumentOutOfRangeException(nameof(directionIndex));
        }
    }
}
