namespace Shizounu.Library.GenerationAlgorithms.Shared
{
    /// <summary>
    /// Defines where along the chosen axis to perform a split.
    /// Shared between BSP 2D and 3D implementations.
    /// </summary>
    public enum SplitPositionStrategy
    {
        /// <summary>Split at the middle position</summary>
        Middle,

        /// <summary>Split at a random position</summary>
        Random,

        /// <summary>Split at a position that creates a golden ratio division</summary>
        GoldenRatio
    }

    /// <summary>
    /// Common utilities for Binary Space Partitioning algorithms.
    /// </summary>
    public static class BspUtilities
    {
        /// <summary>Golden ratio constant used for golden ratio splits.</summary>
        public const float GoldenRatio = 2.618f;

        /// <summary>
        /// Calculates split position along a 1D axis.
        /// </summary>
        /// <param name="min">Minimum bound on the axis</param>
        /// <param name="size">Size of the region on the axis</param>
        /// <param name="strategy">Strategy to use for split positioning</param>
        /// <param name="randomValue">Random value in [0, 1) if using random strategy</param>
        /// <returns>The split position on the axis</returns>
        public static float CalculateSplitPosition(float min, float size, 
            SplitPositionStrategy strategy, float randomValue = 0f)
        {
            return strategy switch
            {
                SplitPositionStrategy.Middle => min + size / 2f,
                SplitPositionStrategy.Random => min + randomValue * size,
                SplitPositionStrategy.GoldenRatio => min + size / GoldenRatio,
                _ => min + size / 2f
            };
        }
    }
}
