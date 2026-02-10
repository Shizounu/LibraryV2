using Shizounu.Library.RandomSystem;

namespace Shizounu.Library.GenerationAlgorithms.Shared
{
    /// <summary>
    /// Interface for generation algorithms that support RNG injection.
    /// Allows algorithms to work with the centralized Random System.
    /// </summary>
    public interface IRngProvider
    {
        /// <summary>
        /// Gets the RNG source used by this algorithm.
        /// </summary>
        IRngSource RngSource { get; }
    }
}
