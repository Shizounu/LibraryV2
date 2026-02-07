using UnityEngine;
using Shizounu.Library.GameAI;
using Shizounu.Library.GameAI.StateMachine;
using System;
using System.Collections.Generic;

namespace Shizounu.Library.Utility
{
    /// <summary>
    /// Debug visualization system for AI components.
    /// Shows blackboard values, state machine state, and other AI-related debug info.
    /// </summary>
    public class AIDebugVisualizer : MonoBehaviour
    {
        [Serializable]
        public class VisualizerSettings
        {
            public bool ShowBlackboardValues = true;
            public bool ShowStateMachineState = true;
            public bool ShowDebugDropdown = false;
            public Vector2 PanelPosition = new Vector2(10, 10);
            public Vector2 PanelSize = new Vector2(300, 400);
            public int FontSize = 12;
        }

        [SerializeField] private VisualizerSettings _settings = new();
        [SerializeField] private StateMachine _targetStateMachine;
        [SerializeField] private Color _panelColor = new Color(0, 0, 0, 0.7f);
        [SerializeField] private Color _textColor = Color.white;
        [SerializeField] private Color _headerColor = Color.yellow;

        private Rect _panelRect;
        private GUIStyle _panelStyle;
        private GUIStyle _labelStyle;
        private Vector2 _scrollPosition = Vector2.zero;
        private bool _isVisible = true;

        private void OnGUI()
        {
            if (!_isVisible || _targetStateMachine == null) return;

            InitializeStyles();
            _panelRect = new Rect(_settings.PanelPosition, _settings.PanelSize);

            GUI.color = _panelColor;
            GUI.Box(_panelRect, "", _panelStyle);
            GUI.color = Color.white;

            GUILayout.BeginArea(_panelRect);
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);

            DrawHeader();
            if (_settings.ShowStateMachineState)
                DrawStateMachineInfo();
            if (_settings.ShowBlackboardValues)
                DrawBlackboardInfo();

            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        private void InitializeStyles()
        {
            if (_panelStyle != null) return;

            _panelStyle = new GUIStyle(GUI.skin.box)
            {
                padding = new RectOffset(10, 10, 10, 10)
            };

            _labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = _settings.FontSize,
                wordWrap = true
            };
        }

        private void DrawHeader()
        {
            GUI.color = _headerColor;
            GUILayout.Label("AI Debug Visualizer", _labelStyle);
            GUI.color = Color.white;

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Clear History", GUILayout.Width(100)))
            {
                // History clearing logic would go here
            }
            if (GUILayout.Button(_isVisible ? "Hide" : "Show", GUILayout.Width(50)))
            {
                _isVisible = !_isVisible;
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(5);
        }

        private void DrawStateMachineInfo()
        {
            if (_targetStateMachine == null) return;

            GUI.color = _headerColor;
            GUILayout.Label("State Machine", _labelStyle);
            GUI.color = _textColor;

            var activeState = _targetStateMachine.ActiveState;
            GUILayout.Label($"Current State: {(activeState != null ? activeState.GetType().Name : "None")}", _labelStyle);
            GUILayout.Label($"GameObject: {_targetStateMachine.gameObject.name}", _labelStyle);

            GUILayout.Space(5);
        }

        private void DrawBlackboardInfo()
        {
            if (_targetStateMachine == null || _targetStateMachine.Blackboard == null) return;

            var blackboard = _targetStateMachine.Blackboard;

            GUI.color = _headerColor;
            GUILayout.Label("Blackboard Values", _labelStyle);
            GUI.color = _textColor;

            var entries = blackboard.GetAllEntries();
            int count = 0;

            foreach (var entry in entries)
            {
                var valueStr = entry.Value != null ? entry.Value.ToString() : "null";
                var displayStr = $"{entry.Key}: {valueStr}";
                
                // Truncate long values
                if (displayStr.Length > 50)
                    displayStr = displayStr.Substring(0, 47) + "...";

                GUILayout.Label(displayStr, _labelStyle);
                count++;
            }

            if (count == 0)
            {
                GUILayout.Label("(empty)", _labelStyle);
            }
            else
            {
                GUILayout.Label($"Total: {count} entries", _labelStyle);
            }

            GUILayout.Space(5);
        }

        /// <summary>
        /// Toggle the visualizer visibility.
        /// </summary>
        public void SetVisible(bool visible)
        {
            _isVisible = visible;
        }

        /// <summary>
        /// Set the target state machine to debug.
        /// </summary>
        public void SetTargetStateMachine(StateMachine stateMachine)
        {
            _targetStateMachine = stateMachine;
        }

        /// <summary>
        /// Setup the visualizer with a state machine.
        /// </summary>
        public void Setup(StateMachine stateMachine, Vector2 position = default)
        {
            _targetStateMachine = stateMachine;
            if (position != default)
                _settings.PanelPosition = position;
        }
    }
}
