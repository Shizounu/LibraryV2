using System.Collections.Generic;
using System.Linq;
using Shizounu.Library.Editor.DialogueEditor.Elements;
using Shizounu.Library.Editor.DialogueEditor.Windows;

namespace Shizounu.Library.Editor.DialogueEditor.Utilities
{
    public sealed class NodeIdentityValidationRule : IDialogueValidationRule
    {
        public int Order => 50;

        public void Validate(DialogueGraphView graphView, List<ValidationIssue> issues)
        {
            List<BaseNode> nodes = graphView.NodeCache.Values.ToList();

            foreach (BaseNode node in nodes)
            {
                if (node is EntryNode)
                    continue;

                if (string.IsNullOrWhiteSpace(node.UID))
                {
                    issues.Add(new ValidationIssue
                    {
                        Severity = ValidationSeverity.Error,
                        Message = "Node is missing a UID.",
                        Node = node
                    });
                }
            }

            foreach (IGrouping<string, BaseNode> duplicateGroup in nodes
                         .Where(node => !string.IsNullOrWhiteSpace(node.UID))
                         .GroupBy(node => node.UID)
                         .Where(group => group.Count() > 1))
            {
                foreach (BaseNode node in duplicateGroup)
                {
                    issues.Add(new ValidationIssue
                    {
                        Severity = ValidationSeverity.Error,
                        Message = $"Duplicate node UID detected: {duplicateGroup.Key}",
                        Node = node
                    });
                }
            }
        }
    }
}
