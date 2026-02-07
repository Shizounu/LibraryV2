using UnityEngine;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Shizounu.Library.Utility
{
    /// <summary>
    /// Debug system for tracking ScriptableEvent invocations, listeners, and history.
    /// Integrates with the ScriptableArchitecture event system.
    /// </summary>
    public static class EventDebugger
    {
        public class EventInvocationRecord
        {
            public string EventName;
            public double Timestamp;
            public int ListenerCount;
            public string StackTrace;
            public float DeltaTimeSinceLastInvoke;

            public override string ToString()
            {
                return $"[{Timestamp:F2}s] {EventName} ({ListenerCount} listeners) - {DeltaTimeSinceLastInvoke:F3}s ago";
            }
        }

        private static bool _isEnabled = false;
        private static bool _captureStackTraces = false;
        private static Dictionary<string, List<EventInvocationRecord>> _eventHistory = new();
        private static Dictionary<string, float> _lastInvokeTimes = new();
        private static int _maxHistoryPerEvent = 100;
        private static HashSet<string> _trackedEvents = new();

        public static bool IsEnabled => _isEnabled;
        public static bool CaptureStackTraces => _captureStackTraces;

        /// <summary>
        /// Enable event debugging.
        /// </summary>
        public static void Enable(bool captureStackTraces = false)
        {
            _isEnabled = true;
            _captureStackTraces = captureStackTraces;
        }

        /// <summary>
        /// Disable event debugging.
        /// </summary>
        public static void Disable()
        {
            _isEnabled = false;
        }

        /// <summary>
        /// Clear all event history.
        /// </summary>
        public static void ClearHistory()
        {
            _eventHistory.Clear();
            _lastInvokeTimes.Clear();
            _trackedEvents.Clear();
        }

        /// <summary>
        /// Clear history for a specific event.
        /// </summary>
        public static void ClearEventHistory(string eventName)
        {
            if (_eventHistory.ContainsKey(eventName))
                _eventHistory[eventName].Clear();
            if (_lastInvokeTimes.ContainsKey(eventName))
                _lastInvokeTimes.Remove(eventName);
        }

        /// <summary>
        /// Set maximum history entries per event (default 100).
        /// </summary>
        public static void SetMaxHistoryPerEvent(int maxCount)
        {
            _maxHistoryPerEvent = Mathf.Max(1, maxCount);
        }

        /// <summary>
        /// Record an event invocation. Call this from ScriptableEvent.Invoke().
        /// </summary>
        internal static void RecordInvocation(string eventName, int listenerCount)
        {
            if (!_isEnabled) return;

            var record = new EventInvocationRecord
            {
                EventName = eventName,
                Timestamp = Time.time,
                ListenerCount = listenerCount,
                StackTrace = _captureStackTraces ? new StackTrace(2, true).ToString() : ""
            };

            // Calculate delta time
            if (_lastInvokeTimes.TryGetValue(eventName, out var lastTime))
            {
                record.DeltaTimeSinceLastInvoke = Time.time - lastTime;
            }
            else
            {
                record.DeltaTimeSinceLastInvoke = 0f;
            }

            _lastInvokeTimes[eventName] = Time.time;

            // Add to history
            if (!_eventHistory.ContainsKey(eventName))
                _eventHistory[eventName] = new List<EventInvocationRecord>();

            _eventHistory[eventName].Add(record);

            // Trim history if needed
            if (_eventHistory[eventName].Count > _maxHistoryPerEvent)
                _eventHistory[eventName].RemoveAt(0);

            _trackedEvents.Add(eventName);

            #if UNITY_EDITOR
            if (_captureStackTraces)
                UnityEngine.Debug.Log($"[EventDebugger] {eventName} invoked ({listenerCount} listeners)");
            #endif
        }

        /// <summary>
        /// Get all recorded events.
        /// </summary>
        public static IEnumerable<string> GetTrackedEvents() => _trackedEvents;

        /// <summary>
        /// Get invocation history for a specific event.
        /// </summary>
        public static List<EventInvocationRecord> GetEventHistory(string eventName)
        {
            return _eventHistory.TryGetValue(eventName, out var history) 
                ? new List<EventInvocationRecord>(history) 
                : new List<EventInvocationRecord>();
        }

        /// <summary>
        /// Get the last invocation time for an event.
        /// </summary>
        public static float GetLastInvokeTime(string eventName)
        {
            return _lastInvokeTimes.TryGetValue(eventName, out var time) ? time : -1f;
        }

        /// <summary>
        /// Get invocation frequency (invocations per second) for an event.
        /// </summary>
        public static float GetInvocationFrequency(string eventName)
        {
            if (!_eventHistory.TryGetValue(eventName, out var history) || history.Count < 2)
                return 0f;

            var firstRecord = history[0];
            var lastRecord = history[history.Count - 1];
            var timeDelta = lastRecord.Timestamp - firstRecord.Timestamp;

            return timeDelta > 0 ? (float)((history.Count - 1) / timeDelta) : 0f;
        }

        /// <summary>
        /// Get total invocation count for an event (in current session).
        /// </summary>
        public static int GetInvocationCount(string eventName)
        {
            return _eventHistory.TryGetValue(eventName, out var history) ? history.Count : 0;
        }

        /// <summary>
        /// Get average listener count for an event.
        /// </summary>
        public static float GetAverageListenerCount(string eventName)
        {
            if (!_eventHistory.TryGetValue(eventName, out var history) || history.Count == 0)
                return 0f;

            float total = 0;
            foreach (var record in history)
                total += record.ListenerCount;

            return total / history.Count;
        }

        /// <summary>
        /// Get a formatted summary of all tracked events.
        /// </summary>
        public static string GetSummary()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("=== Event Debugger Summary ===");
            sb.AppendLine($"Enabled: {_isEnabled}");
            sb.AppendLine($"Tracked Events: {_trackedEvents.Count}");
            sb.AppendLine();

            foreach (var eventName in _trackedEvents)
            {
                var history = GetEventHistory(eventName);
                if (history.Count == 0) continue;

                var lastRecord = history[history.Count - 1];
                var frequency = GetInvocationFrequency(eventName);
                var avgListeners = GetAverageListenerCount(eventName);

                sb.AppendLine($"[{eventName}]");
                sb.AppendLine($"  Invocations: {history.Count}");
                sb.AppendLine($"  Frequency: {frequency:F2}/sec");
                sb.AppendLine($"  Current Listeners: {lastRecord.ListenerCount}");
                sb.AppendLine($"  Avg Listeners: {avgListeners:F1}");
                sb.AppendLine($"  Last Invoke: {lastRecord.DeltaTimeSinceLastInvoke:F3}s ago");
                sb.AppendLine();
            }

            return sb.ToString();
        }

        /// <summary>
        /// Export event history to a CSV format.
        /// </summary>
        public static string ExportToCSV()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("EventName,Timestamp,ListenerCount,DeltaTime");

            foreach (var eventName in _trackedEvents)
            {
                var history = GetEventHistory(eventName);
                foreach (var record in history)
                {
                    sb.AppendLine($"{eventName},{record.Timestamp:F4},{record.ListenerCount},{record.DeltaTimeSinceLastInvoke:F4}");
                }
            }

            return sb.ToString();
        }
    }
}
