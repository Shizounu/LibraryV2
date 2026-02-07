using System.Collections.Generic;
using System.Linq;

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

            ValidateEntryConnections(graphView, issues);
            ValidateOrphanNodes(graphView, issues);
            ValidateDuplicatePriorities(graphView, issues);
            ValidateBlackboardKeys(graphView, issues);
            ValidateRequiredFields(graphView, issues);

            return issues;
        }

        private static void ValidateEntryConnections(DialogueGraphView graphView, List<ValidationIssue> issues)
        {
            if (graphView.entryNode == null)
                return;

            bool hasConnections = graphView.entryNode.BranchPorts.Any(port => port.port.connected);
            if (!hasConnections)
            {
                issues.Add(new ValidationIssue
                {
                    Severity = ValidationSeverity.Warning,
                    Message = "Entry node has no outgoing connections.",
                    Node = graphView.entryNode
                });
            }
        }

        private static void ValidateOrphanNodes(DialogueGraphView graphView, List<ValidationIssue> issues)
        {
            foreach (var node in graphView.NodeCache.Values)
            {
                if (node is EntryNode)
                    continue;

                bool hasIncoming = node.inputPort != null && node.inputPort.connected;
                if (!hasIncoming)
                {
                    issues.Add(new ValidationIssue
                    {
                        Severity = ValidationSeverity.Warning,
                        Message = "Node has no incoming connections.",
                        Node = node
                    });
                }
            }
        }

        private static void ValidateDuplicatePriorities(DialogueGraphView graphView, List<ValidationIssue> issues)
        {
            foreach (var node in graphView.NodeCache.Values)
            {
                var duplicatePriorities = node.BranchPorts
                    .GroupBy(port => port.priority)
                    .Where(group => group.Count() > 1)
                    .Select(group => group.Key)
                    .ToList();

                foreach (var priority in duplicatePriorities)
                {
                    issues.Add(new ValidationIssue
                    {
                        Severity = ValidationSeverity.Warning,
                        Message = $"Node has duplicate branch priority: {priority}",
                        Node = node
                    });
                }
            }
        }

        private static void ValidateBlackboardKeys(DialogueGraphView graphView, List<ValidationIssue> issues)
        {
            foreach (var node in graphView.NodeCache.Values)
            {
                if (node is ConditionalNode conditional)
                {
                    ValidateBlackboardNode(conditional.Blackboard, conditional.FactKey, node, issues);
                }
                else if (node is InformationNode information)
                {
                    ValidateBlackboardNode(information.Blackboard, information.FactKey, node, issues);
                }
            }
        }

        private static void ValidateBlackboardNode(DialogueBlackboard blackboard, string factKey, BaseNode node, List<ValidationIssue> issues)
        {
            if (blackboard == null)
            {
                issues.Add(new ValidationIssue
                {
                    Severity = ValidationSeverity.Error,
                    Message = "Blackboard is not assigned.",
                    Node = node
                });
                return;
            }

            if (string.IsNullOrWhiteSpace(factKey))
            {
                issues.Add(new ValidationIssue
                {
                    Severity = ValidationSeverity.Warning,
                    Message = "Fact key is empty.",
                    Node = node
                });
                return;
            }

            if (!blackboard.HasKey(factKey))
            {
                issues.Add(new ValidationIssue
                {
                    Severity = ValidationSeverity.Error,
                    Message = $"Fact key not found in blackboard: {factKey}",
                    Node = node
                });
            }
        }

        private static void ValidateRequiredFields(DialogueGraphView graphView, List<ValidationIssue> issues)
        {
            foreach (var node in graphView.NodeCache.Values)
            {
                if (node is SentenceNode sentence)
                {
                    if (string.IsNullOrWhiteSpace(sentence.Text) && !sentence.UseLocalization)
                    {
                        issues.Add(new ValidationIssue
                        {
                            Severity = ValidationSeverity.Info,
                            Message = "Sentence node has empty text.",
                            Node = node
                        });
                    }

                    if (sentence.UseLocalization && string.IsNullOrWhiteSpace(sentence.LocalizationKey))
                    {
                        issues.Add(new ValidationIssue
                        {
                            Severity = ValidationSeverity.Warning,
                            Message = "Localization is enabled but key is empty.",
                            Node = node
                        });
                    }
                }
                else if (node is EventTriggerNode eventTrigger && eventTrigger.scriptableEvent == null)
                {
                    issues.Add(new ValidationIssue
                    {
                        Severity = ValidationSeverity.Warning,
                        Message = "Event Trigger has no ScriptableEvent assigned.",
                        Node = node
                    });
                }
            }
        }
    }
}
