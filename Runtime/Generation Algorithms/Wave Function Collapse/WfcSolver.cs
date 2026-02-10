using System;
using System.Collections.Generic;

namespace Shizounu.Library.GenerationAlgorithms.WaveFunctionCollapse
{
    public enum WfcResult
    {
        InProgress,
        Success,
        Contradiction
    }

    public sealed class WfcSolver<T>
    {
        private readonly int _width;
        private readonly int _height;
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
        public WfcTileSet<T> TileSet => _tileSet;

        public WfcSolver(int width, int height, WfcTileSet<T> tileSet, int? seed = null)
        {
            if (width <= 0)
                throw new ArgumentOutOfRangeException(nameof(width));
            if (height <= 0)
                throw new ArgumentOutOfRangeException(nameof(height));
            if (tileSet == null)
                throw new ArgumentNullException(nameof(tileSet));
            if (tileSet.Count == 0)
                throw new ArgumentException("Tile set is empty", nameof(tileSet));

            _width = width;
            _height = height;
            _cellCount = width * height;
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

        public bool SetCell(int x, int y, int tileIndex)
        {
            ValidateCoordinates(x, y);
            ValidateTileIndex(tileIndex);

            int index = ToIndex(x, y);
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

        public bool SetCell(int x, int y, T tile)
        {
            return SetCell(x, y, _tileSet.GetIndex(tile));
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

        public bool TryGetCollapsedGrid(out T[,] grid)
        {
            if (!TryGetCollapsedIndices(out int[,] indices))
            {
                grid = null;
                return false;
            }

            grid = new T[_width, _height];
            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    int index = indices[x, y];
                    grid[x, y] = _tileSet.Tiles[index];
                }
            }

            return true;
        }

        public bool TryGetCollapsedIndices(out int[,] grid)
        {
            grid = new int[_width, _height];
            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    int index = ToIndex(x, y);
                    if (_possibleCount[index] != 1)
                    {
                        grid = null;
                        return false;
                    }

                    int tileIndex = GetCollapsedTile(index);
                    grid[x, y] = tileIndex;
                }
            }

            return true;
        }

        private bool Propagate()
        {
            while (_propagationQueue.Count > 0)
            {
                int cell = _propagationQueue.Dequeue();
                int cellX = cell % _width;
                int cellY = cell / _width;

                foreach (Direction2D direction in Direction2DUtility.All)
                {
                    (int dx, int dy) = Direction2DUtility.Offset(direction);
                    int nx = cellX + dx;
                    int ny = cellY + dy;

                    if (nx < 0 || nx >= _width || ny < 0 || ny >= _height)
                        continue;

                    int neighbor = ToIndex(nx, ny);
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

        private bool ReduceNeighborOptions(int cell, int neighbor, Direction2D direction)
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

        private bool IsNeighborTileAllowed(int cell, int neighborTile, Direction2D direction)
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

        private int ToIndex(int x, int y)
        {
            return x + y * _width;
        }

        private void ValidateCoordinates(int x, int y)
        {
            if (x < 0 || x >= _width)
                throw new ArgumentOutOfRangeException(nameof(x));
            if (y < 0 || y >= _height)
                throw new ArgumentOutOfRangeException(nameof(y));
        }

        private void ValidateTileIndex(int tileIndex)
        {
            if (tileIndex < 0 || tileIndex >= _tileCount)
                throw new ArgumentOutOfRangeException(nameof(tileIndex));
        }
    }
}
