using System;
using Shizounu.Library.RandomSystem;
using Shizounu.Library.GenerationAlgorithms.Shared;

namespace Shizounu.Library.GenerationAlgorithms.CellularAutomata
{
    /// <summary>
    /// Generates 2D maps using cellular automata.
    /// Supports multiple rule sets and can be used iteratively or for full generation.
    /// </summary>
    public class CellularAutomata : IRngProvider
    {
        private readonly int _width;
        private readonly int _height;
        private bool[][] _grid;
        private bool[][] _nextGrid;
        private readonly CellularAutomataRules _rules;
        private readonly IRngSource _rngSource;

        public int Width => _width;
        public int Height => _height;
        public CellularAutomataRules Rules => _rules;

        /// <summary>
        /// Gets the RNG source used by this cellular automata.
        /// </summary>
        public IRngSource RngSource => _rngSource;

        /// <summary>
        /// Creates a new cellular automata solver.
        /// </summary>
        /// <param name="width">Width of the grid</param>
        /// <param name="height">Height of the grid</param>
        /// <param name="rules">Rules for the simulation</param>
        /// <param name="seed">Random seed for initialization (null for random)</param>
        public CellularAutomata(int width, int height, CellularAutomataRules rules, int? seed = null)
        {
            if (width <= 0)
                throw new ArgumentOutOfRangeException(nameof(width));
            if (height <= 0)
                throw new ArgumentOutOfRangeException(nameof(height));
            if (rules == null)
                throw new ArgumentNullException(nameof(rules));

            _width = width;
            _height = height;
            _rules = rules;
            _rngSource = GenerationRng.Create(seed);
            _grid = new bool[height][];
            _nextGrid = new bool[height][];

            for (int y = 0; y < height; y++)
            {
                _grid[y] = new bool[width];
                _nextGrid[y] = new bool[width];
            }
        }

        /// <summary>
        /// Creates a new cellular automata solver with a provided RNG source.
        /// </summary>
        /// <param name="width">Width of the grid</param>
        /// <param name="height">Height of the grid</param>
        /// <param name="rules">Rules for the simulation</param>
        /// <param name="rngSource">RNG source to use</param>
        public CellularAutomata(int width, int height, CellularAutomataRules rules, IRngSource rngSource)
        {
            if (width <= 0)
                throw new ArgumentOutOfRangeException(nameof(width));
            if (height <= 0)
                throw new ArgumentOutOfRangeException(nameof(height));
            if (rules == null)
                throw new ArgumentNullException(nameof(rules));
            if (rngSource == null)
                throw new ArgumentNullException(nameof(rngSource));

            _width = width;
            _height = height;
            _rules = rules;
            _rngSource = rngSource;
            _grid = new bool[height][];
            _nextGrid = new bool[height][];

            for (int y = 0; y < height; y++)
            {
                _grid[y] = new bool[width];
                _nextGrid[y] = new bool[width];
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

            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    _grid[y][x] = _rngSource.NextFloat() < fillProbability;
                }
            }
        }

        /// <summary>
        /// Clears the grid (all cells dead).
        /// </summary>
        public void ClearGrid()
        {
            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    _grid[y][x] = false;
                }
            }
        }

        /// <summary>
        /// Sets the state of a specific cell.
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <param name="alive">Whether the cell should be alive</param>
        public void SetCell(int x, int y, bool alive)
        {
            if (x < 0 || x >= _width || y < 0 || y >= _height)
                throw new ArgumentOutOfRangeException("Coordinates out of bounds");

            _grid[y][x] = alive;
        }

        /// <summary>
        /// Gets the state of a specific cell.
        /// </summary>
        public bool GetCell(int x, int y)
        {
            if (x < 0 || x >= _width || y < 0 || y >= _height)
                return false;

            return _grid[y][x];
        }

        /// <summary>
        /// Performs one simulation step.
        /// </summary>
        public void Step()
        {
            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    int aliveNeighbors = CountAliveNeighbors(x, y);
                    bool isAlive = _grid[y][x];

                    if (isAlive)
                        _nextGrid[y][x] = _rules.ShouldSurvive(aliveNeighbors);
                    else
                        _nextGrid[y][x] = _rules.ShouldBeBorn(aliveNeighbors);
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
        public bool[][] GetGrid()
        {
            var gridCopy = new bool[_height][];
            for (int y = 0; y < _height; y++)
            {
                gridCopy[y] = new bool[_width];
                Array.Copy(_grid[y], gridCopy[y], _width);
            }
            return gridCopy;
        }

        /// <summary>
        /// Sets the grid from an external source.
        /// </summary>
        public void SetGrid(bool[][] grid)
        {
            if (grid == null)
                throw new ArgumentNullException(nameof(grid));
            if (grid.Length != _height)
                throw new ArgumentException("Grid height mismatch", nameof(grid));

            for (int y = 0; y < _height; y++)
            {
                if (grid[y] == null || grid[y].Length != _width)
                    throw new ArgumentException("Grid width mismatch", nameof(grid));
                Array.Copy(grid[y], _grid[y], _width);
            }
        }

        /// <summary>
        /// Counts the number of alive neighbors for a cell at the given coordinates.
        /// </summary>
        private int CountAliveNeighbors(int x, int y)
        {
            int count = 0;

            if (_rules.NeighborhoodType == NeighborhoodType.Moore)
            {
                // Moore neighborhood: all 8 surrounding cells
                for (int dy = -1; dy <= 1; dy++)
                {
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        if (dx == 0 && dy == 0)
                            continue;

                        int nx = x + dx;
                        int ny = y + dy;

                        if (nx >= 0 && nx < _width && ny >= 0 && ny < _height)
                        {
                            if (_grid[ny][nx])
                                count++;
                        }
                    }
                }
            }
            else // VonNeumann
            {
                // Von Neumann neighborhood: 4 orthogonal neighbors
                int[][] neighbors = new int[][]
                {
                    new int[] { x, y - 1 },  // up
                    new int[] { x, y + 1 },  // down
                    new int[] { x - 1, y },  // left
                    new int[] { x + 1, y }   // right
                };

                foreach (var neighbor in neighbors)
                {
                    int nx = neighbor[0];
                    int ny = neighbor[1];

                    if (nx >= 0 && nx < _width && ny >= 0 && ny < _height)
                    {
                        if (_grid[ny][nx])
                            count++;
                    }
                }
            }

            return count;
        }
    }
}
