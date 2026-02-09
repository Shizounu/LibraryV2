using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Shizounu.Library.Utility
{
    public static class ListExtensions
    {
        private static Random _rng = new Random();

        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = _rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        /// <summary>
        /// Get a random element from the list.
        /// </summary>
        public static T GetRandom<T>(this IList<T> list)
        {
            if (list == null || list.Count == 0)
                return default;
            return list[_rng.Next(list.Count)];
        }

        /// <summary>
        /// Remove and return a random element from the list.
        /// </summary>
        public static T RemoveRandom<T>(this IList<T> list)
        {
            if (list == null || list.Count == 0)
                return default;
            int index = _rng.Next(list.Count);
            T item = list[index];
            list.RemoveAt(index);
            return item;
        }

        /// <summary>
        /// Check if the list is null or empty.
        /// </summary>
        public static bool IsNullOrEmpty<T>(this IList<T> list)
        {
            return list == null || list.Count == 0;
        }

        /// <summary>
        /// Check if the list is empty (assumes list is not null).
        /// </summary>
        public static bool IsEmpty<T>(this IList<T> list)
        {
            return list.Count == 0;
        }

        /// <summary>
        /// ForEach with index parameter.
        /// </summary>
        public static void ForEachIndexed<T>(this IList<T> list, Action<T, int> action)
        {
            for (int i = 0; i < list.Count; i++)
            {
                action(list[i], i);
            }
        }

        /// <summary>
        /// AddRange that returns the list for chaining.
        /// </summary>
        public static List<T> AddRangeFluent<T>(this List<T> list, IEnumerable<T> items)
        {
            list.AddRange(items);
            return list;
        }

        /// <summary>
        /// Add item and return the list for chaining.
        /// </summary>
        public static List<T> AddFluent<T>(this List<T> list, T item)
        {
            list.Add(item);
            return list;
        }

        /// <summary>
        /// Add item to the list only if it doesn't already exist.
        /// </summary>
        /// <returns>True if the item was added, false if it already existed.</returns>
        public static bool AddIfUnique<T>(this IList<T> list, T item)
        {
            if (list.Contains(item))
                return false;
            
            list.Add(item);
            return true;
        }

        /// <summary>
        /// Remove all null entries from the list.
        /// </summary>
        public static void RemoveNulls<T>(this List<T> list) where T : class
        {
            list.RemoveAll(item => item == null);
        }

        /// <summary>
        /// Get the last element of the list, or default if empty.
        /// </summary>
        public static T Last<T>(this IList<T> list)
        {
            return list.Count > 0 ? list[list.Count - 1] : default;
        }

        /// <summary>
        /// Get the first element of the list, or default if empty.
        /// </summary>
        public static T First<T>(this IList<T> list)
        {
            return list.Count > 0 ? list[0] : default;
        }

        /// <summary>
        /// Create a shallow copy of the list (copies the list structure but not the elements themselves).
        /// </summary>
        public static List<T> ShallowCopy<T>(this IList<T> list)
        {
            if (list == null)
                return null;
            return new List<T>(list);
        }

        /// <summary>
        /// Create a deep copy of the list using a custom cloning function.
        /// </summary>
        public static List<T> DeepCopy<T>(this IList<T> list, Func<T, T> cloner)
        {
            if (list == null)
                return null;
            if (cloner == null)
                throw new ArgumentNullException(nameof(cloner));

            List<T> copy = new List<T>(list.Count);
            for (int i = 0; i < list.Count; i++)
            {
                copy.Add(cloner(list[i]));
            }
            return copy;
        }

        /// <summary>
        /// Create a deep copy of the list for types that implement ICloneable.
        /// </summary>
        public static List<T> DeepCopy<T>(this IList<T> list) where T : ICloneable
        {
            if (list == null)
                return null;

            List<T> copy = new(list.Count);
            for (int i = 0; i < list.Count; i++)
            {
                copy.Add(list[i] != null ? (T)list[i].Clone() : default);
            }
            return copy;
        }

        
    }

}