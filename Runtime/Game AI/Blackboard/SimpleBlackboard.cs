using System;
using System.Collections.Generic;

namespace Shizounu.Library.GameAI
{
	/// <summary>
	/// Standard blackboard implementation using a dictionary for storage.
	/// </summary>
	public class SimpleBlackboard : Blackboard
	{
		private readonly Dictionary<string, object> data = new Dictionary<string, object>();
		private readonly Dictionary<string, List<Action<object>>> changeCallbacks = new Dictionary<string, List<Action<object>>>();

		public override void SetValue<T>(string key, T value)
		{
			if (string.IsNullOrWhiteSpace(key))
				throw new ArgumentException("Key cannot be null or empty", nameof(key));

			data[key] = value;
			NotifyChangeListeners(key, value);
		}

		public override T GetValue<T>(string key)
		{
			if (string.IsNullOrWhiteSpace(key))
				throw new ArgumentException("Key cannot be null or empty", nameof(key));

			if (data.TryGetValue(key, out var value) && value is T typedValue)
				return typedValue;

			return default;
		}

		public override bool TryGetValue<T>(string key, out T value)
		{
			if (string.IsNullOrWhiteSpace(key))
			{
				value = default;
				return false;
			}

			if (data.TryGetValue(key, out var obj) && obj is T typedValue)
			{
				value = typedValue;
				return true;
			}

			value = default;
			return false;
		}

		public override bool HasKey(string key)
		{
			return !string.IsNullOrWhiteSpace(key) && data.ContainsKey(key);
		}

		public override bool RemoveValue(string key)
		{
			if (string.IsNullOrWhiteSpace(key))
				return false;

			return data.Remove(key);
		}

		public override void Clear()
		{
			data.Clear();
			changeCallbacks.Clear();
		}

		public override void Subscribe(string key, Action<object> callback)
		{
			if (string.IsNullOrWhiteSpace(key))
				throw new ArgumentException("Key cannot be null or empty", nameof(key));
			if (callback == null)
				throw new ArgumentNullException(nameof(callback));

			if (!changeCallbacks.ContainsKey(key))
				changeCallbacks[key] = new List<Action<object>>();

			changeCallbacks[key].Add(callback);
		}

		public override void Unsubscribe(string key, Action<object> callback)
		{
			if (string.IsNullOrWhiteSpace(key) || !changeCallbacks.ContainsKey(key))
				return;

			changeCallbacks[key].Remove(callback);
		}

		private void NotifyChangeListeners(string key, object value)
		{
			if (changeCallbacks.TryGetValue(key, out var callbacks))
			{
				for (int i = 0; i < callbacks.Count; i++)
				{
					callbacks[i]?.Invoke(value);
				}
			}
		}

		public override IEnumerable<string> GetAllKeys()
		{
			return data.Keys;
		}

		public override IEnumerable<KeyValuePair<string, object>> GetAllEntries()
		{
			return data;
		}
	}
}
