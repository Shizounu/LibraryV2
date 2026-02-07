using System;
using System.Collections.Generic;

namespace Shizounu.Library.GameAI
{
    /// <summary>
    /// Composite blackboard that aggregates multiple blackboards.
    /// Reads search in order; writes are applied to all child blackboards.
    /// </summary>
    public class CompositeBlackboard : Blackboard
    {
        private readonly List<Blackboard> boards = new List<Blackboard>();

        #region  Composite
        public CompositeBlackboard(IEnumerable<Blackboard> blackboards)
        {
            if (blackboards == null)
                throw new ArgumentNullException(nameof(blackboards));

            foreach (var board in blackboards)
            {
                if (board != null)
                    boards.Add(board);
            }
        }

        public CompositeBlackboard(params Blackboard[] blackboards) : this((IEnumerable<Blackboard>)blackboards) // Reuse the other constructor for array input
        {

        }

        public void Add(Blackboard blackboard)
        {
            if (blackboard == null)
                throw new ArgumentNullException(nameof(blackboard));
            boards.Add(blackboard);
        }

        public bool Remove(Blackboard blackboard)
        {
            if (blackboard == null)
                return false;
            return boards.Remove(blackboard);
        }
        #endregion

        #region  Implementations
        public override void SetValue<T>(string key, T value)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Key cannot be null or empty", nameof(key));

            for (int i = 0; i < boards.Count; i++)
            {
                boards[i].SetValue(key, value);
            }
        }

        public override T GetValue<T>(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Key cannot be null or empty", nameof(key));

            for (int i = 0; i < boards.Count; i++)
            {
                if (boards[i].TryGetValue<T>(key, out var value))
                    return value;
            }

            return default;
        }

        public override bool TryGetValue<T>(string key, out T value)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                value = default;
                return false;
            }

            for (int i = 0; i < boards.Count; i++)
            {
                if (boards[i].TryGetValue<T>(key, out value))
                    return true;
            }

            value = default;
            return false;
        }

        public override bool HasKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return false;

            for (int i = 0; i < boards.Count; i++)
            {
                if (boards[i].HasKey(key))
                    return true;
            }

            return false;
        }

        public override bool RemoveValue(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return false;

            bool removedAny = false;
            for (int i = 0; i < boards.Count; i++)
            {
                removedAny = boards[i].RemoveValue(key) || removedAny;
            }

            return removedAny;
        }

        public override void Clear()
        {
            for (int i = 0; i < boards.Count; i++)
            {
                boards[i].Clear();
            }
        }

        public override void Subscribe(string key, Action<object> callback)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Key cannot be null or empty", nameof(key));
            if (callback == null)
                throw new ArgumentNullException(nameof(callback));

            for (int i = 0; i < boards.Count; i++)
            {
                boards[i].Subscribe(key, callback);
            }
        }

        public override void Unsubscribe(string key, Action<object> callback)
        {
            if (string.IsNullOrWhiteSpace(key) || callback == null)
                return;

            for (int i = 0; i < boards.Count; i++)
            {
                boards[i].Unsubscribe(key, callback);
            }
        }
        #endregion
    }
}
