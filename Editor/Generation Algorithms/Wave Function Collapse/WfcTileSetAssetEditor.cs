using System;
using System.Collections.Generic;
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

        private static readonly string[] DirectionLabels2D = { "Up", "Right", "Down", "Left" };
        private static readonly string[] DirectionLabels3D = { "Up", "Down", "Forward", "Backward", "Right", "Left" };

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

                if (GUILayout.Button("Convert To Direction Flags"))
                    ConvertRulesToDirectionFlags();
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
            SerializedProperty directionProp = element.FindPropertyRelative("Direction");
            SerializedProperty directionIndexProp = element.FindPropertyRelative("DirectionIndex");
            SerializedProperty useDirectionIndexProp = element.FindPropertyRelative("UseDirectionIndex");
            SerializedProperty directionMaskProp = element.FindPropertyRelative("DirectionMask");
            SerializedProperty useDirectionMaskProp = element.FindPropertyRelative("UseDirectionMask");
            SerializedProperty bidirectionalProp = element.FindPropertyRelative("Bidirectional");
            SerializedProperty allowedProp = element.FindPropertyRelative("AllowedNeighbors");

            int directionCount = Mathf.Max(1, _directionCountProp.intValue);
            string[] labels = directionCount == 6 ? DirectionLabels3D : DirectionLabels2D;

            rect.y += 2f;
            float lineHeight = EditorGUIUtility.singleLineHeight;
            float spacing = 2f;

            Rect tileRect = new Rect(rect.x, rect.y, rect.width, lineHeight);
            EditorGUI.PropertyField(tileRect, tileProp, new GUIContent("Tile"));

            rect.y += lineHeight + spacing;
            Rect toggleRect = new Rect(rect.x, rect.y, rect.width, lineHeight);
            Rect directionRect = new Rect(rect.x, rect.y + lineHeight + spacing, rect.width * 0.6f, lineHeight);
            Rect bidirectionalRect = new Rect(directionRect.xMax + 6f, directionRect.y, rect.width - directionRect.width - 6f, lineHeight);

            int currentIndex = GetRuleDirectionIndex(directionProp, directionIndexProp, useDirectionIndexProp, directionCount);
            int clampedIndex = Mathf.Clamp(currentIndex, 0, Mathf.Max(0, directionCount - 1));

            EditorGUI.BeginChangeCheck();
            bool useFlags = EditorGUI.ToggleLeft(toggleRect, "Use Direction Flags", useDirectionMaskProp.boolValue);
            if (EditorGUI.EndChangeCheck())
            {
                useDirectionMaskProp.boolValue = useFlags;
                if (useFlags && directionMaskProp.intValue == 0)
                    directionMaskProp.intValue = 1 << clampedIndex;
            }

            if (useDirectionMaskProp.boolValue)
            {
                int maskValue = directionMaskProp.intValue;
                if (maskValue == 0)
                    maskValue = 1 << clampedIndex;

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
            }
            else if (directionCount == 6)
            {
                int newIndex = EditorGUI.Popup(directionRect, "Direction", clampedIndex, labels);
                directionIndexProp.intValue = newIndex;
                useDirectionIndexProp.boolValue = true;
                directionProp.enumValueIndex = Mathf.Min(newIndex, DirectionLabels2D.Length - 1);
            }
            else
            {
                int newIndex = EditorGUI.Popup(directionRect, "Direction", clampedIndex, labels);
                directionProp.enumValueIndex = Mathf.Min(newIndex, DirectionLabels2D.Length - 1);
                directionIndexProp.intValue = newIndex;
                useDirectionIndexProp.boolValue = false;
            }

            EditorGUI.PropertyField(bidirectionalRect, bidirectionalProp, new GUIContent("Bidirectional"));

            rect.y += (lineHeight + spacing) * 2f;
            Rect allowedRect = new Rect(rect.x, rect.y, rect.width, EditorGUI.GetPropertyHeight(allowedProp, true));
            EditorGUI.PropertyField(allowedRect, allowedProp, new GUIContent("Allowed Neighbors"), true);
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
                    SerializedProperty directionProp = newRule.FindPropertyRelative("Direction");
                    SerializedProperty directionIndexProp = newRule.FindPropertyRelative("DirectionIndex");
                    SerializedProperty useDirectionIndexProp = newRule.FindPropertyRelative("UseDirectionIndex");

                    directionIndexProp.intValue = dir;
                    useDirectionIndexProp.boolValue = directionCount == 6;
                    directionProp.enumValueIndex = Mathf.Min(dir, DirectionLabels2D.Length - 1);
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
                SerializedProperty directionProp = rule.FindPropertyRelative("Direction");
                SerializedProperty directionIndexProp = rule.FindPropertyRelative("DirectionIndex");
                SerializedProperty useDirectionIndexProp = rule.FindPropertyRelative("UseDirectionIndex");
                int ruleDirection = GetRuleDirectionIndex(directionProp, directionIndexProp, useDirectionIndexProp, Mathf.Max(1, _directionCountProp.intValue));
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

        private void ConvertRulesToDirectionFlags()
        {
            int directionCount = Mathf.Max(1, _directionCountProp.intValue);
            var mergedRules = new List<MergedRule>();
            var ruleIndexByKey = new Dictionary<RuleKey, int>();

            for (int i = 0; i < _rulesProp.arraySize; i++)
            {
                SerializedProperty rule = _rulesProp.GetArrayElementAtIndex(i);
                UnityEngine.Object tile = rule.FindPropertyRelative("Tile").objectReferenceValue;
                if (tile == null)
                    continue;

                bool bidirectional = rule.FindPropertyRelative("Bidirectional").boolValue;
                bool useMask = rule.FindPropertyRelative("UseDirectionMask").boolValue;
                int directionMask = rule.FindPropertyRelative("DirectionMask").intValue;

                if (!useMask)
                {
                    SerializedProperty directionProp = rule.FindPropertyRelative("Direction");
                    SerializedProperty directionIndexProp = rule.FindPropertyRelative("DirectionIndex");
                    SerializedProperty useDirectionIndexProp = rule.FindPropertyRelative("UseDirectionIndex");
                    int directionIndex = GetRuleDirectionIndex(directionProp, directionIndexProp, useDirectionIndexProp, directionCount);
                    directionMask = 1 << Mathf.Clamp(directionIndex, 0, 30);
                }

                directionMask = ClampMaskToDirectionCount(directionMask, directionCount);
                if (directionMask == 0)
                    continue;

                SerializedProperty allowedProp = rule.FindPropertyRelative("AllowedNeighbors");
                List<UnityEngine.Object> allowed = CollectAllowedNeighbors(allowedProp);
                string allowedKey = BuildAllowedKey(allowed);

                var key = new RuleKey(tile.GetInstanceID(), bidirectional, allowedKey);
                if (ruleIndexByKey.TryGetValue(key, out int index))
                {
                    MergedRule existing = mergedRules[index];
                    mergedRules[index] = new MergedRule(existing.Tile, existing.Bidirectional, existing.DirectionMask | directionMask, existing.Allowed);
                }
                else
                {
                    mergedRules.Add(new MergedRule(tile, bidirectional, directionMask, allowed));
                    ruleIndexByKey.Add(key, mergedRules.Count - 1);
                }
            }

            _rulesProp.ClearArray();
            for (int i = 0; i < mergedRules.Count; i++)
            {
                _rulesProp.InsertArrayElementAtIndex(i);
                SerializedProperty rule = _rulesProp.GetArrayElementAtIndex(i);
                rule.FindPropertyRelative("Tile").objectReferenceValue = mergedRules[i].Tile;
                rule.FindPropertyRelative("Bidirectional").boolValue = mergedRules[i].Bidirectional;
                rule.FindPropertyRelative("UseDirectionMask").boolValue = true;
                rule.FindPropertyRelative("DirectionMask").intValue = mergedRules[i].DirectionMask;
                rule.FindPropertyRelative("UseDirectionIndex").boolValue = false;
                rule.FindPropertyRelative("DirectionIndex").intValue = 0;
                rule.FindPropertyRelative("Direction").enumValueIndex = 0;

                SerializedProperty allowedProp = rule.FindPropertyRelative("AllowedNeighbors");
                allowedProp.ClearArray();
                for (int a = 0; a < mergedRules[i].Allowed.Count; a++)
                {
                    allowedProp.InsertArrayElementAtIndex(a);
                    allowedProp.GetArrayElementAtIndex(a).objectReferenceValue = mergedRules[i].Allowed[a];
                }
            }
        }

        private static int ClampMaskToDirectionCount(int mask, int directionCount)
        {
            if (directionCount >= 31)
                return mask;

            int maxMask = (1 << directionCount) - 1;
            return mask & maxMask;
        }

        private static List<UnityEngine.Object> CollectAllowedNeighbors(SerializedProperty allowedProp)
        {
            var allowed = new List<UnityEngine.Object>();
            for (int i = 0; i < allowedProp.arraySize; i++)
            {
                UnityEngine.Object obj = allowedProp.GetArrayElementAtIndex(i).objectReferenceValue;
                if (obj != null)
                    allowed.Add(obj);
            }

            return allowed;
        }

        private static string BuildAllowedKey(List<UnityEngine.Object> allowed)
        {
            if (allowed.Count == 0)
                return string.Empty;

            int[] ids = new int[allowed.Count];
            for (int i = 0; i < allowed.Count; i++)
                ids[i] = allowed[i].GetInstanceID();

            Array.Sort(ids);
            return string.Join("|", ids);
        }

        private readonly struct RuleKey : IEquatable<RuleKey>
        {
            private readonly int _tileId;
            private readonly bool _bidirectional;
            private readonly string _allowedKey;

            public RuleKey(int tileId, bool bidirectional, string allowedKey)
            {
                _tileId = tileId;
                _bidirectional = bidirectional;
                _allowedKey = allowedKey ?? string.Empty;
            }

            public bool Equals(RuleKey other)
            {
                return _tileId == other._tileId
                    && _bidirectional == other._bidirectional
                    && _allowedKey == other._allowedKey;
            }

            public override bool Equals(object obj)
            {
                return obj is RuleKey other && Equals(other);
            }

            public override int GetHashCode()
            {
                int hash = _tileId;
                hash = (hash * 397) ^ (_bidirectional ? 1 : 0);
                hash = (hash * 397) ^ _allowedKey.GetHashCode();
                return hash;
            }
        }

        private readonly struct MergedRule
        {
            public readonly UnityEngine.Object Tile;
            public readonly bool Bidirectional;
            public readonly int DirectionMask;
            public readonly List<UnityEngine.Object> Allowed;

            public MergedRule(UnityEngine.Object tile, bool bidirectional, int directionMask, List<UnityEngine.Object> allowed)
            {
                Tile = tile;
                Bidirectional = bidirectional;
                DirectionMask = directionMask;
                Allowed = allowed;
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
            float height = EditorGUIUtility.singleLineHeight * 3f + 8f;
            height += EditorGUI.GetPropertyHeight(allowedProp, true) + 2f;
            return height + 6f;
        }

        private int GetRuleDirectionIndex(SerializedProperty directionProp, SerializedProperty directionIndexProp, SerializedProperty useDirectionIndexProp, int directionCount)
        {
            if (directionCount == 6 && useDirectionIndexProp.boolValue)
                return Mathf.Clamp(directionIndexProp.intValue, 0, directionCount - 1);

            return Mathf.Clamp(directionProp.enumValueIndex, 0, Mathf.Max(0, directionCount - 1));
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
                bool useDirectionMask = rule.FindPropertyRelative("UseDirectionMask").boolValue;
                int directionMask = rule.FindPropertyRelative("DirectionMask").intValue;
                SerializedProperty allowed = rule.FindPropertyRelative("AllowedNeighbors");
                if (tile == null)
                    return "One or more adjacency rules are missing a tile reference.";
                if (useDirectionMask && directionMask == 0)
                    return "One or more adjacency rules have no directions selected.";
                if (allowed.arraySize == 0)
                    return "One or more adjacency rules have no allowed neighbors.";
            }

            return null;
        }
    }
}
