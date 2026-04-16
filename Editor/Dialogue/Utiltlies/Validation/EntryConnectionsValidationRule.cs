using System.Collections.Generic;
using System.Linq;
using Shizounu.Library.Editor.DialogueEditor.Windows;

namespace Shizounu.Library.Editor.DialogueEditor.Utilities
{
    public sealed class EntryConnectionsValidationRule : IDialogueValidationRule
    {
        public int Order => 100;

        public void Validate(DialogueGraphView graphView, List<ValidationIssue> issues)
        {
            if (graphView?.entryNode == null)
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
    }
}
