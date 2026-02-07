using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using Shizounu.Library.Dialogue.Data;

namespace Shizounu.Library.Editor.DialogueEditor
{
    /// <summary>
    /// Custom editor for DialogueBlackboard that provides a table view 
    /// for managing key-value pairs.
    /// </summary>
    [CustomEditor(typeof(DialogueBlackboard))]
    public class BlackboardEditor : UnityEditor.Editor
    {
        #region Fields
        
        private SimpleEditorTableView<KeyValuePair<string, object>> table;
        private string newEntryKey = string.Empty;
        
        #endregion

        #region Unity Lifecycle
        
        private void OnEnable()
        {
            if (table == null)
            {
                table = CreateBlackboardTable();
            }
        }

        public override void OnInspectorGUI()
        {
            DialogueBlackboard blackboard = (DialogueBlackboard)target;
            var entries = blackboard.GetAllEntries().ToArray();

            // Draw table
            DrawTable(entries);

            // Draw add new entry controls
            DrawAddEntryControls(blackboard);

            // Mark as dirty if changes were made
            if (GUI.changed)
            {
                EditorUtility.SetDirty(target);
            }
        }
        
        #endregion

        #region Drawing
        
        /// <summary>
        /// Draws the blackboard table.
        /// </summary>
        private void DrawTable(KeyValuePair<string, object>[] entries)
        {
            float tableHeight = EditorGUIUtility.singleLineHeight * 10;
            table.DrawTableGUI(entries, tableHeight);
        }

        /// <summary>
        /// Draws the controls for adding a new entry.
        /// </summary>
        private void DrawAddEntryControls(DialogueBlackboard blackboard)
        {
            EditorGUILayout.BeginHorizontal();

            newEntryKey = EditorGUILayout.TextField("Key", newEntryKey);

            bool canAdd = !string.IsNullOrWhiteSpace(newEntryKey) && !blackboard.HasKey(newEntryKey);
            
            using (new EditorGUI.DisabledScope(!canAdd))
            {
                if (GUILayout.Button("Add"))
                {
                    blackboard.SetValue(newEntryKey, 0);
                    newEntryKey = string.Empty;
                    GUI.FocusControl(null);
                }
            }

            EditorGUILayout.EndHorizontal();

            // Show warning if key already exists
            if (!string.IsNullOrWhiteSpace(newEntryKey) && blackboard.HasKey(newEntryKey))
            {
                EditorGUILayout.HelpBox("A key with this name already exists.", MessageType.Warning);
            }
        }
        
        #endregion

        #region Table Creation
        
        /// <summary>
        /// Creates and configures the blackboard table view.
        /// </summary>
        private SimpleEditorTableView<KeyValuePair<string, object>> CreateBlackboardTable()
        {
            SimpleEditorTableView<KeyValuePair<string, object>> newTable = new SimpleEditorTableView<KeyValuePair<string, object>>();

            // Add Key column
            newTable.AddColumn("Key", 150, DrawKeyColumn)
                .SetSorting((a, b) => string.Compare(a.Key, b.Key, System.StringComparison.Ordinal));

            // Add Type column
            newTable.AddColumn("Type", 100, DrawTypeColumn);

            // Add Value column
            newTable.AddColumn("Value", 150, DrawValueColumn);

            // Add Remove column
            newTable.AddColumn("Remove", 80, DrawRemoveColumn)
                .SetAllowToggleVisibility(true);

            return newTable;
        }
        
        #endregion

        #region Column Drawers
        
        /// <summary>
        /// Draws the key column for an entry.
        /// </summary>
        private void DrawKeyColumn(Rect rect, KeyValuePair<string, object> item)
        {
            rect.xMin += 10;
            EditorGUI.LabelField(rect, item.Key);
        }

        /// <summary>
        /// Draws the type column for an entry.
        /// </summary>
        private void DrawTypeColumn(Rect rect, KeyValuePair<string, object> item)
        {
            string typeName = item.Value != null ? item.Value.GetType().Name : "null";
            EditorGUI.LabelField(rect, typeName);
        }

        /// <summary>
        /// Draws the value column for an entry with appropriate editor control.
        /// </summary>
        private void DrawValueColumn(Rect rect, KeyValuePair<string, object> item)
        {
            DialogueBlackboard blackboard = (DialogueBlackboard)target;

            switch (item.Value)
            {
                case int intValue:
                    DrawIntValue(rect, blackboard, item.Key, intValue);
                    break;

                case float floatValue:
                    DrawFloatValue(rect, blackboard, item.Key, floatValue);
                    break;

                case string stringValue:
                    DrawStringValue(rect, blackboard, item.Key, stringValue);
                    break;

                case bool boolValue:
                    DrawBoolValue(rect, blackboard, item.Key, boolValue);
                    break;

                default:
                    EditorGUI.LabelField(rect, item.Value?.ToString() ?? "null");
                    break;
            }
        }

        /// <summary>
        /// Draws the remove column for an entry.
        /// </summary>
        private void DrawRemoveColumn(Rect rect, KeyValuePair<string, object> item)
        {
            if (GUI.Button(rect, "Remove"))
            {
                DialogueBlackboard blackboard = (DialogueBlackboard)target;
                blackboard.Remove(item.Key);
                EditorUtility.SetDirty(target);
            }
        }
        
        #endregion

        #region Value Editors
        
        /// <summary>
        /// Draws an integer value editor.
        /// </summary>
        private void DrawIntValue(Rect rect, DialogueBlackboard blackboard, string key, int currentValue)
        {
            int newValue = EditorGUI.DelayedIntField(rect, currentValue);
            if (newValue != currentValue)
            {
                blackboard.SetValue(key, newValue);
                EditorUtility.SetDirty(target);
            }
        }

        /// <summary>
        /// Draws a float value editor.
        /// </summary>
        private void DrawFloatValue(Rect rect, DialogueBlackboard blackboard, string key, float currentValue)
        {
            float newValue = EditorGUI.DelayedFloatField(rect, currentValue);
            if (!Mathf.Approximately(newValue, currentValue))
            {
                blackboard.SetValue(key, newValue);
                EditorUtility.SetDirty(target);
            }
        }

        /// <summary>
        /// Draws a string value editor.
        /// </summary>
        private void DrawStringValue(Rect rect, DialogueBlackboard blackboard, string key, string currentValue)
        {
            string newValue = EditorGUI.DelayedTextField(rect, currentValue);
            if (newValue != currentValue)
            {
                blackboard.SetValue(key, newValue);
                EditorUtility.SetDirty(target);
            }
        }

        /// <summary>
        /// Draws a boolean value editor.
        /// </summary>
        private void DrawBoolValue(Rect rect, DialogueBlackboard blackboard, string key, bool currentValue)
        {
            bool newValue = EditorGUI.Toggle(rect, currentValue);
            if (newValue != currentValue)
            {
                blackboard.SetValue(key, newValue);
                EditorUtility.SetDirty(target);
            }
        }
        
        #endregion
    }
}
