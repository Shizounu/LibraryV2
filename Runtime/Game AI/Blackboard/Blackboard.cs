using System;
using System.Collections.Generic;

namespace Shizounu.Library.GameAI
{
    /// <summary>
    /// Abstract base class for blackboards.
    /// Implementations provide concrete storage and behavior.
    /// </summary>
    public abstract class Blackboard
    {
        public abstract void SetValue<T>(string key, T value);
        public abstract T GetValue<T>(string key);
        public abstract bool TryGetValue<T>(string key, out T value);
        public abstract bool HasKey(string key);
        public abstract bool RemoveValue(string key);
        public abstract void Clear();
        public abstract void Subscribe(string key, Action<object> callback);
        public abstract void Unsubscribe(string key, Action<object> callback);
        public abstract IEnumerable<string> GetAllKeys();
        public abstract IEnumerable<KeyValuePair<string, object>> GetAllEntries();
        
        /// <summary>
        /// Creates a deep copy of this blackboard.
        /// Values are cloned to prevent shared references between copies.
        /// </summary>
        public abstract Blackboard DeepCopy();
    }
}