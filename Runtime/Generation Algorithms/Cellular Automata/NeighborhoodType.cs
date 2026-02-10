namespace Shizounu.Library.GenerationAlgorithms.CellularAutomata
{
    /// <summary>
    /// Defines the types of neighborhoods for cellular automata.
    /// </summary>
    public enum NeighborhoodType
    {
        /// <summary>
        /// Includes all 8 surrounding cells (including diagonals).
        /// Standard for Conway's Game of Life.
        /// </summary>
        Moore,

        /// <summary>
        /// Includes only the 4 orthogonally adjacent cells (no diagonals).
        /// </summary>
        VonNeumann
    }
}
