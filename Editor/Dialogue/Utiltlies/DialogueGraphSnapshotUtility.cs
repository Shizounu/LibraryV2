using UnityEditor;
using UnityEngine;

using Shizounu.Library.Dialogue.Data;
using Shizounu.Library.Editor.DialogueEditor.Windows;

namespace Shizounu.Library.Editor.DialogueEditor.Utilities
{
    /// <summary>
    /// Utilities for writing and restoring dialogue graph snapshots.
    /// </summary>
    public static class DialogueGraphSnapshotUtility
    {
        public static void WriteSnapshot(DialogueGraphView graphView, DialogueGraphSnapshot snapshot)
        {
            if (graphView == null || snapshot == null)
                return;

            DialogueData data = SavingUtility.BuildData(graphView);

            string json = EditorJsonUtility.ToJson(data, true);
            EditorJsonUtility.FromJsonOverwrite(json, snapshot);
        }

        public static void ApplySnapshot(DialogueGraphSnapshot snapshot, DialogueGraphView graphView)
        {
            if (graphView == null || snapshot == null)
                return;

            DialogueData data = ScriptableObject.CreateInstance<DialogueData>();
            string json = EditorJsonUtility.ToJson(snapshot, true);
            EditorJsonUtility.FromJsonOverwrite(json, data);

            SavingUtility.LoadFromData(data, graphView);
        }
    }
}
