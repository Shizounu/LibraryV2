using System.Collections.Generic;
using Shizounu.Library.Editor.DialogueEditor.Elements;
using Shizounu.Library.Editor.DialogueEditor.Windows;
using Shizounu.Library.Dialogue.Data;

namespace Shizounu.Library.Editor.DialogueEditor.Utilities
{
    public sealed class RuntimeContractValidationRule : IDialogueValidationRule
    {
        public int Order => 450;

        public void Validate(DialogueGraphView graphView, List<ValidationIssue> issues)
        {
            foreach (BaseNode node in graphView.NodeCache.Values)
            {
                switch (node)
                {
                    case ConditionalNode conditional:
                        ValidateValueReference(conditional.Value, "Conditional", node, issues);
                        ValidateBlackboardValueType(conditional.Blackboard, conditional.FactKey, "Conditional", node, issues);
                        break;
                    case InformationNode information:
                        ValidateValueReference(information.Value, "Information", node, issues);
                        ValidateBlackboardValueType(information.Blackboard, information.FactKey, "Information", node, issues);
                        break;
                }
            }
        }

        private static void ValidateValueReference(object valueReference, string nodeType, BaseNode node, List<ValidationIssue> issues)
        {
            if (valueReference != null)
                return;

            issues.Add(new ValidationIssue
            {
                Severity = ValidationSeverity.Error,
                Message = $"{nodeType} node is missing a Value reference.",
                Node = node
            });
        }

        private static void ValidateBlackboardValueType(DialogueBlackboard blackboard, string factKey, string nodeType, BaseNode node, List<ValidationIssue> issues)
        {
            if (blackboard == null || string.IsNullOrWhiteSpace(factKey) || !blackboard.HasKey(factKey))
                return;

            if (!blackboard.TryGetValue<object>(factKey, out object value) || value is not int)
            {
                issues.Add(new ValidationIssue
                {
                    Severity = ValidationSeverity.Error,
                    Message = $"{nodeType} node requires blackboard key '{factKey}' to contain an int value.",
                    Node = node
                });
            }
        }
    }
}
