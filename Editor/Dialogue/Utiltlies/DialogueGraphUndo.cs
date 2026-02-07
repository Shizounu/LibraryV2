using UnityEditor;
using UnityEngine;

using Shizounu.Library.Editor.DialogueEditor.Windows;

namespace Shizounu.Library.Editor.DialogueEditor.Utilities
{
    /// <summary>
    /// Manages undo/redo for dialogue graph edits using snapshots.
    /// </summary>
    public class DialogueGraphUndo
    {
        private readonly DialogueGraphView graphView;
        private DialogueGraphSnapshot snapshot;
        private bool isRestoring;
        private bool isInitialized;

        public bool IsRestoring => isRestoring;

        public DialogueGraphUndo(DialogueGraphView graphView)
        {
            this.graphView = graphView;
        }

        public void Initialize()
        {
            if (isInitialized)
                return;

            snapshot = ScriptableObject.CreateInstance<DialogueGraphSnapshot>();
            snapshot.hideFlags = HideFlags.HideAndDontSave;

            RecordSnapshot("Initialize Graph");
            Undo.undoRedoPerformed += HandleUndoRedo;

            isInitialized = true;
        }

        public void Dispose()
        {
            Undo.undoRedoPerformed -= HandleUndoRedo;
        }

        public void RecordSnapshot(string actionName)
        {
            if (isRestoring || graphView == null)
                return;

            Undo.RecordObject(snapshot, actionName);
            DialogueGraphSnapshotUtility.WriteSnapshot(graphView, snapshot);
            EditorUtility.SetDirty(snapshot);
        }

        public void BeginRestore()
        {
            isRestoring = true;
        }

        public void EndRestore()
        {
            isRestoring = false;
        }

        private void HandleUndoRedo()
        {
            if (graphView == null || snapshot == null)
                return;

            BeginRestore();
            DialogueGraphSnapshotUtility.ApplySnapshot(snapshot, graphView);
            EndRestore();
        }
    }
}
