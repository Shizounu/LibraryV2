using System.Collections.Generic;
using System.Linq;
using Shizounu.Library.Editor.DialogueEditor.Elements;
using Shizounu.Library.Editor.DialogueEditor.Windows;

namespace Shizounu.Library.Editor.DialogueEditor.Utilities
{
    public sealed class DuplicatePrioritiesValidationRule : IDialogueValidationRule
    {
        public int Order => 300;

        public void Validate(DialogueGraphView graphView, List<ValidationIssue> issues)
        {
            foreach (BaseNode node in graphView.NodeCache.Values)
            {
                List<int> duplicatePriorities = node.BranchPorts
                    .GroupBy(port => port.priority)
                    .Where(group => group.Count() > 1)
                    .Select(group => group.Key)
                    .ToList();

                foreach (int priority in duplicatePriorities)
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
    }
}
