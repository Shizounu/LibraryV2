using Shizounu.Library.ScriptableArchitecture;
using Shizounu.Library.Utility;

namespace Shizounu.Library.Extensions
{
    /// <summary>
    /// Extension methods for ScriptableEvent to integrate with EventDebugger.
    /// </summary>
    public static class ScriptableEventDebugExtensions
    {
        /// <summary>
        /// Get the listener count for a ScriptableEvent (requires reflection).
        /// </summary>
        public static int GetListenerCount(this ScriptableEvent scriptableEvent)
        {
            var field = typeof(ScriptableEvent).GetField("_eventListeners", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (field != null)
            {
                var listeners = field.GetValue(scriptableEvent) as System.Collections.Generic.List<System.Action>;
                return listeners?.Count ?? 0;
            }

            return 0;
        }

        /// <summary>
        /// Record an invocation with EventDebugger.
        /// </summary>
        public static void RecordInvocation(this ScriptableEvent scriptableEvent)
        {
            EventDebugger.RecordInvocation(
                scriptableEvent.name,
                scriptableEvent.GetListenerCount()
            );
        }
    }
}
