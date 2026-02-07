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
		private readonly List<Blackboard> _boards = new List<Blackboard>();

		#region Composite
		public CompositeBlackboard(IEnumerable<Blackboard> blackboards)
		{
			if (blackboards == null)
				throw new ArgumentNullException(nameof(blackboards));

			foreach (var board in blackboards)
			{
				if (board != null)
					_boards.Add(board);
			}
		}

		public CompositeBlackboard(params Blackboard[] blackboards) : this((IEnumerable<Blackboard>)blackboards)
		{
		}

		public void Add(Blackboard blackboard)
		{
			if (blackboard == null)
				throw new ArgumentNullException(nameof(blackboard));
			_boards.Add(blackboard);
		}

		public bool Remove(Blackboard blackboard)
		{
			if (blackboard == null)
				return false;
			return _boards.Remove(blackboard);
		}
		#endregion

		#region Implementations
		public override T GetValue<T>(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Key cannot be null or empty", nameof(key));

            for (int i = 0; i < _boards.Count; i++)
            {
                if (_boards[i].TryGetValue<T>(key, out var value))
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

            for (int i = 0; i < _boards.Count; i++)
            {
                if (_boards[i].TryGetValue<T>(key, out value))
                    return true;
            }

            value = default;
            return false;
        }

        public override bool HasKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return false;

            for (int i = 0; i < _boards.Count; i++)
            {
                if (_boards[i].HasKey(key))
                    return true;
            }

            return false;
        }

        public override bool RemoveValue(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return false;

            bool removedAny = false;
            for (int i = 0; i < _boards.Count; i++)
            {
                removedAny = _boards[i].RemoveValue(key) || removedAny;
            }

            return removedAny;
        }

        public override void Clear()
        {
            for (int i = 0; i < _boards.Count; i++)
            {
                _boards[i].Clear();
            }
        }

        public override void Subscribe(string key, Action<object> callback)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Key cannot be null or empty", nameof(key));
            if (callback == null)
                throw new ArgumentNullException(nameof(callback));

            for (int i = 0; i < _boards.Count; i++)
            {
                _boards[i].Subscribe(key, callback);
            }
        }

        public override void Unsubscribe(string key, Action<object> callback)
        {
            if (string.IsNullOrWhiteSpace(key) || callback == null)
                return;

            for (int i = 0; i < _boards.Count; i++)
            {
                _boards[i].Unsubscribe(key, callback);
            }
        }

        public override IEnumerable<string> GetAllKeys()
        {
            HashSet<string> keys = new HashSet<string>();
            for (int i = 0; i < _boards.Count; i++)
            {
                foreach (var key in _boards[i].GetAllKeys())
                {
                    keys.Add(key);
                }
            }
            return keys;
        }

        public override IEnumerable<KeyValuePair<string, object>> GetAllEntries()
        {
            Dictionary<string, object> entries = new Dictionary<string, object>();
            for (int i = 0; i < _boards.Count; i++)
            {
                foreach (var entry in _boards[i].GetAllEntries())
                {
                    // First blackboard with the key wins (since reads search in order)
                    if (!entries.ContainsKey(entry.Key))
                    {
                        entries[entry.Key] = entry.Value;
                    }
                }
            }
            return entries;
        }

        public override void SetValue<T>(string key, T value)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Key cannot be null or empty", nameof(key));

            for (int i = 0; i < _boards.Count; i++)
            {
                _boards[i].SetValue(key, value);
            }
        }
        #endregion
    }
}
