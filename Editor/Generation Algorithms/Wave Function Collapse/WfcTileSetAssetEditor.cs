using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Shizounu.Library.GenerationAlgorithms.WaveFunctionCollapse;

namespace Shizounu.Library.Editor
{
    [CustomEditor(typeof(WfcTileSetAsset))]
    public sealed class WfcTileSetAssetEditor : UnityEditor.Editor
    {
        private SerializedProperty _tilesProp;
        private SerializedProperty _rulesProp;
        private ReorderableList _tilesList;
        private ReorderableList _rulesList;

        private void OnEnable()
        {
            _tilesProp = serializedObject.FindProperty("tiles");
            _rulesProp = serializedObject.FindProperty("adjacencyRules");

            _tilesList = new ReorderableList(serializedObject, _tilesProp, true, true, true, true);
            _tilesList.drawHeaderCallback = rect => EditorGUI.LabelField(rect, "Tiles");
            _tilesList.elementHeight = EditorGUIUtility.singleLineHeight + 6f;
            _tilesList.drawElementCallback = DrawTileElement;

            _rulesList = new ReorderableList(serializedObject, _rulesProp, true, true, true, true);
            _rulesList.drawHeaderCallback = rect => EditorGUI.LabelField(rect, "Adjacency Rules");
            _rulesList.elementHeightCallback = index =>
            {
                SerializedProperty element = _rulesProp.GetArrayElementAtIndex(index);
                return EditorGUI.GetPropertyHeight(element, true) + 6f;
            };
            _rulesList.drawElementCallback = DrawRuleElement;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawToolbar();
            EditorGUILayout.Space(4f);
            _tilesList.DoLayoutList();
            EditorGUILayout.Space(6f);
            _rulesList.DoLayoutList();
            DrawValidation();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Add Rules For All Tiles"))
                    AddRulesForAllTiles();

                if (GUILayout.Button("Remove Empty Rules"))
                    RemoveEmptyRules();
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Fill Allowed: All Tiles"))
                    FillAllowedNeighbors(allTiles: true);

                if (GUILayout.Button("Fill Allowed: Same Tile"))
                    FillAllowedNeighbors(allTiles: false);
            }
        }

        private void DrawTileElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty element = _tilesProp.GetArrayElementAtIndex(index);
            SerializedProperty tileProp = element.FindPropertyRelative("Tile");
            SerializedProperty weightProp = element.FindPropertyRelative("Weight");

            rect.y += 2f;
            float lineHeight = EditorGUIUtility.singleLineHeight;
            float weightWidth = Mathf.Min(80f, rect.width * 0.3f);

            Rect tileRect = new Rect(rect.x, rect.y, rect.width - weightWidth - 6f, lineHeight);
            Rect weightRect = new Rect(tileRect.xMax + 6f, rect.y, weightWidth, lineHeight);

            EditorGUI.PropertyField(tileRect, tileProp, GUIContent.none);
            EditorGUI.PropertyField(weightRect, weightProp, GUIContent.none);
        }

        private void DrawRuleElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty element = _rulesProp.GetArrayElementAtIndex(index);
            rect.y += 2f;
            rect.height = EditorGUI.GetPropertyHeight(element, true);
            EditorGUI.PropertyField(rect, element, new GUIContent($"Rule {index + 1}"), true);
        }

        private void AddRulesForAllTiles()
        {
            for (int i = 0; i < _tilesProp.arraySize; i++)
            {
                SerializedProperty tileEntry = _tilesProp.GetArrayElementAtIndex(i);
                SerializedProperty tileProp = tileEntry.FindPropertyRelative("Tile");
                Object tile = tileProp.objectReferenceValue;
                if (tile == null)
                    continue;

                for (int dir = 0; dir < 4; dir++)
                {
                    if (HasRule(tile, dir))
                        continue;

                    int newIndex = _rulesProp.arraySize;
                    _rulesProp.InsertArrayElementAtIndex(newIndex);
                    SerializedProperty newRule = _rulesProp.GetArrayElementAtIndex(newIndex);
                    newRule.FindPropertyRelative("Tile").objectReferenceValue = tile;
                    newRule.FindPropertyRelative("Direction").enumValueIndex = dir;
                    newRule.FindPropertyRelative("Bidirectional").boolValue = true;
                    SerializedProperty allowed = newRule.FindPropertyRelative("AllowedNeighbors");
                    allowed.ClearArray();
                }
            }
        }

        private bool HasRule(Object tile, int direction)
        {
            for (int i = 0; i < _rulesProp.arraySize; i++)
            {
                SerializedProperty rule = _rulesProp.GetArrayElementAtIndex(i);
                Object ruleTile = rule.FindPropertyRelative("Tile").objectReferenceValue;
                int ruleDirection = rule.FindPropertyRelative("Direction").enumValueIndex;
                if (ruleTile == tile && ruleDirection == direction)
                    return true;
            }

            return false;
        }

        private void RemoveEmptyRules()
        {
            for (int i = _rulesProp.arraySize - 1; i >= 0; i--)
            {
                SerializedProperty rule = _rulesProp.GetArrayElementAtIndex(i);
                Object ruleTile = rule.FindPropertyRelative("Tile").objectReferenceValue;
                if (ruleTile == null)
                    _rulesProp.DeleteArrayElementAtIndex(i);
            }
        }

        private void FillAllowedNeighbors(bool allTiles)
        {
            if (_tilesProp.arraySize == 0)
                return;

            for (int i = 0; i < _rulesProp.arraySize; i++)
            {
                SerializedProperty rule = _rulesProp.GetArrayElementAtIndex(i);
                Object ruleTile = rule.FindPropertyRelative("Tile").objectReferenceValue;
                SerializedProperty allowed = rule.FindPropertyRelative("AllowedNeighbors");

                if (ruleTile == null)
                    continue;

                allowed.ClearArray();

                if (allTiles)
                {
                    for (int t = 0; t < _tilesProp.arraySize; t++)
                    {
                        SerializedProperty tileEntry = _tilesProp.GetArrayElementAtIndex(t);
                        Object tile = tileEntry.FindPropertyRelative("Tile").objectReferenceValue;
                        if (tile == null)
                            continue;

                        int insertIndex = allowed.arraySize;
                        allowed.InsertArrayElementAtIndex(insertIndex);
                        allowed.GetArrayElementAtIndex(insertIndex).objectReferenceValue = tile;
                    }
                }
                else
                {
                    allowed.InsertArrayElementAtIndex(0);
                    allowed.GetArrayElementAtIndex(0).objectReferenceValue = ruleTile;
                }
            }
        }

        private void DrawValidation()
        {
            string message = BuildValidationMessage();
            if (!string.IsNullOrEmpty(message))
                EditorGUILayout.HelpBox(message, MessageType.Warning);
        }

        private string BuildValidationMessage()
        {
            if (_tilesProp.arraySize == 0)
                return "Tile set has no tiles.";

            for (int i = 0; i < _tilesProp.arraySize; i++)
            {
                SerializedProperty tileEntry = _tilesProp.GetArrayElementAtIndex(i);
                Object tile = tileEntry.FindPropertyRelative("Tile").objectReferenceValue;
                if (tile == null)
                    return "One or more tiles are missing a reference.";
            }

            for (int i = 0; i < _rulesProp.arraySize; i++)
            {
                SerializedProperty rule = _rulesProp.GetArrayElementAtIndex(i);
                Object tile = rule.FindPropertyRelative("Tile").objectReferenceValue;
                SerializedProperty allowed = rule.FindPropertyRelative("AllowedNeighbors");
                if (tile == null)
                    return "One or more adjacency rules are missing a tile reference.";
                if (allowed.arraySize == 0)
                    return "One or more adjacency rules have no allowed neighbors.";
            }

            return null;
        }
    }
}
