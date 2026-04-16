using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

using Shizounu.Library.Editor.DialogueEditor.Utilities;

namespace Shizounu.Library.Editor.DialogueEditor.Windows
{
    /// <summary>
    /// Window for validating dialogue graphs and listing issues.
    /// </summary>
    public class DialogueValidationWindow : EditorWindow
    {
        private Vector2 scrollPosition;
        private List<ValidationIssue> issues = new List<ValidationIssue>();

        [MenuItem("Shizounu/Dialogue/Validation")]
        public static void Open()
        {
            GetWindow<DialogueValidationWindow>("Dialogue Validation");
        }

        private void OnEnable()
        {
            RefreshIssues();
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Refresh"))
                RefreshIssues();
            EditorGUILayout.EndHorizontal();

            if (issues.Count == 0)
            {
                EditorGUILayout.HelpBox("No validation issues found.", MessageType.Info);
                return;
            }

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            foreach (var issue in issues)
            {
                DrawIssue(issue);
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawIssue(ValidationIssue issue)
        {
            MessageType messageType = issue.Severity switch
            {
                ValidationSeverity.Info => MessageType.Info,
                ValidationSeverity.Warning => MessageType.Warning,
                ValidationSeverity.Error => MessageType.Error,
                _ => MessageType.None
            };

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.HelpBox(issue.Message, messageType);

            using (new EditorGUI.DisabledScope(issue.Node == null))
            {
                if (GUILayout.Button("Focus", GUILayout.Width(60)))
                {
                    FocusNode(issue);
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        private void FocusNode(ValidationIssue issue)
        {
            DialogueEditorWindow activeWindow = DialogueEditorWindow.ActiveWindow;
            if (activeWindow == null || issue.Node == null)
                return;

            var graphView = activeWindow.GraphView;
            if (graphView == null)
                return;

            graphView.ClearSelection();
            graphView.AddToSelection(issue.Node);
            graphView.FrameSelection();
        }

        private void RefreshIssues()
        {
            DialogueEditorWindow activeWindow = DialogueEditorWindow.ActiveWindow;
            if (activeWindow == null)
            {
                issues = new List<ValidationIssue>
                {
                    new ValidationIssue
                    {
                        Severity = ValidationSeverity.Error,
                        Message = "No active Dialogue Editor window found."
                    }
                };
                return;
            }

            issues = DialogueValidationUtility.ValidateGraph(activeWindow.GraphView);
            issues.AddRange(DialogueValidationUtility.ValidateDialogueData(activeWindow.LoadedData, activeWindow.GraphView));
            Repaint();
        }
    }
}
