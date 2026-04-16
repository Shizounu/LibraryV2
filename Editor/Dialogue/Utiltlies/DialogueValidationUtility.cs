using System.Collections.Generic;
using System.Linq;
using UnityEditor;

using UnityEngine;

using Shizounu.Library.Dialogue.Data;
using Shizounu.Library.Editor.DialogueEditor.Elements;
using Shizounu.Library.Editor.DialogueEditor.Windows;

namespace Shizounu.Library.Editor.DialogueEditor.Utilities
{
    public enum ValidationSeverity
    {
        Info,
        Warning,
        Error
    }

    public class ValidationIssue
    {
        public ValidationSeverity Severity;
        public string Message;
        public BaseNode Node;
    }

    /// <summary>
    /// Validation rules for dialogue graphs.
    /// </summary>
    public static class DialogueValidationUtility
    {
        private static List<IDialogueValidationRule> _cachedRules;

        public static List<ValidationIssue> ValidateGraph(DialogueGraphView graphView)
        {
            List<ValidationIssue> issues = new List<ValidationIssue>();

            if (graphView == null)
            {
                issues.Add(new ValidationIssue
                {
                    Severity = ValidationSeverity.Error,
                    Message = "No active graph view found."
                });
                return issues;
            }

            foreach (IDialogueValidationRule rule in GetRules())
            {
                rule.Validate(graphView, issues);
            }

            return issues;
        }

        public static bool HasErrors(List<ValidationIssue> issues)
        {
            if (issues == null)
                return false;

            return issues.Any(issue => issue != null && issue.Severity == ValidationSeverity.Error);
        }

        public static List<ValidationIssue> ValidateDialogueData(DialogueData dialogueData, DialogueGraphView graphView)
        {
            List<ValidationIssue> issues = new List<ValidationIssue>();
            if (dialogueData == null)
                return issues;

            foreach (DialogueDataReferenceIssue issue in DialogueDataReferenceValidator.ValidateDetailed(dialogueData))
            {
                issues.Add(new ValidationIssue
                {
                    Severity = ValidationSeverity.Error,
                    Message = issue.Message,
                    Node = ResolveNode(issue.OwnerId, graphView)
                });
            }

            foreach (DialogueElement element in dialogueData.Elements)
            {
                if (element == null)
                    continue;

                if (!IsSupportedElementType(element))
                {
                    issues.Add(new ValidationIssue
                    {
                        Severity = ValidationSeverity.Error,
                        Message = $"Unsupported dialogue element type: {element.GetType().Name} ({GetDisplayId(element.ID)}).",
                        Node = ResolveNode(element.ID, graphView)
                    });
                }
            }

#if UNITY_EDITOR
            HashSet<string> knownNodeIds = dialogueData.Elements
                .Where(element => element != null && !string.IsNullOrWhiteSpace(element.ID))
                .Select(element => element.ID)
                .ToHashSet();

            foreach (GroupData groupData in dialogueData.groupDatas)
            {
                if (groupData == null)
                {
                    issues.Add(new ValidationIssue
                    {
                        Severity = ValidationSeverity.Error,
                        Message = "Dialogue data contains a null group entry."
                    });
                    continue;
                }

                foreach (string nodeId in groupData.NodeIDs ?? new List<string>())
                {
                    if (string.IsNullOrWhiteSpace(nodeId) || knownNodeIds.Contains(nodeId))
                        continue;

                    issues.Add(new ValidationIssue
                    {
                        Severity = ValidationSeverity.Warning,
                        Message = $"Group '{groupData.Title}' references missing node '{nodeId}'.",
                        Node = ResolveNode(nodeId, graphView)
                    });
                }
            }
#endif

            return issues
                .GroupBy(issue => $"{issue.Severity}|{issue.Message}|{issue.Node?.GetHashCode() ?? 0}")
                .Select(group => group.First())
                .ToList();
        }

        private static bool IsSupportedElementType(DialogueElement element)
        {
            return element is Sentence
                || element is Choice
                || element is Conditional
                || element is Information
                || element is EventTrigger;
        }

        private static BaseNode ResolveNode(string ownerId, DialogueGraphView graphView)
        {
            if (graphView == null || string.IsNullOrWhiteSpace(ownerId) || ownerId == "Entry")
                return null;

            graphView.NodeCache.TryGetValue(ownerId, out BaseNode node);
            return node;
        }

        private static string GetDisplayId(string id)
        {
            return string.IsNullOrWhiteSpace(id) ? "missing-id" : id;
        }

        private static IEnumerable<IDialogueValidationRule> GetRules()
        {
            if (_cachedRules != null)
                return _cachedRules;

            _cachedRules = new List<IDialogueValidationRule>();

            foreach (System.Type ruleType in TypeCache.GetTypesDerivedFrom<IDialogueValidationRule>())
            {
                if (ruleType == null || ruleType.IsAbstract || ruleType.IsInterface)
                    continue;

                if (System.Activator.CreateInstance(ruleType) is IDialogueValidationRule rule)
                {
                    _cachedRules.Add(rule);
                }
            }

            _cachedRules = _cachedRules
                .OrderBy(rule => rule.Order)
                .ThenBy(rule => rule.GetType().Name)
                .ToList();

            return _cachedRules;
        }
    }
}
