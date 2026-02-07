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
            DialogueManager.ElementEntered += HandleElementEntered;
        }

        private void OnDisable()
        {
            DialogueManager.DialogueStarted -= HandleDialogueStarted;
            DialogueManager.DialogueEnded -= HandleDialogueEnded;
            DialogueManager.ElementEntered -= HandleElementEntered;
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
            Repaint();
        }

        private void HandleDialogueEnded()
        {
            activeDialogue = null;
            activeElement = null;
            Repaint();
        }

        private void HandleElementEntered(DialogueElement element)
        {
            activeElement = element;
            Repaint();
        }
    }
}
