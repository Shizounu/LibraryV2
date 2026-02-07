using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Shizounu.Library.GameAI
{
	/// <summary>
	/// Standard blackboard implementation using a dictionary for storage.
	/// </summary>
	public class SimpleBlackboard : Blackboard
	{
		private readonly Dictionary<string, object> _data = new Dictionary<string, object>();
		private readonly Dictionary<string, List<Action<object>>> _changeCallbacks = new Dictionary<string, List<Action<object>>>();

		public override void SetValue<T>(string key, T value)
		{
			if (string.IsNullOrWhiteSpace(key))
				throw new ArgumentException("Key cannot be null or empty", nameof(key));

			_data[key] = value;
			NotifyChangeListeners(key, value);
		}

		public override T GetValue<T>(string key)
		{
			if (string.IsNullOrWhiteSpace(key))
				throw new ArgumentException("Key cannot be null or empty", nameof(key));

			if (_data.TryGetValue(key, out var value) && value is T typedValue)
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

			if (_data.TryGetValue(key, out var obj) && obj is T typedValue)
			{
				value = typedValue;
				return true;
			}

			value = default;
			return false;
		}

		public override bool HasKey(string key)
		{
			return !string.IsNullOrWhiteSpace(key) && _data.ContainsKey(key);
		}

		public override bool RemoveValue(string key)
		{
			if (string.IsNullOrWhiteSpace(key))
				return false;

			return _data.Remove(key);
		}

		public override void Clear()
		{
			_data.Clear();
			_changeCallbacks.Clear();
		}

		public override void Subscribe(string key, Action<object> callback)
		{
			if (string.IsNullOrWhiteSpace(key))
				throw new ArgumentException("Key cannot be null or empty", nameof(key));
			if (callback == null)
				throw new ArgumentNullException(nameof(callback));

			if (!_changeCallbacks.ContainsKey(key))
				_changeCallbacks[key] = new List<Action<object>>();

			_changeCallbacks[key].Add(callback);
		}

		public override void Unsubscribe(string key, Action<object> callback)
		{
			if (string.IsNullOrWhiteSpace(key) || !_changeCallbacks.ContainsKey(key))
				return;

			_changeCallbacks[key].Remove(callback);
		}

		private void NotifyChangeListeners(string key, object value)
		{
			if (_changeCallbacks.TryGetValue(key, out var callbacks))
			{
				for (int i = 0; i < callbacks.Count; i++)
				{
					callbacks[i]?.Invoke(value);
				}
			}
		}

		public override IEnumerable<string> GetAllKeys()
		{
			return _data.Keys;
		}

		public override IEnumerable<KeyValuePair<string, object>> GetAllEntries()
		{
			return _data;
		}

		public override Blackboard DeepCopy()
		{
			var copy = new SimpleBlackboard();
			
			foreach (var entry in _data)
			{
				var clonedValue = DeepCloneValue(entry.Value);
				copy._data[entry.Key] = clonedValue;
			}
			
			// Note: Callbacks are not copied as they would reference the original context
			
			return copy;
		}

		private object DeepCloneValue(object value)
		{
			if (value == null)
				return null;

			var type = value.GetType();

			// Value types and strings are safe (immutable or copied by value)
			if (type.IsValueType || type == typeof(string))
				return value;

			// Handle ICloneable
			if (value is ICloneable cloneable)
				return cloneable.Clone();

			// Handle arrays
			if (type.IsArray)
			{
				var array = value as Array;
				var elementType = type.GetElementType();
				var clonedArray = Array.CreateInstance(elementType, array.Length);
				
				for (int i = 0; i < array.Length; i++)
				{
					clonedArray.SetValue(DeepCloneValue(array.GetValue(i)), i);
				}
				
				return clonedArray;
			}

			// Handle List<T>
			if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
			{
				var list = value as System.Collections.IList;
				var elementType = type.GetGenericArguments()[0];
				var clonedList = (System.Collections.IList)Activator.CreateInstance(type);
				
				foreach (var item in list)
				{
					clonedList.Add(DeepCloneValue(item));
				}
				
				return clonedList;
			}

			// Handle Dictionary<TKey, TValue>
			if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
			{
				var dict = value as System.Collections.IDictionary;
				var clonedDict = (System.Collections.IDictionary)Activator.CreateInstance(type);
				
				foreach (System.Collections.DictionaryEntry entry in dict)
				{
					clonedDict.Add(DeepCloneValue(entry.Key), DeepCloneValue(entry.Value));
				}
				
				return clonedDict;
			}

			// Handle Unity-serializable objects using JSON
			if (type.IsSerializable || type.GetCustomAttribute<SerializableAttribute>() != null)
			{
				try
				{
					var json = JsonUtility.ToJson(value);
					return JsonUtility.FromJson(json, type);
				}
				catch
				{
					// If JSON serialization fails, try MemberwiseClone as fallback
					var method = type.GetMethod("MemberwiseClone", BindingFlags.Instance | BindingFlags.NonPublic);
					if (method != null)
						return method.Invoke(value, null);
				}
			}

			// For other reference types, attempt MemberwiseClone
			// This creates a shallow copy, but it's better than sharing the reference
			try
			{
				var method = type.GetMethod("MemberwiseClone", BindingFlags.Instance | BindingFlags.NonPublic);
				if (method != null)
					return method.Invoke(value, null);
			}
			catch { }

			// Last resort: return the original value
			// This means it will be a shallow copy for this specific value
			Debug.LogWarning($"Unable to deep clone value of type {type.Name}. Using shallow copy.");
			return value;
		}
	}
}
