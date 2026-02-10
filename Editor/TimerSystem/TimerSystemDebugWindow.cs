using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Shizounu.Library.Timer;

namespace Shizounu.Library.Editor.Timer
{
    /// <summary>
    /// Runtime debugger window for the TimerSystem.
    /// </summary>
    public class TimerSystemDebugWindow : EditorWindow
    {
        private Vector2 _timerScroll;
        private Vector2 _cooldownScroll;
        private double _lastRepaintTime;

        [MenuItem("Shizounu/Timer System/Debugger")]
        public static void Open()
        {
            GetWindow<TimerSystemDebugWindow>("Timer Debugger");
        }

        private void OnEnable()
        {
            EditorApplication.update += HandleEditorUpdate;
        }

        private void OnDisable()
        {
            EditorApplication.update -= HandleEditorUpdate;
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Play Mode", Application.isPlaying ? "Running" : "Stopped");

            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Enter Play Mode to view live timer state.", MessageType.Info);
                return;
            }

            var timers = TimerSystem.Instance.GetTimersSnapshot();
            var cooldowns = TimerSystem.Instance.GetCooldownSnapshots();

            EditorGUILayout.Space();
            DrawTimersSection(timers);
            EditorGUILayout.Space();
            DrawCooldownsSection(cooldowns);
        }

        private void DrawTimersSection(List<TimerSnapshot> timers)
        {
            EditorGUILayout.LabelField("Timers", EditorStyles.boldLabel);

            if (timers.Count == 0)
            {
                EditorGUILayout.LabelField("No active timers.");
                return;
            }

            _timerScroll = EditorGUILayout.BeginScrollView(_timerScroll, GUILayout.Height(180));
            for (int i = 0; i < timers.Count; i++)
            {
                var snapshot = timers[i];
                var info = snapshot.Info;

                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField($"Timer #{i + 1}");
                EditorGUILayout.LabelField("State", info.State.ToString());
                EditorGUILayout.LabelField("Update Mode", info.UpdateMode.ToString());
                EditorGUILayout.LabelField("Duration", info.Duration.ToString("F2"));
                EditorGUILayout.LabelField("Remaining", Mathf.Max(0f, info.Remaining).ToString("F2"));
                EditorGUILayout.LabelField("Repeats", info.RepeatCountRemaining.ToString());
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndScrollView();
        }

        private void DrawCooldownsSection(List<CooldownSnapshot> cooldowns)
        {
            EditorGUILayout.LabelField("Cooldowns", EditorStyles.boldLabel);

            if (cooldowns.Count == 0)
            {
                EditorGUILayout.LabelField("No cooldowns registered.");
                return;
            }

            _cooldownScroll = EditorGUILayout.BeginScrollView(_cooldownScroll, GUILayout.Height(150));
            foreach (var snapshot in cooldowns)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField("Key", snapshot.Key);
                EditorGUILayout.LabelField("Status", snapshot.IsReady ? "Ready" : "Active");
                EditorGUILayout.LabelField("Remaining", Mathf.Max(0f, snapshot.Remaining).ToString("F2"));
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndScrollView();
        }

        private void HandleEditorUpdate()
        {
            if (!Application.isPlaying)
                return;

            double now = EditorApplication.timeSinceStartup;
            if (now - _lastRepaintTime < 0.25)
                return;

            _lastRepaintTime = now;
            Repaint();
        }
    }
}
