using System.Collections.Generic;
using Shizounu.Library.Editor.DialogueEditor.Elements;
using Shizounu.Library.Editor.DialogueEditor.Windows;

namespace Shizounu.Library.Editor.DialogueEditor.Utilities
{
    public sealed class ReachabilityValidationRule : IDialogueValidationRule
    {
        public int Order => 250;

        public void Validate(DialogueGraphView graphView, List<ValidationIssue> issues)
        {
            if (graphView.entryNode == null)
                return;

            HashSet<string> reachable = CollectReachableNodeIds(graphView.entryNode);

            foreach (BaseNode node in graphView.NodeCache.Values)
            {
                if (node is EntryNode)
                    continue;

                if (string.IsNullOrWhiteSpace(node.UID))
                    continue;

                if (!reachable.Contains(node.UID))
                {
                    issues.Add(new ValidationIssue
                    {
                        Severity = ValidationSeverity.Warning,
                        Message = "Node is not reachable from Entry.",
                        Node = node
                    });
                }
            }
        }

        private static HashSet<string> CollectReachableNodeIds(BaseNode entryNode)
        {
            HashSet<string> visited = new HashSet<string>();
            Queue<BaseNode> queue = new Queue<BaseNode>();
            queue.Enqueue(entryNode);

            while (queue.Count > 0)
            {
                BaseNode current = queue.Dequeue();

                foreach (PriorityPort branchPort in current.BranchPorts)
                {
                    foreach (UnityEditor.Experimental.GraphView.Edge edge in branchPort.port.connections)
                    {
                        if (edge?.input?.node is not BaseNode target)
                            continue;

                        if (string.IsNullOrWhiteSpace(target.UID) || visited.Contains(target.UID))
                            continue;

                        visited.Add(target.UID);
                        queue.Enqueue(target);
                    }
                }
            }

            return visited;
        }
    }
}
