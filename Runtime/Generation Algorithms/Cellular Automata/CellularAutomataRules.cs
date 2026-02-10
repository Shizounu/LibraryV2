using System;
using System.Collections.Generic;
using System.Linq;

namespace Shizounu.Library.GenerationAlgorithms.CellularAutomata
{
    /// <summary>
    /// Defines the rules for a cellular automata simulation.
    /// Uses the notation B{births}/S{survivals} where numbers are neighbor counts.
    /// </summary>
    public class CellularAutomataRules
    {
        /// <summary>Neighbor counts that cause dead cells to become alive</summary>
        public int[] BirthRules { get; private set; }

        /// <summary>Neighbor counts that keep alive cells alive</summary>
        public int[] SurvivalRules { get; private set; }

        /// <summary>The type of neighborhood to consider (Moore or Von Neumann)</summary>
        public NeighborhoodType NeighborhoodType { get; private set; }

        /// <summary>
        /// Creates rules with specified birth and survival conditions.
        /// </summary>
        /// <param name="birthRules">Neighbor counts that cause birth</param>
        /// <param name="survivalRules">Neighbor counts that cause survival</param>
        /// <param name="neighborhoodType">Type of neighborhood to use</param>
        public CellularAutomataRules(int[] birthRules, int[] survivalRules, NeighborhoodType neighborhoodType = NeighborhoodType.Moore)
        {
            if (birthRules == null)
                throw new ArgumentNullException(nameof(birthRules));
            if (survivalRules == null)
                throw new ArgumentNullException(nameof(survivalRules));

            BirthRules = birthRules;
            SurvivalRules = survivalRules;
            NeighborhoodType = neighborhoodType;
        }

        /// <summary>
        /// Creates rules from the standard B/S notation (e.g., "B3/S23" for Conway's Game of Life).
        /// </summary>
        /// <param name="notation">Rules in B{births}/S{survivals} format</param>
        /// <param name="neighborhoodType">Type of neighborhood to use</param>
        /// <returns>Parsed CellularAutomataRules</returns>
        public static CellularAutomataRules FromNotation(string notation, NeighborhoodType neighborhoodType = NeighborhoodType.Moore)
        {
            if (string.IsNullOrEmpty(notation))
                throw new ArgumentException("Notation cannot be null or empty", nameof(notation));

            var parts = notation.Split('/');
            if (parts.Length != 2)
                throw new ArgumentException("Notation must be in B{births}/S{survivals} format", nameof(notation));

            var birthStr = parts[0];
            var survivalStr = parts[1];

            if (!birthStr.StartsWith("B"))
                throw new ArgumentException("Birth rules must start with 'B'", nameof(notation));
            if (!survivalStr.StartsWith("S"))
                throw new ArgumentException("Survival rules must start with 'S'", nameof(notation));

            var births = ExtractRuleNumbers(birthStr.Substring(1));
            var survivals = ExtractRuleNumbers(survivalStr.Substring(1));

            return new CellularAutomataRules(births, survivals, neighborhoodType);
        }

        private static int[] ExtractRuleNumbers(string ruleString)
        {
            if (string.IsNullOrEmpty(ruleString))
                return new int[0];

            return ruleString.Select(c => int.Parse(c.ToString())).ToArray();
        }

        /// <summary>Checks if a cell should be born given its neighbor count</summary>
        public bool ShouldBeBorn(int aliveNeighbors) => BirthRules.Contains(aliveNeighbors);

        /// <summary>Checks if a cell should survive given its neighbor count</summary>
        public bool ShouldSurvive(int aliveNeighbors) => SurvivalRules.Contains(aliveNeighbors);

        /// <summary>Conway's Game of Life rules: B3/S23</summary>
        public static CellularAutomataRules ConwaysGameOfLife => 
            FromNotation("B3/S23", NeighborhoodType.Moore);

        /// <summary>Maze generation rules: B3/S12345</summary>
        public static CellularAutomataRules Maze => 
            FromNotation("B3/S12345", NeighborhoodType.Moore);

        /// <summary>High Life variant: B36/S23</summary>
        public static CellularAutomataRules HighLife => 
            FromNotation("B36/S23", NeighborhoodType.Moore);

        /// <summary>Amoebas: B357/S1358</summary>
        public static CellularAutomataRules Amoebas => 
            FromNotation("B357/S1358", NeighborhoodType.Moore);
    }
}
