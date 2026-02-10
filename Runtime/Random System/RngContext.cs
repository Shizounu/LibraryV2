using System;
using System.Collections.Generic;
using System.Linq;

namespace Shizounu.Library.RandomSystem
{
    /// <summary>
    /// Central manager for reproducible RNG in your application.
    /// Manages multiple RNG users, each pulling from their own RNG source,
    /// while maintaining a shared history for tracking and rewinding.
    /// </summary>
    public class RngContext
    {
        private readonly Dictionary<int, RngUser> _users = new Dictionary<int, RngUser>();
        private readonly Dictionary<string, int> _userIdsByName = new Dictionary<string, int>(StringComparer.Ordinal);
        private int _nextUserId = 0;
        private readonly RngHistory _history = new RngHistory();
        private readonly List<RngSnapshot> _snapshots = new List<RngSnapshot>();
        private bool _recordHistory = true;

        /// <summary>
        /// Gets the RNG history tracker.
        /// </summary>
        public RngHistory History => _history;

        /// <summary>
        /// Gets whether history recording is currently enabled.
        /// </summary>
        public bool RecordHistory => _recordHistory;

        /// <summary>
        /// Gets all registered RNG users.
        /// </summary>
        public IReadOnlyDictionary<int, RngUser> Users => _users;

        /// <summary>
        /// Gets all saved snapshots.
        /// </summary>
        public IReadOnlyList<RngSnapshot> Snapshots => _snapshots;

        /// <summary>
        /// Creates a new Rng context.
        /// </summary>
        public RngContext()
        {
        }

        /// <summary>
        /// Registers a new RNG user or retrieves an existing one.
        /// </summary>
        public RngUser GetOrCreateUser(string name, IRngSource source = null, uint? seed = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("User name must be non-empty", nameof(name));

            if (_userIdsByName.TryGetValue(name, out var existingId) && _users.TryGetValue(existingId, out var existingUser))
                return existingUser;

            return CreateUser(name, source, seed);
        }

        /// <summary>
        /// Gets a registered user by ID.
        /// </summary>
        public bool TryGetUser(int userId, out RngUser user)
        {
            return _users.TryGetValue(userId, out user);
        }

        /// <summary>
        /// Gets a registered user by name.
        /// </summary>
        public bool TryGetUser(string name, out RngUser user)
        {
            user = null;
            if (string.IsNullOrWhiteSpace(name))
                return false;

            if (_userIdsByName.TryGetValue(name, out var userId))
                return _users.TryGetValue(userId, out user);

            return false;
        }

        /// <summary>
        /// Deregisters an RNG user.
        /// </summary>
        public bool RemoveUser(int userId)
        {
            if (!_users.TryGetValue(userId, out var user))
                return false;

            _users.Remove(userId);
            _userIdsByName.Remove(user.Name);
            return true;
        }

        /// <summary>
        /// Enables or disables history recording.
        /// Useful for performance-critical sections where you don't need history.
        /// </summary>
        public void SetRecordHistory(bool record)
        {
            _recordHistory = record;
        }

        /// <summary>
        /// Records a random generation event from a user.
        /// Called internally by RngUser.
        /// </summary>
        internal void RecordGeneration(int userId, uint value, uint stateBeforeGeneration, 
                                       uint stateAfterGeneration, string label)
        {
            if (_recordHistory)
            {
                _history.RecordGeneration(userId, value, stateBeforeGeneration, stateAfterGeneration, label);
            }
        }

        /// <summary>
        /// Creates a snapshot of all RNG users' current states.
        /// </summary>
        public RngSnapshot CreateSnapshot(string label = "")
        {
            var snapshot = new RngSnapshot(label);

            foreach (var user in _users.Values)
            {
                snapshot.RecordUserState(user.Id, user.Source);
            }

            snapshot.SetHistoryStep(_history.CurrentStep);
            _snapshots.Add(snapshot);

            return snapshot;
        }

        /// <summary>
        /// Restores all RNG users to the state recorded in a snapshot.
        /// </summary>
        public bool RestoreFromSnapshot(RngSnapshot snapshot)
        {
            if (snapshot == null)
                return false;

            bool allRestored = true;
            foreach (var userId in snapshot.GetRecordedUserIds())
            {
                if (_users.TryGetValue(userId, out var user))
                {
                    if (!user.RestoreFromSnapshot(snapshot))
                        allRestored = false;
                }
            }

            _history.RewindToStep(snapshot.HistoryStep);

            return allRestored;
        }

        /// <summary>
        /// Rewinds the RNG history to a specific step and restores user states accordingly.
        /// Note: This requires having snapshots to restore from; without them, 
        /// you can only navigate history for analysis.
        /// </summary>
        public bool RewindToStep(int step)
        {
            if (!_history.RewindToStep(step))
                return false;

            // Find the closest snapshot at or before this step
            var snapshotAtOrBefore = _snapshots
                .Where(s => s.HistoryStep <= step)
                .OrderByDescending(s => s.HistoryStep)
                .FirstOrDefault();

            if (snapshotAtOrBefore != null)
            {
                return RestoreFromSnapshot(snapshotAtOrBefore);
            }

            return false;
        }

        /// <summary>
        /// Gets a detailed comparison of different permutation branches.
        /// </summary>
        public RngPermutationComparison ComparePermutations(
            IEnumerable<RngSnapshot> snapshots,
            Action<RngSnapshot> simulationAction)
        {
            if (snapshots == null || !snapshots.Any())
                throw new ArgumentException("Must provide at least one snapshot");

            var comparison = new RngPermutationComparison();

            foreach (var snapshot in snapshots)
            {
                var currentSnapshot = CreateSnapshot($"before_sim_{snapshot.Label}");

                // Restore to snapshot
                RestoreFromSnapshot(snapshot);

                // Run simulation
                simulationAction?.Invoke(snapshot);

                // Record results
                var results = new Dictionary<int, uint>();
                foreach (var user in _users.Values)
                {
                    var userHistory = _history.GetUserEntries(user.Id);
                    if (userHistory.Any())
                    {
                        results[user.Id] = userHistory.Last().Value;
                    }
                }

                comparison.AddBranch(snapshot, results);

                // Restore to pre-simulation state
                RestoreFromSnapshot(currentSnapshot);
            }

            return comparison;
        }

        /// <summary>
        /// Clears all history and snapshots, but keeps registered users.
        /// </summary>
        public void ClearHistory()
        {
            _history.Clear();
            _snapshots.Clear();
        }

        /// <summary>
        /// Clears everything and resets the context as if newly created.
        /// </summary>
        public void Reset()
        {
            _users.Clear();
            _userIdsByName.Clear();
            _nextUserId = 0;
            _history.Clear();
            _snapshots.Clear();
            _recordHistory = true;
        }

        private RngUser CreateUser(string name, IRngSource source, uint? seed)
        {
            if (source == null)
            {
                source = new XorshiftRng(seed ?? GenerateSeed());
            }

            var userId = _nextUserId++;
            var user = new RngUser(userId, name, source, this);
            _users[userId] = user;
            _userIdsByName[name] = userId;
            return user;
        }

        private static uint GenerateSeed()
        {
            unchecked
            {
                return (uint)Environment.TickCount ^ (uint)DateTime.UtcNow.Ticks;
            }
        }

        // ===== LIGHTWEIGHT SNAPSHOT API (for performance-critical algorithms) =====

        /// <summary>
        /// Creates a lightweight snapshot that stores only seed values (4 bytes per user).
        /// ~10-50x faster than full snapshots. Does NOT add to snapshot tracking list.
        /// Perfect for Mini-Max, BSP, Monte Carlo, or any algorithm creating thousands of snapshots.
        /// </summary>
        public RngLightweightSnapshot CreateLightweightSnapshot()
        {
            return RngLightweightSnapshot.Create(this);
        }

        /// <summary>
        /// Restores state from a lightweight snapshot.
        /// </summary>
        public void RestoreFromLightweightSnapshot(in RngLightweightSnapshot snapshot)
        {
            snapshot.Restore(this);
        }

        /// <summary>
        /// Creates a snapshot stack manager for recursive algorithms.
        /// Provides push/pop semantics with automatic restoration on dispose.
        /// </summary>
        /// <param name="autoRestore">If true, restores to initial state when disposed.</param>
        /// <param name="initialCapacity">Initial capacity for the stack (prevents reallocation).</param>
        public RngSnapshotStack CreateSnapshotStack(bool autoRestore = true, int initialCapacity = 32)
        {
            return new RngSnapshotStack(this, autoRestore, initialCapacity);
        }

        /// <summary>
        /// Gets a summary string of the current RNG context state.
        /// </summary>
        public override string ToString()
        {
            return $"RngContext - Users: {_users.Count}, History Entries: {_history.EntryCount}, " +
                   $"Snapshots: {_snapshots.Count}, Recording: {_recordHistory}";
        }
    }
}
