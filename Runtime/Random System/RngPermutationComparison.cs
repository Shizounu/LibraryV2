using System;
using System.Collections.Generic;
using System.Linq;

namespace Shizounu.Library.RandomSystem
{
    /// <summary>
    /// Stores and compares the results of different RNG permutation branches.
    /// Useful for analyzing how different random outcomes affect game flow.
    /// </summary>
    public class RngPermutationComparison
    {
        /// <summary>
        /// Represents a single branch/permutation result.
        /// </summary>
        public class Branch
        {
            /// <summary>
            /// The snapshot this branch was simulated from.
            /// </summary>
            public RngSnapshot SourceSnapshot { get; set; }

            /// <summary>
            /// Final generated values for each user in this branch.
            /// Key: User ID, Value: Final random value generated.
            /// </summary>
            public Dictionary<int, uint> Results { get; set; } = new Dictionary<int, uint>();

            /// <summary>
            /// Gets the result for a specific user.
            /// </summary>
            public bool TryGetResult(int userId, out uint value)
            {
                return Results.TryGetValue(userId, out value);
            }
        }

        private List<Branch> _branches = new List<Branch>();

        /// <summary>
        /// Gets all branches in this comparison.
        /// </summary>
        public IReadOnlyList<Branch> Branches => _branches;

        /// <summary>
        /// Gets the number of branches in this comparison.
        /// </summary>
        public int BranchCount => _branches.Count;

        /// <summary>
        /// Adds a new branch result to the comparison.
        /// </summary>
        public void AddBranch(RngSnapshot sourceSnapshot, Dictionary<int, uint> results)
        {
            _branches.Add(new Branch
            {
                SourceSnapshot = sourceSnapshot,
                Results = new Dictionary<int, uint>(results)
            });
        }

        /// <summary>
        /// Gets all unique results across all branches for a specific user.
        /// </summary>
        public HashSet<uint> GetUniqueResultsForUser(int userId)
        {
            var uniqueResults = new HashSet<uint>();
            foreach (var branch in _branches)
            {
                if (branch.TryGetResult(userId, out var value))
                    uniqueResults.Add(value);
            }
            return uniqueResults;
        }

        /// <summary>
        /// Gets all unique results across all branches for all users.
        /// </summary>
        public Dictionary<int, HashSet<uint>> GetAllUniqueResults()
        {
            var results = new Dictionary<int, HashSet<uint>>();

            foreach (var branch in _branches)
            {
                foreach (var kvp in branch.Results)
                {
                    if (!results.ContainsKey(kvp.Key))
                        results[kvp.Key] = new HashSet<uint>();

                    results[kvp.Key].Add(kvp.Value);
                }
            }

            return results;
        }

        /// <summary>
        /// Determines if all branches produced the same result for a given user.
        /// </summary>
        public bool AreResultsConsistentForUser(int userId)
        {
            var uniqueResults = GetUniqueResultsForUser(userId);
            return uniqueResults.Count <= 1;
        }

        /// <summary>
        /// Determines if all branches produced the same results for all users.
        /// </summary>
        public bool AreAllResultsConsistent()
        {
            var allResults = GetAllUniqueResults();
            return allResults.Values.All(set => set.Count <= 1);
        }

        /// <summary>
        /// Gets a variance score (0-1) for a user across branches.
        /// 1 = all different values, 0 = all same values.
        /// </summary>
        public float GetVarianceForUser(int userId)
        {
            if (_branches.Count <= 1)
                return 0;

            var uniqueResults = GetUniqueResultsForUser(userId);
            return (float)uniqueResults.Count / _branches.Count;
        }

        /// <summary>
        /// Gets the average variance score across all users.
        /// </summary>
        public float GetAverageVariance()
        {
            if (BranchCount == 0)
                return 0;

            var allResults = GetAllUniqueResults();
            if (allResults.Count == 0)
                return 0;

            float totalVariance = 0;
            foreach (var kvp in allResults)
            {
                totalVariance += (float)kvp.Value.Count / BranchCount;
            }

            return totalVariance / allResults.Count;
        }

        /// <summary>
        /// Gets a string representation comparing the branches.
        /// </summary>
        public string GetComparisonSummary()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"RngPermutationComparison - {_branches.Count} branches");

            var allResults = GetAllUniqueResults();
            foreach (var kvp in allResults)
            {
                var userId = kvp.Key;
                var uniqueVals = kvp.Value;
                var variance = GetVarianceForUser(userId);
                sb.AppendLine($"  User {userId}: {uniqueVals.Count} unique values (variance: {variance:P})");

                foreach (var val in uniqueVals.OrderBy(v => v).Take(5))
                {
                    var count = _branches.Count(b => b.TryGetResult(userId, out var v) && v == val);
                    sb.AppendLine($"    - {val} ({count} branches)");
                }

                if (uniqueVals.Count > 5)
                    sb.AppendLine($"    ... and {uniqueVals.Count - 5} more");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Gets summary statistics for this comparison.
        /// </summary>
        public override string ToString()
        {
            return $"RngPermutationComparison({_branches.Count} branches, avg variance: {GetAverageVariance():P})";
        }
    }
}
