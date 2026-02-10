using System;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Shizounu.Library.GenerationAlgorithms.WaveFunctionCollapse;

namespace Shizounu.Library.Editor
{
    [CustomEditor(typeof(WfcTileSetAsset))]
    public sealed class WfcTileSetAssetEditor : UnityEditor.Editor
    {
        [Flags]
        private enum DirectionMask2D
        {
            Up = 1 << 0,
            Right = 1 << 1,
            Down = 1 << 2,
            Left = 1 << 3
        }

        [Flags]
        private enum DirectionMask3D
        {
            Up = 1 << 0,
            Down = 1 << 1,
            Forward = 1 << 2,
            Backward = 1 << 3,
            Right = 1 << 4,
            Left = 1 << 5
        }
        private SerializedProperty _tilesProp;
        private SerializedProperty _rulesProp;
        private SerializedProperty _directionCountProp;
        private ReorderableList _tilesList;
        private ReorderableList _rulesList;

        private void OnEnable()
        {
            _tilesProp = serializedObject.FindProperty("tiles");
            _rulesProp = serializedObject.FindProperty("adjacencyRules");
            _directionCountProp = serializedObject.FindProperty("directionCount");

            _tilesList = new ReorderableList(serializedObject, _tilesProp, true, true, true, true);
            _tilesList.drawHeaderCallback = rect => EditorGUI.LabelField(rect, "Tiles");
            _tilesList.elementHeight = EditorGUIUtility.singleLineHeight + 6f;
            _tilesList.drawElementCallback = DrawTileElement;

            _rulesList = new ReorderableList(serializedObject, _rulesProp, true, true, true, true);
            _rulesList.drawHeaderCallback = rect => EditorGUI.LabelField(rect, "Adjacency Rules");
            _rulesList.elementHeightCallback = GetRuleElementHeight;
            _rulesList.drawElementCallback = DrawRuleElement;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_directionCountProp, new GUIContent("Direction Count (4=2D, 6=3D)"));
            if (_directionCountProp.intValue < 1)
                _directionCountProp.intValue = 1;

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
            SerializedProperty tileProp = element.FindPropertyRelative("Tile");
            SerializedProperty directionMaskProp = element.FindPropertyRelative("DirectionMask");
            SerializedProperty bidirectionalProp = element.FindPropertyRelative("Bidirectional");
            SerializedProperty allowedProp = element.FindPropertyRelative("AllowedNeighbors");

            int directionCount = Mathf.Max(1, _directionCountProp.intValue);

            rect.y += 2f;
            float lineHeight = EditorGUIUtility.singleLineHeight;
            float spacing = 2f;

            Rect tileRect = new Rect(rect.x, rect.y, rect.width, lineHeight);
            EditorGUI.PropertyField(tileRect, tileProp, new GUIContent("Tile"));

            rect.y += lineHeight + spacing;
            Rect directionRect = new Rect(rect.x, rect.y, rect.width * 0.6f, lineHeight);
            Rect bidirectionalRect = new Rect(directionRect.xMax + 6f, rect.y, rect.width - directionRect.width - 6f, lineHeight);

            int maskValue = ClampMaskToDirectionCount(directionMaskProp.intValue, directionCount);
            if (maskValue == 0)
                maskValue = 1;

            if (directionCount == 6)
            {
                DirectionMask3D newMask = (DirectionMask3D)EditorGUI.EnumFlagsField(directionRect, "Directions", (DirectionMask3D)maskValue);
                directionMaskProp.intValue = Convert.ToInt32(newMask);
            }
            else
            {
                DirectionMask2D newMask = (DirectionMask2D)EditorGUI.EnumFlagsField(directionRect, "Directions", (DirectionMask2D)maskValue);
                directionMaskProp.intValue = Convert.ToInt32(newMask);
            }

            EditorGUI.PropertyField(bidirectionalRect, bidirectionalProp, new GUIContent("Bidirectional"));

            rect.y += lineHeight + spacing;
            Rect allowedRect = new Rect(rect.x, rect.y, rect.width, EditorGUI.GetPropertyHeight(allowedProp, true));
            EditorGUI.PropertyField(allowedRect, allowedProp, new GUIContent("Allowed Neighbors"), true);
        }

        private static int ClampMaskToDirectionCount(int mask, int directionCount)
        {
            if (directionCount >= 31)
                return mask;

            int maxMask = (1 << directionCount) - 1;
            return mask & maxMask;
        }

        private void AddRulesForAllTiles()
        {
            for (int i = 0; i < _tilesProp.arraySize; i++)
            {
                SerializedProperty tileEntry = _tilesProp.GetArrayElementAtIndex(i);
                SerializedProperty tileProp = tileEntry.FindPropertyRelative("Tile");
                UnityEngine.Object tile = tileProp.objectReferenceValue;
                if (tile == null)
                    continue;

                int directionCount = Mathf.Max(1, _directionCountProp.intValue);
                for (int dir = 0; dir < directionCount; dir++)
                {
                    if (HasRule(tile, dir))
                        continue;

                    int newIndex = _rulesProp.arraySize;
                    _rulesProp.InsertArrayElementAtIndex(newIndex);
                    SerializedProperty newRule = _rulesProp.GetArrayElementAtIndex(newIndex);
                    newRule.FindPropertyRelative("Tile").objectReferenceValue = tile;
                    newRule.FindPropertyRelative("DirectionMask").intValue = 1 << dir;
                    newRule.FindPropertyRelative("Bidirectional").boolValue = true;
                    SerializedProperty allowed = newRule.FindPropertyRelative("AllowedNeighbors");
                    allowed.ClearArray();
                }
            }
        }

        private bool HasRule(UnityEngine.Object tile, int direction)
        {
            for (int i = 0; i < _rulesProp.arraySize; i++)
            {
                SerializedProperty rule = _rulesProp.GetArrayElementAtIndex(i);
                UnityEngine.Object ruleTile = rule.FindPropertyRelative("Tile").objectReferenceValue;
                int directionMask = rule.FindPropertyRelative("DirectionMask").intValue;
                if (ruleTile == tile && (directionMask & (1 << direction)) != 0)
                    return true;
            }

            return false;
        }

        private void RemoveEmptyRules()
        {
            for (int i = _rulesProp.arraySize - 1; i >= 0; i--)
            {
                SerializedProperty rule = _rulesProp.GetArrayElementAtIndex(i);
                UnityEngine.Object ruleTile = rule.FindPropertyRelative("Tile").objectReferenceValue;
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
                UnityEngine.Object ruleTile = rule.FindPropertyRelative("Tile").objectReferenceValue;
                SerializedProperty allowed = rule.FindPropertyRelative("AllowedNeighbors");

                if (ruleTile == null)
                    continue;

                allowed.ClearArray();

                if (allTiles)
                {
                    for (int t = 0; t < _tilesProp.arraySize; t++)
                    {
                        SerializedProperty tileEntry = _tilesProp.GetArrayElementAtIndex(t);
                        UnityEngine.Object tile = tileEntry.FindPropertyRelative("Tile").objectReferenceValue;
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

        private float GetRuleElementHeight(int index)
        {
            SerializedProperty element = _rulesProp.GetArrayElementAtIndex(index);
            SerializedProperty allowedProp = element.FindPropertyRelative("AllowedNeighbors");
            float height = EditorGUIUtility.singleLineHeight * 2f + 6f;
            height += EditorGUI.GetPropertyHeight(allowedProp, true) + 2f;
            return height + 6f;
        }

        private string BuildValidationMessage()
        {
            if (_tilesProp.arraySize == 0)
                return "Tile set has no tiles.";

            for (int i = 0; i < _tilesProp.arraySize; i++)
            {
                SerializedProperty tileEntry = _tilesProp.GetArrayElementAtIndex(i);
                UnityEngine.Object tile = tileEntry.FindPropertyRelative("Tile").objectReferenceValue;
                if (tile == null)
                    return "One or more tiles are missing a reference.";
            }

            for (int i = 0; i < _rulesProp.arraySize; i++)
            {
                SerializedProperty rule = _rulesProp.GetArrayElementAtIndex(i);
                UnityEngine.Object tile = rule.FindPropertyRelative("Tile").objectReferenceValue;
                int directionMask = rule.FindPropertyRelative("DirectionMask").intValue;
                SerializedProperty allowed = rule.FindPropertyRelative("AllowedNeighbors");
                if (tile == null)
                    return "One or more adjacency rules are missing a tile reference.";
                if (directionMask == 0)
                    return "One or more adjacency rules have no directions selected.";
                if (allowed.arraySize == 0)
                    return "One or more adjacency rules have no allowed neighbors.";
            }

            return null;
        }
    }
}
