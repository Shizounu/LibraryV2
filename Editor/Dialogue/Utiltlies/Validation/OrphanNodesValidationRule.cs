using System.Collections.Generic;
using Shizounu.Library.Editor.DialogueEditor.Elements;
using Shizounu.Library.Editor.DialogueEditor.Windows;

namespace Shizounu.Library.Editor.DialogueEditor.Utilities
{
    public sealed class OrphanNodesValidationRule : IDialogueValidationRule
    {
        public int Order => 200;

        public void Validate(DialogueGraphView graphView, List<ValidationIssue> issues)
        {
            foreach (BaseNode node in graphView.NodeCache.Values)
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
    }
}
