using System;
using System.Collections.Generic;

namespace Shizounu.Library.RandomSystem
{
    /// <summary>
    /// Records a single random value generation event in the RNG history.
    /// </summary>
    public readonly struct RngHistoryEntry
    {
        /// <summary>
        /// The unique user ID that generated this value.
        /// </summary>
        public int UserId { get; }

        /// <summary>
        /// The random value that was generated.
        /// </summary>
        public uint Value { get; }

        /// <summary>
        /// Optional label/name for this generation (e.g., "attack_roll", "damage_roll").
        /// </summary>
        public string Label { get; }

        /// <summary>
        /// The RNG state immediately before this value was generated.
        /// </summary>
        public uint StateBeforeGeneration { get; }

        /// <summary>
        /// The RNG state immediately after this value was generated.
        /// </summary>
        public uint StateAfterGeneration { get; }

        /// <summary>
        /// The global step number in the RNG timeline.
        /// </summary>
        public int GlobalStep { get; }

        private RngHistoryEntry(
            int userId,
            uint value,
            string label,
            uint stateBeforeGeneration,
            uint stateAfterGeneration,
            int globalStep)
        {
            UserId = userId;
            Value = value;
            Label = label ?? string.Empty;
            StateBeforeGeneration = stateBeforeGeneration;
            StateAfterGeneration = stateAfterGeneration;
            GlobalStep = globalStep;
        }

        public static RngHistoryEntry Create(
            int userId,
            uint value,
            string label,
            uint stateBeforeGeneration,
            uint stateAfterGeneration,
            int globalStep)
        {
            return new RngHistoryEntry(
                userId,
                value,
                label,
                stateBeforeGeneration,
                stateAfterGeneration,
                globalStep);
        }
    }

    /// <summary>
    /// Tracks the history of all random number generations for rewinding and comparison.
    /// </summary>
    public class RngHistory
    {
        private List<RngHistoryEntry> _entries = new List<RngHistoryEntry>();
        private int _currentStep = 0;

        /// <summary>
        /// Gets the total number of entries in the history.
        /// </summary>
        public int EntryCount => _entries.Count;

        /// <summary>
        /// Gets the current step position in the history.
        /// </summary>
        public int CurrentStep => _currentStep;

        /// <summary>
        /// Gets a read-only view of the recorded entries without allocation.
        /// </summary>
        public IReadOnlyList<RngHistoryEntry> Entries => _entries;

        /// <summary>
        /// Gets whether we can step backward from the current position.
        /// </summary>
        public bool CanStepBackward => _currentStep > 0;

        /// <summary>
        /// Gets whether we can step forward from the current position.
        /// </summary>
        public bool CanStepForward => _currentStep < _entries.Count;

        /// <summary>
        /// Records a new random generation event.
        /// </summary>
        public void RecordGeneration(int userId, uint value, uint stateBeforeGeneration, 
                                     uint stateAfterGeneration, string label = "")
        {
            // If we're not at the end, remove all entries after current position
            if (_currentStep < _entries.Count)
            {
                _entries.RemoveRange(_currentStep, _entries.Count - _currentStep);
            }

            var entry = RngHistoryEntry.Create(
                userId,
                value,
                label,
                stateBeforeGeneration,
                stateAfterGeneration,
                _currentStep);

            _entries.Add(entry);
            _currentStep++;
        }

        /// <summary>
        /// Rewinds to a specific step in the history.
        /// </summary>
        public bool RewindToStep(int step)
        {
            if (step < 0 || step > _entries.Count)
                return false;

            _currentStep = step;
            return true;
        }

        /// <summary>
        /// Steps backward one generation.
        /// </summary>
        public bool StepBackward()
        {
            if (!CanStepBackward)
                return false;

            _currentStep--;
            return true;
        }

        /// <summary>
        /// Steps forward one generation.
        /// </summary>
        public bool StepForward()
        {
            if (!CanStepForward)
                return false;

            _currentStep++;
            return true;
        }

        /// <summary>
        /// Gets the entry at the specified index.
        /// </summary>
        public bool TryGetEntry(int index, out RngHistoryEntry entry)
        {
            if (index < 0 || index >= _entries.Count)
            {
                entry = default;
                return false;
            }

            entry = _entries[index];
            return true;
        }

        /// <summary>
        /// Gets all entries from a specific user.
        /// </summary>
        public List<RngHistoryEntry> GetUserEntries(int userId)
        {
            var userEntries = new List<RngHistoryEntry>();
            foreach (var entry in _entries)
            {
                if (entry.UserId == userId)
                    userEntries.Add(entry);
            }
            return userEntries;
        }

        /// <summary>
        /// Enumerates entries for a specific user without allocating a list.
        /// </summary>
        public IEnumerable<RngHistoryEntry> EnumerateUserEntries(int userId)
        {
            foreach (var entry in _entries)
            {
                if (entry.UserId == userId)
                    yield return entry;
            }
        }

        /// <summary>
        /// Clears all history.
        /// </summary>
        public void Clear()
        {
            _entries.Clear();
            _currentStep = 0;
        }

        /// <summary>
        /// Gets all entries in the history.
        /// </summary>
        public List<RngHistoryEntry> GetAllEntries()
        {
            return new List<RngHistoryEntry>(_entries);
        }
    }
}
