using System.Collections.Generic;
using Shizounu.Library.Dialogue.Data;
using Shizounu.Library.Editor.DialogueEditor.Elements;
using Shizounu.Library.Editor.DialogueEditor.Windows;

namespace Shizounu.Library.Editor.DialogueEditor.Utilities
{
    public sealed class BlackboardKeysValidationRule : IDialogueValidationRule
    {
        public int Order => 400;

        public void Validate(DialogueGraphView graphView, List<ValidationIssue> issues)
        {
            foreach (BaseNode node in graphView.NodeCache.Values)
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
                    Severity = ValidationSeverity.Error,
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
    }
}
