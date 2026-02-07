using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace Shizounu.Library.Utility
{
    public static class DictionaryExtension
    {
        public static void Merge<TKey, TValue>(this Dictionary<TKey, TValue> mergeTo, Dictionary<TKey, TValue> mergeFrom) {
            mergeFrom.ToList().ForEach(x => mergeTo.AddElementIfUnique(x));
        }
        public static Dictionary<TKey, TValue> MergeNew<TKey, TValue>(this Dictionary<TKey, TValue> dict1, Dictionary<TKey, TValue> dict2) {
            return dict1.Concat(dict2).ToDictionary(x => x.Key, x => x.Value);
        }

        public static Dictionary<TValue,TKey> Invert<TKey, TValue>(this Dictionary<TKey, TValue> dict) {
            return dict.ToDictionary(x => x.Value, x => x.Key);
        }
        public static TKey FindKey<TKey, TValue>(this Dictionary<TKey, TValue> dict, TValue val) {
            return dict.FirstOrDefault(x => EqualityComparer<TValue>.Default.Equals(x.Value, val)).Key;
        }
        public static List<TKey> FindKey<TKey, TValue>(this Dictionary<TKey, TValue> dict, List<TValue> val)
        {
            return dict.Where(x => val.Contains(x.Value)).Select(x => x.Key).ToList();
        }

        public static void AddElementIfUnique<TKey, TValue>(this Dictionary<TKey, TValue> mergeTo, KeyValuePair<TKey, TValue> elem) {
            if(!mergeTo.ContainsKey(elem.Key)) 
                mergeTo.Add(elem.Key, elem.Value);
        }

        /// <summary>
        /// Get value from dictionary or return default if key doesn't exist.
        /// </summary>
        public static TValue GetOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue = default)
        {
            return dictionary.TryGetValue(key, out var value) ? value : defaultValue;
        }

        /// <summary>
        /// Get value from dictionary or add and return the default value if key doesn't exist.
        /// </summary>
        public static TValue GetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue)
        {
            if (dictionary.TryGetValue(key, out var value))
                return value;
            
            dictionary[key] = defaultValue;
            return defaultValue;
        }

        /// <summary>
        /// Get value from dictionary or add and return the result of the factory function if key doesn't exist.
        /// </summary>
        public static TValue GetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, System.Func<TValue> factory)
        {
            if (dictionary.TryGetValue(key, out var value))
                return value;
            
            var newValue = factory();
            dictionary[key] = newValue;
            return newValue;
        }

        /// <summary>
        /// Try to add a key-value pair to the dictionary. Returns false if key already exists.
        /// </summary>
        public static bool TryAdd<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            if (dictionary.ContainsKey(key))
                return false;
            
            dictionary.Add(key, value);
            return true;
        }

        /// <summary>
        /// Check if dictionary is null or empty.
        /// </summary>
        public static bool IsNullOrEmpty<TKey, TValue>(this Dictionary<TKey, TValue> dictionary)
        {
            return dictionary == null || dictionary.Count == 0;
        }
    }
}