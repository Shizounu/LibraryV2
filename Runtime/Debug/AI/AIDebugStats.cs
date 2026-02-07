using UnityEngine;
using Shizounu.Library.GameAI;
using Shizounu.Library.GameAI.StateMachine;
using System.Collections.Generic;

namespace Shizounu.Library.Utility
{
    /// <summary>
    /// Advanced AI debugging with persistent visualization using DebugDraw.
    /// Displays pathfinding, state transitions, and blackboard monitoring.
    /// </summary>
    public static class AIDebugStats
    {
        private struct PathPoint
        {
            public Vector3 Position;
            public StateMachine Owner;
            public float CreationTime;
        }

        private struct StateTransition
        {
            public StateMachine Owner;
            public string FromState;
            public string ToState;
            public float TransitionTime;
            public bool IsActive => Time.time - TransitionTime < 2f; // Show for 2 seconds
        }

        private static List<PathPoint> _currentPaths = new();
        private static List<StateTransition> _stateTransitions = new();
        private static Dictionary<StateMachine, Vector3> _lastPositions = new();
        private static bool _isEnabled = false;
        private static float _pathDrawDuration = 0f;

        /// <summary>
        /// Enable AI debug visualization.
        /// </summary>
        public static void Enable()
        {
            _isEnabled = true;
        }

        /// <summary>
        /// Disable AI debug visualization.
        /// </summary>
        public static void Disable()
        {
            _isEnabled = false;
            Clear();
        }

        /// <summary>
        /// Clear all collected debug data.
        /// </summary>
        public static void Clear()
        {
            _currentPaths.Clear();
            _stateTransitions.Clear();
            _lastPositions.Clear();
        }

        /// <summary>
        /// Record a path for visualization.
        /// </summary>
        public static void RecordPath(StateMachine owner, List<Vector3> pathPoints, float duration = 5f)
        {
            if (!_isEnabled || owner == null || pathPoints == null || pathPoints.Count == 0)
                return;

            // Draw path with lines
            for (int i = 0; i < pathPoints.Count - 1; i++)
            {
                DebugDraw.DrawLine(
                    pathPoints[i],
                    pathPoints[i + 1],
                    category: DebugDraw.Category.Pathfinding,
                    duration: duration
                );
            }

            // Mark start
            if (pathPoints.Count > 0)
            {
                DebugDraw.DrawSphere(
                    pathPoints[0],
                    radius: 0.2f,
                    color: Color.green,
                    duration: duration,
                    category: DebugDraw.Category.Pathfinding
                );
            }

            // Mark end
            if (pathPoints.Count > 1)
            {
                DebugDraw.DrawSphere(
                    pathPoints[pathPoints.Count - 1],
                    radius: 0.15f,
                    color: Color.red,
                    duration: duration,
                    category: DebugDraw.Category.Pathfinding
                );
            }
        }

        /// <summary>
        /// Record a state transition for visualization.
        /// </summary>
        public static void RecordStateTransition(StateMachine owner, string fromState, string toState)
        {
            if (!_isEnabled || owner == null) return;

            _stateTransitions.Add(new StateTransition
            {
                Owner = owner,
                FromState = fromState,
                ToState = toState,
                TransitionTime = Time.time
            });

            // Draw state change as vertical arrow
            var position = owner.transform.position + Vector3.up * 2f;
            DebugDraw.DrawArrow(
                position,
                Vector3.up,
                length: 1f,
                color: Color.magenta,
                duration: 1f,
                category: DebugDraw.Category.AI
            );

            #if UNITY_EDITOR
            Debug.Log($"[AIDebugStats] {owner.name}: {fromState} → {toState}");
            #endif
        }

        /// <summary>
        /// Record a decision point (e.g., AI choosing between options).
        /// </summary>
        public static void RecordDecision(StateMachine owner, string decisionName, bool result, Vector3? position = null)
        {
            if (!_isEnabled || owner == null) return;

            var drawPos = position ?? (owner.transform.position + Vector3.up * 1f);
            var color = result ? Color.green : Color.red;

            DebugDraw.DrawSphere(
                drawPos,
                radius: 0.25f,
                color: color,
                duration: 1f,
                category: DebugDraw.Category.AI
            );

            #if UNITY_EDITOR
            Debug.Log($"[AIDebugStats] {owner.name}: {decisionName} = {result}");
            #endif
        }

        /// <summary>
        /// Record a blackboard value change for monitoring.
        /// </summary>
        public static void RecordBlackboardChange(StateMachine owner, string key, object newValue)
        {
            if (!_isEnabled || owner == null) return;

            #if UNITY_EDITOR
            Debug.Log($"[AIDebugStats] {owner.name}: {key} = {(newValue != null ? newValue.ToString() : "null")}");
            #endif
        }

        /// <summary>
        /// Record a threat detection (e.g., enemy spotted).
        /// </summary>
        public static void RecordThreatDetection(Vector3 threatPosition, float detectionRange, StateMachine detector, bool success)
        {
            if (!_isEnabled) return;

            var color = success ? Color.red : Color.yellow;

            // Draw detection sphere
            DebugDraw.DrawSphere(
                threatPosition,
                radius: 0.3f,
                color: color,
                duration: 1f,
                category: DebugDraw.Category.AI
            );

            // Draw detection range
            DebugDraw.DrawBox(
                detector.transform.position,
                size: Vector3.one * detectionRange * 2f,
                color: new Color(color.r, color.g, color.b, 0.3f),
                duration: 0.5f,
                category: DebugDraw.Category.AI
            );
        }

        /// <summary>
        /// Record movement intent/direction.
        /// </summary>
        public static void RecordMovementIntent(StateMachine owner, Vector3 direction, float magnitude, bool isMoving)
        {
            if (!_isEnabled || owner == null) return;

            if (!isMoving) return;

            DebugDraw.DrawArrow(
                owner.transform.position + Vector3.up * 0.5f,
                direction.normalized,
                length: magnitude,
                color: Color.cyan,
                duration: 0f,
                category: DebugDraw.Category.AI
            );
        }

        /// <summary>
        /// Get a summary of active state transitions.
        /// </summary>
        public static string GetTransitionSummary()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("=== Recent State Transitions ===");
            
            int count = 0;
            foreach (var transition in _stateTransitions)
            {
                if (transition.IsActive)
                {
                    sb.AppendLine($"{transition.Owner.name}: {transition.FromState} → {transition.ToState}");
                    count++;
                }
            }

            if (count == 0)
                sb.AppendLine("(none)");

            return sb.ToString();
        }

        /// <summary>
        /// Clean up expired transitions.
        /// </summary>
        public static void Cleanup()
        {
            _stateTransitions.RemoveAll(t => !t.IsActive);
        }

        public static bool IsEnabled => _isEnabled;
    }
}
