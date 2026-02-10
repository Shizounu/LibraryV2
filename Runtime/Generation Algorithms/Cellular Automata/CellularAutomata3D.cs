using System;
using Shizounu.Library.RandomSystem;
using Shizounu.Library.GenerationAlgorithms.Shared;

namespace Shizounu.Library.GenerationAlgorithms.CellularAutomata
{
    /// <summary>
    /// Generates 3D maps using cellular automata.
    /// Extends the 2D algorithm to three dimensions.
    /// </summary>
    public class CellularAutomata3D : IRngProvider
    {
        private readonly int _width;
        private readonly int _height;
        private readonly int _depth;
        private bool[][][] _grid;
        private bool[][][] _nextGrid;
        private readonly CellularAutomataRules _rules;
        private readonly IRngSource _rngSource;

        public int Width => _width;
        public int Height => _height;
        public int Depth => _depth;
        public CellularAutomataRules Rules => _rules;

        /// <summary>
        /// Gets the RNG source used by this cellular automata.
        /// </summary>
        public IRngSource RngSource => _rngSource;

        /// <summary>
        /// Creates a new 3D cellular automata solver.
        /// </summary>
        /// <param name="width">Width of the grid</param>
        /// <param name="height">Height of the grid</param>
        /// <param name="depth">Depth of the grid</param>
        /// <param name="rules">Rules for the simulation</param>
        /// <param name="seed">Random seed for initialization (null for random)</param>
        public CellularAutomata3D(int width, int height, int depth, CellularAutomataRules rules, int? seed = null)
        {
            if (width <= 0)
                throw new ArgumentOutOfRangeException(nameof(width));
            if (height <= 0)
                throw new ArgumentOutOfRangeException(nameof(height));
            if (depth <= 0)
                throw new ArgumentOutOfRangeException(nameof(depth));
            if (rules == null)
                throw new ArgumentNullException(nameof(rules));

            _width = width;
            _height = height;
            _depth = depth;
            _rules = rules;
            _rngSource = GenerationRng.Create(seed);

            InitializeGrids();
        }

        /// <summary>
        /// Creates a new 3D cellular automata solver with a provided RNG source.
        /// </summary>
        /// <param name="width">Width of the grid</param>
        /// <param name="height">Height of the grid</param>
        /// <param name="depth">Depth of the grid</param>
        /// <param name="rules">Rules for the simulation</param>
        /// <param name="rngSource">RNG source to use</param>
        public CellularAutomata3D(int width, int height, int depth, CellularAutomataRules rules, IRngSource rngSource)
        {
            if (width <= 0)
                throw new ArgumentOutOfRangeException(nameof(width));
            if (height <= 0)
                throw new ArgumentOutOfRangeException(nameof(height));
            if (depth <= 0)
                throw new ArgumentOutOfRangeException(nameof(depth));
            if (rules == null)
                throw new ArgumentNullException(nameof(rules));
            if (rngSource == null)
                throw new ArgumentNullException(nameof(rngSource));

            _width = width;
            _height = height;
            _depth = depth;
            _rules = rules;
            _rngSource = rngSource;

            InitializeGrids();
        }

        private void InitializeGrids()
        {
            _grid = new bool[_depth][][];
            _nextGrid = new bool[_depth][][];

            for (int z = 0; z < _depth; z++)
            {
                _grid[z] = new bool[_height][];
                _nextGrid[z] = new bool[_height][];

                for (int y = 0; y < _height; y++)
                {
                    _grid[z][y] = new bool[_width];
                    _nextGrid[z][y] = new bool[_width];
                }
            }
        }

        /// <summary>
        /// Initializes the grid with random cells.
        /// </summary>
        /// <param name="fillProbability">Probability (0-1) that each cell is alive</param>
        public void RandomizeGrid(float fillProbability = 0.5f)
        {
            if (fillProbability < 0 || fillProbability > 1)
                throw new ArgumentOutOfRangeException(nameof(fillProbability));

            for (int z = 0; z < _depth; z++)
            {
                for (int y = 0; y < _height; y++)
                {
                    for (int x = 0; x < _width; x++)
                    {
                        _grid[z][y][x] = _rngSource.NextFloat() < fillProbability;
                    }
                }
            }
        }

        /// <summary>
        /// Clears the grid (all cells dead).
        /// </summary>
        public void ClearGrid()
        {
            for (int z = 0; z < _depth; z++)
            {
                for (int y = 0; y < _height; y++)
                {
                    for (int x = 0; x < _width; x++)
                    {
                        _grid[z][y][x] = false;
                    }
                }
            }
        }

        /// <summary>
        /// Sets the state of a specific cell.
        /// </summary>
        public void SetCell(int x, int y, int z, bool alive)
        {
            if (x < 0 || x >= _width || y < 0 || y >= _height || z < 0 || z >= _depth)
                throw new ArgumentOutOfRangeException("Coordinates out of bounds");

            _grid[z][y][x] = alive;
        }

        /// <summary>
        /// Gets the state of a specific cell.
        /// </summary>
        public bool GetCell(int x, int y, int z)
        {
            if (x < 0 || x >= _width || y < 0 || y >= _height || z < 0 || z >= _depth)
                return false;

            return _grid[z][y][x];
        }

        /// <summary>
        /// Performs one simulation step.
        /// </summary>
        public void Step()
        {
            for (int z = 0; z < _depth; z++)
            {
                for (int y = 0; y < _height; y++)
                {
                    for (int x = 0; x < _width; x++)
                    {
                        int aliveNeighbors = CountAliveNeighbors(x, y, z);
                        bool isAlive = _grid[z][y][x];

                        if (isAlive)
                            _nextGrid[z][y][x] = _rules.ShouldSurvive(aliveNeighbors);
                        else
                            _nextGrid[z][y][x] = _rules.ShouldBeBorn(aliveNeighbors);
                    }
                }
            }

            // Swap grids
            var temp = _grid;
            _grid = _nextGrid;
            _nextGrid = temp;
        }

        /// <summary>
        /// Performs multiple simulation steps.
        /// </summary>
        /// <param name="steps">Number of steps to perform</param>
        public void Generate(int steps)
        {
            if (steps < 0)
                throw new ArgumentOutOfRangeException(nameof(steps));

            for (int i = 0; i < steps; i++)
                Step();
        }

        /// <summary>
        /// Gets a copy of the current grid state.
        /// </summary>
        public bool[][][] GetGrid()
        {
            var gridCopy = new bool[_depth][][];
            for (int z = 0; z < _depth; z++)
            {
                gridCopy[z] = new bool[_height][];
                for (int y = 0; y < _height; y++)
                {
                    gridCopy[z][y] = new bool[_width];
                    Array.Copy(_grid[z][y], gridCopy[z][y], _width);
                }
            }
            return gridCopy;
        }

        /// <summary>
        /// Sets the grid from an external source.
        /// </summary>
        public void SetGrid(bool[][][] grid)
        {
            if (grid == null)
                throw new ArgumentNullException(nameof(grid));
            if (grid.Length != _depth)
                throw new ArgumentException("Grid depth mismatch", nameof(grid));

            for (int z = 0; z < _depth; z++)
            {
                if (grid[z] == null || grid[z].Length != _height)
                    throw new ArgumentException("Grid height mismatch", nameof(grid));

                for (int y = 0; y < _height; y++)
                {
                    if (grid[z][y] == null || grid[z][y].Length != _width)
                        throw new ArgumentException("Grid width mismatch", nameof(grid));
                    Array.Copy(grid[z][y], _grid[z][y], _width);
                }
            }
        }

        /// <summary>
        /// Counts the number of alive neighbors for a cell at the given coordinates.
        /// Uses Moore neighborhood extended to 3D (26 neighbors).
        /// </summary>
        private int CountAliveNeighbors(int x, int y, int z)
        {
            int count = 0;

            for (int dz = -1; dz <= 1; dz++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        if (dx == 0 && dy == 0 && dz == 0)
                            continue;

                        int nx = x + dx;
                        int ny = y + dy;
                        int nz = z + dz;

                        if (nx >= 0 && nx < _width && ny >= 0 && ny < _height && nz >= 0 && nz < _depth)
                        {
                            if (_grid[nz][ny][nx])
                                count++;
                        }
                    }
                }
            }

            return count;
        }
    }
}
