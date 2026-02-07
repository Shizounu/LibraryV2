using UnityEngine;
using Shizounu.Library.GameAI.StateMachine;
using Shizounu.Library.Utility;

namespace Shizounu.Library.Examples
{
    /// <summary>
    /// Example demonstrating AIDebugVisualizer and AIDebugStats usage.
    /// </summary>
    public class AIDebugVisualizerExample : MonoBehaviour
    {
        [SerializeField] private StateMachine _targetStateMachine;
        [SerializeField] private bool _enableDebugStats = true;
        [SerializeField] private bool _enableVisualizer = true;

        private AIDebugVisualizer _visualizer;
        private float _threatDetectionRange = 10f;
        private Vector3 _lastRecordedPath;

        private void Start()
        {
            if (_enableDebugStats)
            {
                AIDebugStats.Enable();
            }

            if (_enableVisualizer && _targetStateMachine != null)
            {
                var visualizerGO = new GameObject("_AIDebugVisualizer");
                _visualizer = visualizerGO.AddComponent<AIDebugVisualizer>();
                _visualizer.Setup(_targetStateMachine, new Vector2(400, 10));
            }
        }

        private void Update()
        {
            if (!AIDebugStats.IsEnabled) return;

            // Example: Record movement intent
            var moveDirection = Vector3.zero;
            if (Input.GetKey(KeyCode.W)) moveDirection += Vector3.forward;
            if (Input.GetKey(KeyCode.A)) moveDirection += Vector3.left;
            if (Input.GetKey(KeyCode.S)) moveDirection += Vector3.back;
            if (Input.GetKey(KeyCode.D)) moveDirection += Vector3.right;

            if (_targetStateMachine != null && moveDirection.sqrMagnitude > 0)
            {
                AIDebugStats.RecordMovementIntent(
                    _targetStateMachine,
                    moveDirection.normalized,
                    magnitude: 3f,
                    isMoving: true
                );
            }

            // Example: Record threat detection
            if (Input.GetKeyDown(KeyCode.T))
            {
                var threatPos = _targetStateMachine.transform.position + 
                               _targetStateMachine.transform.forward * (_threatDetectionRange * 0.5f);
                
                AIDebugStats.RecordThreatDetection(
                    threatPos,
                    _threatDetectionRange,
                    _targetStateMachine,
                    success: true
                );
            }

            // Example: Record state transition
            if (Input.GetKeyDown(KeyCode.I))
            {
                if (_targetStateMachine != null)
                {
                    AIDebugStats.RecordStateTransition(
                        _targetStateMachine,
                        "Patrol",
                        "Chase"
                    );
                }
            }

            // Example: Record decision
            if (Input.GetKeyDown(KeyCode.H))
            {
                if (_targetStateMachine != null)
                {
                    AIDebugStats.RecordDecision(
                        _targetStateMachine,
                        "CanSeeTarget",
                        result: true,
                        position: _targetStateMachine.transform.position + Vector3.up * 2f
                    );
                }
            }

            // Example: Record path
            if (Input.GetKeyDown(KeyCode.P))
            {
                if (_targetStateMachine != null)
                {
                    var path = new System.Collections.Generic.List<Vector3>
                    {
                        _targetStateMachine.transform.position,
                        _targetStateMachine.transform.position + Vector3.forward * 5f,
                        _targetStateMachine.transform.position + new Vector3(5f, 0, 5f),
                        _targetStateMachine.transform.position + new Vector3(5f, 0, 10f)
                    };

                    AIDebugStats.RecordPath(_targetStateMachine, path, duration: 5f);
                }
            }

            // Debug info
            if (Input.GetKeyDown(KeyCode.F4))
            {
                Debug.Log(AIDebugStats.GetTransitionSummary());
            }

            if (Input.GetKeyDown(KeyCode.F5))
            {
                AIDebugStats.Cleanup();
                Debug.Log("AI debug stats cleaned up");
            }

            AIDebugStats.Cleanup();
        }

        private void OnGUI()
        {
            GUILayout.Label("AI Debug Controls:");
            GUILayout.Label("T - Record threat detection");
            GUILayout.Label("I - Record state transition");
            GUILayout.Label("H - Record decision");
            GUILayout.Label("P - Record path");
            GUILayout.Label("F4 - Show transition summary");
            GUILayout.Label("F5 - Cleanup");
        }
    }
}
