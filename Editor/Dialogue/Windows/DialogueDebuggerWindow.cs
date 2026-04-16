using UnityEditor;
using UnityEngine;

using Shizounu.Library.Dialogue.Data;
using Shizounu.Library.Dialogue;

namespace Shizounu.Library.Editor.DialogueEditor.Windows
{
    /// <summary>
    /// Runtime debugger window for live dialogue playback.
    /// </summary>
    public class DialogueDebuggerWindow : EditorWindow
    {
        private DialogueData activeDialogue;
        private DialogueElement activeElement;
        private DialogueExecutionState executionState;
        private bool wasCancelled;
        private Vector2 scrollPosition;

        [MenuItem("Shizounu/Dialogue/Debugger")]
        public static void Open()
        {
            GetWindow<DialogueDebuggerWindow>("Dialogue Debugger");
        }

        private void OnEnable()
        {
            DialogueManager.DialogueStarted += HandleDialogueStarted;
            DialogueManager.DialogueEnded += HandleDialogueEnded;
            DialogueManager.DialogueCancelled += HandleDialogueCancelled;
            DialogueManager.ElementEntered += HandleElementEntered;
            DialogueManager.ElementExited += HandleElementExited;
            DialogueManager.ExecutionStateChanged += HandleExecutionStateChanged;
        }

        private void OnDisable()
        {
            DialogueManager.DialogueStarted -= HandleDialogueStarted;
            DialogueManager.DialogueEnded -= HandleDialogueEnded;
            DialogueManager.DialogueCancelled -= HandleDialogueCancelled;
            DialogueManager.ElementEntered -= HandleElementEntered;
            DialogueManager.ElementExited -= HandleElementExited;
            DialogueManager.ExecutionStateChanged -= HandleExecutionStateChanged;
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Play Mode", Application.isPlaying ? "Running" : "Stopped");

            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Enter Play Mode to view live dialogue state.", MessageType.Info);
                return;
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Active Dialogue", activeDialogue != null ? activeDialogue.name : "None");
            EditorGUILayout.LabelField("Active Element", activeElement != null ? activeElement.ID : "None");
            EditorGUILayout.LabelField("Element Type", activeElement != null ? activeElement.GetType().Name : "None");
            EditorGUILayout.LabelField("Execution State", executionState.ToString());
            EditorGUILayout.LabelField("Last Exit", wasCancelled ? "Cancelled" : "Completed");

            if (activeElement is Sentence sentence)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Speaker", sentence.Speaker != null ? sentence.Speaker.name : "None");

                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(100));
                EditorGUILayout.TextArea(sentence.GetDisplayText());
                EditorGUILayout.EndScrollView();
            }
        }

        private void HandleDialogueStarted(DialogueData dialogue)
        {
            activeDialogue = dialogue;
            wasCancelled = false;
            Repaint();
        }

        private void HandleDialogueEnded()
        {
            activeDialogue = null;
            activeElement = null;
            executionState = DialogueExecutionState.Idle;
            Repaint();
        }

        private void HandleDialogueCancelled()
        {
            wasCancelled = true;
            Repaint();
        }

        private void HandleElementEntered(DialogueElement element)
        {
            activeElement = element;
            wasCancelled = false;
            Repaint();
        }

        private void HandleElementExited(DialogueElement element)
        {
            if (activeElement == element)
                activeElement = null;

            Repaint();
        }

        private void HandleExecutionStateChanged(DialogueExecutionState state)
        {
            executionState = state;
            Repaint();
        }
    }
}
