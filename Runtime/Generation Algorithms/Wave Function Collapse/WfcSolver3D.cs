using System;
using System.Collections.Generic;

namespace Shizounu.Library.GenerationAlgorithms.WaveFunctionCollapse
{
    public sealed class WfcSolver3D<T>
    {
        private readonly int _width;
        private readonly int _height;
        private readonly int _depth;
        private readonly int _cellCount;
        private readonly WfcTileSet<T> _tileSet;
        private readonly int _tileCount;
        private readonly bool[][] _possible;
        private readonly int[] _possibleCount;
        private readonly float[] _weights;
        private readonly Random _random;
        private readonly Queue<int> _propagationQueue = new Queue<int>();

        public int Width => _width;
        public int Height => _height;
        public int Depth => _depth;
        public WfcTileSet<T> TileSet => _tileSet;

        public WfcSolver3D(int width, int height, int depth, WfcTileSet<T> tileSet, int? seed = null)
        {
            if (width <= 0)
                throw new ArgumentOutOfRangeException(nameof(width));
            if (height <= 0)
                throw new ArgumentOutOfRangeException(nameof(height));
            if (depth <= 0)
                throw new ArgumentOutOfRangeException(nameof(depth));
            if (tileSet == null)
                throw new ArgumentNullException(nameof(tileSet));
            if (tileSet.Count == 0)
                throw new ArgumentException("Tile set is empty", nameof(tileSet));
            if (tileSet.DirectionCount != 6)
                throw new ArgumentException("3D solver requires tile set with 6 directions", nameof(tileSet));

            _width = width;
            _height = height;
            _depth = depth;
            _cellCount = width * height * depth;
            _tileSet = tileSet;
            _tileCount = tileSet.Count;
            _weights = new float[_tileCount];
            for (int i = 0; i < _tileCount; i++)
                _weights[i] = tileSet.Weights[i];

            _possible = new bool[_cellCount][];
            _possibleCount = new int[_cellCount];

            _random = seed.HasValue ? new Random(seed.Value) : new Random();

            Reset();
        }

        public void Reset()
        {
            for (int i = 0; i < _cellCount; i++)
            {
                bool[] options = new bool[_tileCount];
                for (int t = 0; t < _tileCount; t++)
                    options[t] = true;

                _possible[i] = options;
                _possibleCount[i] = _tileCount;
            }

            _propagationQueue.Clear();
        }

        public bool SetCell(int x, int y, int z, int tileIndex)
        {
            ValidateCoordinates(x, y, z);
            ValidateTileIndex(tileIndex);

            int index = ToIndex(x, y, z);
            bool[] options = _possible[index];

            if (!options[tileIndex])
                return false;

            if (options[tileIndex] && _possibleCount[index] == 1)
                return true;

            for (int t = 0; t < _tileCount; t++)
                options[t] = t == tileIndex;

            _possibleCount[index] = 1;
            _propagationQueue.Enqueue(index);

            return Propagate();
        }

        public bool SetCell(int x, int y, int z, T tile)
        {
            return SetCell(x, y, z, _tileSet.GetIndex(tile));
        }

        public WfcResult Step()
        {
            int cell = FindCellWithLowestEntropy();
            if (cell == -1)
                return WfcResult.Success;

            int chosenTile = ChooseTile(cell);
            bool[] options = _possible[cell];
            for (int t = 0; t < _tileCount; t++)
                options[t] = t == chosenTile;

            _possibleCount[cell] = 1;
            _propagationQueue.Enqueue(cell);

            if (!Propagate())
                return WfcResult.Contradiction;

            return WfcResult.InProgress;
        }

        public WfcResult Run(int maxSteps = int.MaxValue)
        {
            if (maxSteps <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxSteps));

            for (int i = 0; i < maxSteps; i++)
            {
                WfcResult result = Step();
                if (result != WfcResult.InProgress)
                    return result;
            }

            return WfcResult.InProgress;
        }

        public bool TryGetCollapsedGrid(out T[,,] grid)
        {
            if (!TryGetCollapsedIndices(out int[,,] indices))
            {
                grid = null;
                return false;
            }

            grid = new T[_width, _height, _depth];
            for (int z = 0; z < _depth; z++)
            {
                for (int y = 0; y < _height; y++)
                {
                    for (int x = 0; x < _width; x++)
                    {
                        int index = indices[x, y, z];
                        grid[x, y, z] = _tileSet.Tiles[index];
                    }
                }
            }

            return true;
        }

        public bool TryGetCollapsedIndices(out int[,,] grid)
        {
            grid = new int[_width, _height, _depth];
            for (int z = 0; z < _depth; z++)
            {
                for (int y = 0; y < _height; y++)
                {
                    for (int x = 0; x < _width; x++)
                    {
                        int index = ToIndex(x, y, z);
                        if (_possibleCount[index] != 1)
                        {
                            grid = null;
                            return false;
                        }

                        int tileIndex = GetCollapsedTile(index);
                        grid[x, y, z] = tileIndex;
                    }
                }
            }

            return true;
        }

        private bool Propagate()
        {
            while (_propagationQueue.Count > 0)
            {
                int cell = _propagationQueue.Dequeue();
                (int cellX, int cellY, int cellZ) = FromIndex(cell);

                foreach (Direction3D direction in Direction3DUtility.All)
                {
                    (int dx, int dy, int dz) = Direction3DUtility.Offset(direction);
                    int nx = cellX + dx;
                    int ny = cellY + dy;
                    int nz = cellZ + dz;

                    if (nx < 0 || nx >= _width || ny < 0 || ny >= _height || nz < 0 || nz >= _depth)
                        continue;

                    int neighbor = ToIndex(nx, ny, nz);
                    if (ReduceNeighborOptions(cell, neighbor, direction))
                    {
                        if (_possibleCount[neighbor] == 0)
                            return false;

                        _propagationQueue.Enqueue(neighbor);
                    }
                }
            }

            return true;
        }

        private bool ReduceNeighborOptions(int cell, int neighbor, Direction3D direction)
        {
            bool changed = false;
            bool[] neighborOptions = _possible[neighbor];

            for (int t = 0; t < _tileCount; t++)
            {
                if (!neighborOptions[t])
                    continue;

                if (!IsNeighborTileAllowed(cell, t, direction))
                {
                    neighborOptions[t] = false;
                    _possibleCount[neighbor]--;
                    changed = true;
                }
            }

            return changed;
        }

        private bool IsNeighborTileAllowed(int cell, int neighborTile, Direction3D direction)
        {
            bool[] options = _possible[cell];
            IReadOnlyList<HashSet<int>[]> allowed = _tileSet.Allowed;

            for (int t = 0; t < _tileCount; t++)
            {
                if (!options[t])
                    continue;

                if (allowed[t][(int)direction].Contains(neighborTile))
                    return true;
            }

            return false;
        }

        private int FindCellWithLowestEntropy()
        {
            int bestIndex = -1;
            int bestCount = int.MaxValue;

            for (int i = 0; i < _cellCount; i++)
            {
                int count = _possibleCount[i];
                if (count <= 1)
                    continue;

                if (count < bestCount)
                {
                    bestCount = count;
                    bestIndex = i;
                }
                else if (count == bestCount && bestIndex != -1)
                {
                    if (_random.NextDouble() < 0.5)
                        bestIndex = i;
                }
            }

            return bestIndex;
        }

        private int ChooseTile(int cell)
        {
            bool[] options = _possible[cell];
            float totalWeight = 0f;

            for (int t = 0; t < _tileCount; t++)
            {
                if (options[t])
                    totalWeight += _weights[t];
            }

            if (totalWeight <= 0f)
                return GetCollapsedTile(cell);

            double roll = _random.NextDouble() * totalWeight;
            float cumulative = 0f;

            for (int t = 0; t < _tileCount; t++)
            {
                if (!options[t])
                    continue;

                cumulative += _weights[t];
                if (roll <= cumulative)
                    return t;
            }

            return GetCollapsedTile(cell);
        }

        private int GetCollapsedTile(int cell)
        {
            bool[] options = _possible[cell];
            for (int t = 0; t < _tileCount; t++)
            {
                if (options[t])
                    return t;
            }

            return -1;
        }

        private int ToIndex(int x, int y, int z)
        {
            return x + y * _width + z * _width * _height;
        }

        private (int x, int y, int z) FromIndex(int index)
        {
            int x = index % _width;
            int y = (index / _width) % _height;
            int z = index / (_width * _height);
            return (x, y, z);
        }

        private void ValidateCoordinates(int x, int y, int z)
        {
            if (x < 0 || x >= _width)
                throw new ArgumentOutOfRangeException(nameof(x));
            if (y < 0 || y >= _height)
                throw new ArgumentOutOfRangeException(nameof(y));
            if (z < 0 || z >= _depth)
                throw new ArgumentOutOfRangeException(nameof(z));
        }

        private void ValidateTileIndex(int tileIndex)
        {
            if (tileIndex < 0 || tileIndex >= _tileCount)
                throw new ArgumentOutOfRangeException(nameof(tileIndex));
        }
    }
}
