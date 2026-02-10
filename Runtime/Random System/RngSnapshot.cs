using System;
using System.Collections.Generic;

namespace Shizounu.Library.RandomSystem
{
    /// <summary>
    /// A snapshot of all RNG sources at a specific point in time.
    /// Can be used to save and restore the complete RNG state for branching simulations.
    /// </summary>
    public class RngSnapshot
    {
        /// <summary>
        /// The timestamp when this snapshot was created.
        /// </summary>
        public DateTime Timestamp { get; private set; }

        /// <summary>
        /// Optional label for this snapshot (e.g., "before_combat", "turn_5").
        /// </summary>
        public string Label { get; private set; }

        /// <summary>
        /// The RNG state for each user at this snapshot point.
        /// Key: User ID, Value: RNG source (cloned state).
        /// </summary>
        private Dictionary<int, IRngSource> _sourceStates = new Dictionary<int, IRngSource>();

        /// <summary>
        /// The history position at this snapshot point.
        /// </summary>
        public int HistoryStep { get; private set; }

        /// <summary>
        /// Creates a new RNG snapshot with the given label.
        /// </summary>
        public RngSnapshot(string label = "")
        {
            Label = label;
            Timestamp = DateTime.UtcNow;
        }

        /// <summary>
        /// Records a user's RNG source state at this snapshot point.
        /// </summary>
        public void RecordUserState(int userId, IRngSource source)
        {
            if (source != null)
            {
                _sourceStates[userId] = source.Clone();
            }
        }

        /// <summary>
        /// Sets the history step number for this snapshot.
        /// </summary>
        public void SetHistoryStep(int step)
        {
            HistoryStep = step;
        }

        /// <summary>
        /// Retrieves a user's RNG source state from this snapshot.
        /// </summary>
        public bool TryGetUserState(int userId, out IRngSource source)
        {
            return _sourceStates.TryGetValue(userId, out source);
        }

        /// <summary>
        /// Gets all recorded user states in this snapshot.
        /// </summary>
        public Dictionary<int, IRngSource> GetAllUserStates()
        {
            return new Dictionary<int, IRngSource>(_sourceStates);
        }

        /// <summary>
        /// Gets the set of user IDs currently recorded in this snapshot.
        /// </summary>
        public IEnumerable<int> GetRecordedUserIds()
        {
            return _sourceStates.Keys;
        }

        /// <summary>
        /// Gets information about this snapshot as a string.
        /// </summary>
        public override string ToString()
        {
            var timeStr = Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff");
            var labelStr = string.IsNullOrEmpty(Label) ? "" : $" ({Label})";
            var userCount = _sourceStates.Count;
            return $"RngSnapshot{labelStr} - {timeStr} - {userCount} users at step {HistoryStep}";
        }
    }
}
