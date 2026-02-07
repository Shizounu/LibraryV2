using System;
using System.Collections.Generic;
using System.Linq;

using UnityEditor;
using UnityEngine;
using UnityEditor.Experimental.GraphView;

using Shizounu.Library.Dialogue.Data;
using Shizounu.Library.Editor.DialogueEditor.Elements;
using Shizounu.Library.Editor.DialogueEditor.Windows;

namespace Shizounu.Library.Editor.DialogueEditor.Utilities
{
    /// <summary>
    /// Utilities for creating and instantiating dialogue templates.
    /// </summary>
    public static class DialogueTemplateUtility
    {
        public static DialogueTemplate CreateTemplateFromSelection(DialogueGraphView graphView)
        {
            if (graphView == null)
                return null;

            List<BaseNode> selectedNodes = graphView.selection
                .OfType<BaseNode>()
                .Where(node => node is not EntryNode)
                .ToList();

            if (selectedNodes.Count == 0)
                return null;

            DialogueTemplate template = ScriptableObject.CreateInstance<DialogueTemplate>();
            HashSet<string> selectedIds = selectedNodes.Select(node => node.UID).ToHashSet();

            foreach (var node in selectedNodes)
            {
                DialogueElement element = node.GetElement();
                if (element == null)
                    continue;

                element.Branches = GetBranchesFiltered(node, selectedIds);
                template.Elements.Add(element);
            }

            template.groupDatas = BuildGroupData(graphView, selectedIds);
            template.AnchorPosition = CalculateAnchorPosition(selectedNodes);

            return template;
        }

        public static void InstantiateTemplate(DialogueTemplate template, DialogueGraphView graphView, Vector2 spawnPosition)
        {
            if (template == null || graphView == null)
                return;

            Dictionary<string, string> idMap = new Dictionary<string, string>();
            List<DialogueElement> clonedElements = new List<DialogueElement>();

            foreach (var element in template.Elements)
            {
                DialogueElement clone = CloneElement(element);
                string newId = DialogueData.GetID();

                idMap[element.ID] = newId;
                clone.ID = newId;

                Vector2 offset = element.NodePosition.position - template.AnchorPosition;
                clone.NodePosition = new Rect(spawnPosition + offset, element.NodePosition.size);

                clonedElements.Add(clone);
            }

            foreach (var element in clonedElements)
            {
                element.Branches = element.Branches
                    .Where(branch => idMap.ContainsKey(branch.ID))
                    .Select(branch => new PriorityIDTuple(branch.Priority, idMap[branch.ID]))
                    .ToList();
            }

            List<GroupData> groups = RemapGroups(template.groupDatas, idMap, spawnPosition, template.AnchorPosition);

            foreach (var element in clonedElements)
            {
                BaseNode node = graphView.CreateNode(GetNodeType(element), element.NodePosition.position, element, false);
                if (node != null)
                {
                    graphView.AddElement(node);
                }
            }

            foreach (var element in clonedElements)
            {
                foreach (var branch in element.Branches)
                {
                    if (graphView.NodeCache.TryGetValue(element.ID, out BaseNode from) &&
                        graphView.NodeCache.TryGetValue(branch.ID, out BaseNode to))
                    {
                        graphView.AddElement(from.GetPortWithPriority(branch.Priority).port.ConnectTo(to.inputPort));
                    }
                }
            }

            foreach (var groupData in groups)
            {
                Group group = new Group
                {
                    title = groupData.Title
                };

                group.SetPosition(groupData.position);

                foreach (var nodeId in groupData.NodeIDs)
                {
                    if (graphView.NodeCache.TryGetValue(nodeId, out BaseNode node))
                        group.AddElement(node);
                }

                graphView.Groups.Add(group);
                graphView.Add(group);
            }

            graphView.RecordUndo("Instantiate Template");
        }

        private static List<PriorityIDTuple> GetBranchesFiltered(BaseNode node, HashSet<string> allowedIds)
        {
            List<PriorityIDTuple> branches = new List<PriorityIDTuple>();

            foreach (var priorityPort in node.BranchPorts)
            {
                foreach (var connection in priorityPort.port.connections)
                {
                    if (connection.input.node is BaseNode targetNode && allowedIds.Contains(targetNode.UID))
                    {
                        branches.Add(new PriorityIDTuple(priorityPort.priority, targetNode.UID));
                    }
                }
            }

            return branches;
        }

        private static List<GroupData> BuildGroupData(DialogueGraphView graphView, HashSet<string> selectedIds)
        {
            List<GroupData> groupDatas = new List<GroupData>();

            foreach (var group in graphView.Groups)
            {
                List<string> containedIds = group.containedElements
                    .OfType<BaseNode>()
                    .Select(node => node.UID)
                    .Where(selectedIds.Contains)
                    .ToList();

                if (containedIds.Count == 0)
                    continue;

                groupDatas.Add(new GroupData
                {
                    Title = group.title,
                    position = group.GetPosition(),
                    NodeIDs = containedIds
                });
            }

            return groupDatas;
        }

        private static Vector2 CalculateAnchorPosition(List<BaseNode> nodes)
        {
            if (nodes.Count == 0)
                return Vector2.zero;

            Vector2 sum = Vector2.zero;
            foreach (var node in nodes)
                sum += node.GetPosition().position;

            return sum / nodes.Count;
        }

        private static DialogueElement CloneElement(DialogueElement element)
        {
            DialogueElement clone = (DialogueElement)Activator.CreateInstance(element.GetType());
            string json = EditorJsonUtility.ToJson(element, true);
            EditorJsonUtility.FromJsonOverwrite(json, clone);
            return clone;
        }

        private static NodeType GetNodeType(DialogueElement element)
        {
            return element switch
            {
                Conditional => NodeType.Condition,
                Information => NodeType.Information,
                Sentence => NodeType.SentenceNode,
                EventTrigger => NodeType.EventTrigger,
                _ => NodeType.SentenceNode
            };
        }

        private static List<GroupData> RemapGroups(
            List<GroupData> sourceGroups,
            Dictionary<string, string> idMap,
            Vector2 spawnPosition,
            Vector2 anchorPosition)
        {
            List<GroupData> groups = new List<GroupData>();

            foreach (var group in sourceGroups)
            {
                List<string> remappedIds = group.NodeIDs
                    .Where(idMap.ContainsKey)
                    .Select(id => idMap[id])
                    .ToList();

                if (remappedIds.Count == 0)
                    continue;

                Rect position = group.position;
                Vector2 offset = position.position - anchorPosition;
                position.position = spawnPosition + offset;

                groups.Add(new GroupData
                {
                    Title = group.Title,
                    position = position,
                    NodeIDs = remappedIds
                });
            }

            return groups;
        }
    }
}
