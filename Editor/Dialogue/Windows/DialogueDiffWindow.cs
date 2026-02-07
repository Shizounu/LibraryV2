using System.Collections.Generic;
using System.Linq;

using UnityEditor;
using UnityEngine;

using Shizounu.Library.Dialogue.Data;

namespace Shizounu.Library.Editor.DialogueEditor.Windows
{
    /// <summary>
    /// Diff window for comparing two DialogueData assets.
    /// </summary>
    public class DialogueDiffWindow : EditorWindow
    {
        private DialogueData left;
        private DialogueData right;
        private Vector2 scrollPosition;
        private List<string> diffLines = new List<string>();

        [MenuItem("Shizounu/Dialogue/Diff")]
        public static void Open()
        {
            GetWindow<DialogueDiffWindow>("Dialogue Diff");
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Left Dialogue", EditorStyles.boldLabel);
            left = (DialogueData)EditorGUILayout.ObjectField(left, typeof(DialogueData), false);

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Right Dialogue", EditorStyles.boldLabel);
            right = (DialogueData)EditorGUILayout.ObjectField(right, typeof(DialogueData), false);

            EditorGUILayout.Space();

            using (new EditorGUI.DisabledScope(left == null || right == null))
            {
                if (GUILayout.Button("Compare"))
                    Compare();
            }

            using (new EditorGUI.DisabledScope(left == null || right == null))
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Copy Left -> Right"))
                    CopyDialogue(left, right);
                if (GUILayout.Button("Copy Right -> Left"))
                    CopyDialogue(right, left);
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Space();

            if (diffLines.Count == 0)
            {
                EditorGUILayout.HelpBox("No differences found or comparison not run.", MessageType.Info);
                return;
            }

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            foreach (var line in diffLines)
                EditorGUILayout.LabelField(line, EditorStyles.wordWrappedLabel);
            EditorGUILayout.EndScrollView();
        }

        private void Compare()
        {
            diffLines.Clear();

            Dictionary<string, DialogueElement> leftMap = left.Elements.ToDictionary(e => e.ID, e => e);
            Dictionary<string, DialogueElement> rightMap = right.Elements.ToDictionary(e => e.ID, e => e);

            foreach (var id in leftMap.Keys.Except(rightMap.Keys))
            {
                diffLines.Add($"Removed node: {id} ({leftMap[id].GetType().Name})");
            }

            foreach (var id in rightMap.Keys.Except(leftMap.Keys))
            {
                diffLines.Add($"Added node: {id} ({rightMap[id].GetType().Name})");
            }

            foreach (var id in leftMap.Keys.Intersect(rightMap.Keys))
            {
                DialogueElement leftElem = leftMap[id];
                DialogueElement rightElem = rightMap[id];

                if (leftElem.GetType() != rightElem.GetType())
                {
                    diffLines.Add($"Type changed for {id}: {leftElem.GetType().Name} -> {rightElem.GetType().Name}");
                }

                CompareElementFields(id, leftElem, rightElem);
                CompareBranches(id, leftElem, rightElem);
            }

            if (diffLines.Count == 0)
                diffLines.Add("No differences found.");
        }

        private void CopyDialogue(DialogueData source, DialogueData target)
        {
            if (source == null || target == null)
                return;

            if (!EditorUtility.DisplayDialog(
                "Overwrite Dialogue",
                $"This will overwrite '{target.name}' with data from '{source.name}'. Continue?",
                "Overwrite",
                "Cancel"))
            {
                return;
            }

            string json = EditorJsonUtility.ToJson(source, true);
            EditorJsonUtility.FromJsonOverwrite(json, target);
            EditorUtility.SetDirty(target);
            AssetDatabase.SaveAssets();
            Compare();
        }

        private void CompareElementFields(string id, DialogueElement leftElem, DialogueElement rightElem)
        {
            if (leftElem is Sentence leftSentence && rightElem is Sentence rightSentence)
            {
                if (leftSentence.Text != rightSentence.Text)
                    diffLines.Add($"Sentence text changed ({id}).");

                if (leftSentence.LocalizationKey != rightSentence.LocalizationKey || leftSentence.UseLocalization != rightSentence.UseLocalization)
                    diffLines.Add($"Sentence localization settings changed ({id}).");

                if (leftSentence.Speaker != rightSentence.Speaker)
                    diffLines.Add($"Sentence speaker changed ({id}).");
            }
            else if (leftElem is Conditional leftCond && rightElem is Conditional rightCond)
            {
                if (leftCond.FactKey != rightCond.FactKey)
                    diffLines.Add($"Conditional fact key changed ({id}).");

                if (leftCond.Operator != rightCond.Operator)
                    diffLines.Add($"Conditional operator changed ({id}).");
            }
            else if (leftElem is Information leftInfo && rightElem is Information rightInfo)
            {
                if (leftInfo.FactKey != rightInfo.FactKey)
                    diffLines.Add($"Information fact key changed ({id}).");

                if (leftInfo.Operator != rightInfo.Operator)
                    diffLines.Add($"Information operator changed ({id}).");
            }
            else if (leftElem is EventTrigger leftEvent && rightElem is EventTrigger rightEvent)
            {
                if (leftEvent.scriptableEvent != rightEvent.scriptableEvent)
                    diffLines.Add($"EventTrigger event changed ({id}).");
            }
        }

        private void CompareBranches(string id, DialogueElement leftElem, DialogueElement rightElem)
        {
            string leftBranches = string.Join(",", leftElem.Branches.Select(b => $"{b.Priority}:{b.ID}"));
            string rightBranches = string.Join(",", rightElem.Branches.Select(b => $"{b.Priority}:{b.ID}"));

            if (leftBranches != rightBranches)
                diffLines.Add($"Branch connections changed ({id}).");
        }
    }
}
