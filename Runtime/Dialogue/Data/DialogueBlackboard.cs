using UnityEngine;
using Shizounu.Library.GameAI;

namespace Shizounu.Library.Dialogue.Data
{
    /// <summary>
    /// ScriptableObject wrapper for the blackboard system, allowing dialogue blackboards to be created as assets and assigned in the inspector.
    /// </summary>
    [CreateAssetMenu(fileName = "new Dialogue Blackboard", menuName = "Shizounu/Dialogue/Blackboard")]
    public class DialogueBlackboard : ScriptableObject
    {
        private SimpleBlackboard _blackboard;

        /// <summary>
        /// Gets the underlying blackboard instance.
        /// </summary>
        public SimpleBlackboard Blackboard
        {
            get
            {
                if (_blackboard == null)
                    _blackboard = new SimpleBlackboard();
                return _blackboard;
            }
        }

        private void OnEnable()
        {
            if (_blackboard == null)
                _blackboard = new SimpleBlackboard();
        }

        // Convenience methods for direct access
        public T GetValue<T>(string key) => Blackboard.GetValue<T>(key);
        public void SetValue<T>(string key, T value) => Blackboard.SetValue(key, value);
        public bool TryGetValue<T>(string key, out T value) => Blackboard.TryGetValue(key, out value);
        public bool HasKey(string key) => Blackboard.HasKey(key);
        public void Remove(string key) => Blackboard.RemoveValue(key);
        public void Clear() => Blackboard.Clear();
        public System.Collections.Generic.IEnumerable<string> GetAllKeys() => Blackboard.GetAllKeys();
        public System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, object>> GetAllEntries() => Blackboard.GetAllEntries();
    }
}
