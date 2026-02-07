using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;

using Shizounu.Library.Editor.DialogueEditor.Elements;
using Shizounu.Library.Editor.DialogueEditor.Windows;
using Shizounu.Library.Dialogue.Data;

namespace Shizounu.Library.Editor.DialogueEditor.Utilities
{
    /// <summary>
    /// Utility class for saving and loading dialogue graphs to/from DialogueData assets.
    /// </summary>
    public static class SavingUtility
    {
        #region Constants
        
        private const string DialogueFolderPath = "Assets/Prefabs/Dialogue/Dialogues";
        
        #endregion

        #region Saving
        
        /// <summary>
        /// Saves the dialogue graph to a DialogueData asset.
        /// </summary>
        /// <param name="dialogueName">The name for the dialogue file.</param>
        /// <param name="graphView">The graph view to save.</param>
        public static void Save(string dialogueName, DialogueGraphView graphView)
        {
            if (string.IsNullOrWhiteSpace(dialogueName))
            {
                Debug.LogError("[SavingUtility] Dialogue name cannot be empty.");
                return;
            }

            if (graphView == null)
            {
                Debug.LogError("[SavingUtility] Graph view cannot be null.");
                return;
            }

            // Ensure folder structure exists
            EnsureDialogueFolderExists(dialogueName);

            // Load or create DialogueData asset
            DialogueData data = LoadOrCreateDialogueData(dialogueName);
            data.Clear();

            // Save all nodes and connections
            SaveNodes(graphView, data);
            data.EntryElements = GetBranches(graphView.entryNode);

            // Save groups
            SaveGroups(graphView, data);

            // Save asset
            AssetDatabase.SaveAssets();
            Debug.Log($"[SavingUtility] Successfully saved dialogue: {dialogueName}");
        }

        /// <summary>
        /// Ensures the folder structure for the dialogue exists.
        /// </summary>
        private static void EnsureDialogueFolderExists(string dialogueName)
        {
            if (!AssetDatabase.IsValidFolder(DialogueFolderPath))
            {
                string parentFolder = System.IO.Path.GetDirectoryName(DialogueFolderPath);
                string folderName = System.IO.Path.GetFileName(DialogueFolderPath);
                AssetDatabase.CreateFolder(parentFolder, folderName);
            }

            string dialogueFolder = $"{DialogueFolderPath}/{dialogueName}";
            if (!AssetDatabase.IsValidFolder(dialogueFolder))
            {
                AssetDatabase.CreateFolder(DialogueFolderPath, dialogueName);
            }
        }

        /// <summary>
        /// Loads existing DialogueData or creates a new one.
        /// </summary>
        private static DialogueData LoadOrCreateDialogueData(string dialogueName)
        {
            string assetPath = GetDialogueDataPath(dialogueName);
            DialogueData data = AssetDatabase.LoadAssetAtPath<DialogueData>(assetPath);

            if (data == null)
            {
                data = ScriptableObject.CreateInstance<DialogueData>();
                AssetDatabase.CreateAsset(data, assetPath);
            }

            return data;
        }

        /// <summary>
        /// Gets the full asset path for a dialogue data file.
        /// </summary>
        private static string GetDialogueDataPath(string dialogueName)
        {
            return $"{DialogueFolderPath}/{dialogueName}/{dialogueName}.asset";
        }

        /// <summary>
        /// Saves all nodes from the graph view to the dialogue data.
        /// </summary>
        private static void SaveNodes(DialogueGraphView graphView, DialogueData data)
        {
            foreach (var nodeEntry in graphView.NodeCache)
            {
                DialogueElement element = nodeEntry.Value.GetElement();
                
                if (element == null)
                {
                    Debug.LogWarning($"[SavingUtility] Node {nodeEntry.Key} returned null element.");
                    continue;
                }

                element.Branches = GetBranches(nodeEntry.Value);
                data.Elements.Add(element);
            }
        }

        /// <summary>
        /// Saves all groups from the graph view to the dialogue data.
        /// </summary>
        private static void SaveGroups(DialogueGraphView graphView, DialogueData data)
        {
            foreach (var group in graphView.Groups)
            {
                GroupData groupData = new GroupData
                {
                    Title = group.title,
                    position = group.GetPosition(),
                    NodeIDs = new List<string>()
                };

                foreach (var element in group.containedElements)
                {
                    if (element is BaseNode node)
                    {
                        groupData.NodeIDs.Add(node.UID);
                    }
                }

                data.groupDatas.Add(groupData);
            }
        }

        /// <summary>
        /// Gets all branch connections from a node.
        /// </summary>
        private static List<PriorityIDTuple> GetBranches(BaseNode node)
        {
            List<PriorityIDTuple> branches = new List<PriorityIDTuple>();

            foreach (var priorityPort in node.BranchPorts)
            {
                foreach (var connection in priorityPort.port.connections)
                {
                    if (connection.input.node is BaseNode targetNode)
                    {
                        branches.Add(new PriorityIDTuple(priorityPort.priority, targetNode.UID));
                    }
                }
            }

            return branches;
        }
        
        #endregion

        #region Loading
        
        /// <summary>
        /// Loads a dialogue graph from DialogueData into the graph view.
        /// </summary>
        /// <param name="dialogueData">The dialogue data to load.</param>
        /// <param name="graphView">The graph view to load into.</param>
        public static void Load(DialogueData dialogueData, DialogueGraphView graphView)
        {
            if (dialogueData == null)
            {
                Debug.LogError("[SavingUtility] Dialogue data cannot be null.");
                return;
            }

            if (graphView == null)
            {
                Debug.LogError("[SavingUtility] Graph view cannot be null.");
                return;
            }

            // Clear existing nodes (except entry node)
            ClearGraphView(graphView);

            // Load all elements
            foreach (var element in dialogueData.Elements)
            {
                AddNode(element, graphView);
            }

            // Load all connections between nodes
            foreach (var element in dialogueData.Elements)
            {
                LoadConnections(element, graphView);
            }

            // Load entry connections
            foreach (var tuple in dialogueData.EntryElements)
            {
                if (graphView.NodeCache.TryGetValue(tuple.ID, out BaseNode targetNode))
                {
                    MakeConnection(graphView.entryNode, targetNode, tuple.Priority, graphView);
                }
            }

            // Load all groups
            LoadGroups(dialogueData, graphView);

            Debug.Log($"[SavingUtility] Successfully loaded dialogue: {dialogueData.name}");
        }

        /// <summary>
        /// Clears the graph view of all nodes except the entry node.
        /// </summary>
        private static void ClearGraphView(DialogueGraphView graphView)
        {
            // Collect nodes to remove (all except entry node)
            List<BaseNode> nodesToRemove = new List<BaseNode>();
            foreach (var nodeEntry in graphView.NodeCache)
            {
                if (nodeEntry.Value != graphView.entryNode)
                {
                    nodesToRemove.Add(nodeEntry.Value);
                }
            }

            // Remove nodes
            foreach (var node in nodesToRemove)
            {
                graphView.RemoveElement(node);
            }

            // Remove groups
            List<Group> groupsToRemove = new List<Group>(graphView.Groups);
            foreach (var group in groupsToRemove)
            {
                graphView.RemoveElement(group);
            }
        }

        /// <summary>
        /// Loads connections for a dialogue element.
        /// </summary>
        private static void LoadConnections(DialogueElement element, DialogueGraphView graphView)
        {
            if (!graphView.NodeCache.TryGetValue(element.ID, out BaseNode sourceNode))
            {
                Debug.LogWarning($"[SavingUtility] Source node not found: {element.ID}");
                return;
            }

            foreach (var tuple in element.Branches)
            {
                if (graphView.NodeCache.TryGetValue(tuple.ID, out BaseNode targetNode))
                {
                    MakeConnection(sourceNode, targetNode, tuple.Priority, graphView);
                }
                else
                {
                    Debug.LogWarning($"[SavingUtility] Target node not found: {tuple.ID}");
                }
            }
        }

        /// <summary>
        /// Loads groups from dialogue data.
        /// </summary>
        private static void LoadGroups(DialogueData dialogueData, DialogueGraphView graphView)
        {
            foreach (var groupData in dialogueData.groupDatas)
            {
                Group group = new Group
                {
                    title = groupData.Title
                };
                
                group.SetPosition(groupData.position);

                // Add nodes to group
                foreach (var nodeId in groupData.NodeIDs)
                {
                    if (graphView.NodeCache.TryGetValue(nodeId, out BaseNode node))
                    {
                        group.AddElement(node);
                    }
                }

                graphView.Groups.Add(group);
                graphView.Add(group);
            }
        }

        /// <summary>
        /// Adds a node to the graph view based on the dialogue element.
        /// </summary>
        private static BaseNode AddNode(DialogueElement element, DialogueGraphView graphView)
        {
            BaseNode node = element switch
            {
                Information info => CreateInformationNode(info, graphView),
                Conditional conditional => CreateConditionalNode(conditional, graphView),
                Sentence sentence => CreateSentenceNode(sentence, graphView),
                EventTrigger eventTrigger => CreateEventTriggerNode(eventTrigger, graphView),
                _ => null
            };

            if (node != null)
            {
                graphView.AddElement(node);
            }
            else
            {
                Debug.LogWarning($"[SavingUtility] Unknown element type: {element?.GetType().Name}");
            }

            return node;
        }

        /// <summary>
        /// Creates an information node from element data.
        /// </summary>
        private static InformationNode CreateInformationNode(Information info, DialogueGraphView graphView)
        {
            InformationNode node = (InformationNode)graphView.CreateNode(
                NodeType.Information, 
                info.NodePosition.position, 
                info);

            node.Blackboard = info.Blackboard;
            node.FactKey = info.FactKey;
            node.ConditionOperator = info.Operator;
            node.Value = info.Value;

            return node;
        }

        /// <summary>
        /// Creates a conditional node from element data.
        /// </summary>
        private static ConditionalNode CreateConditionalNode(Conditional conditional, DialogueGraphView graphView)
        {
            ConditionalNode node = (ConditionalNode)graphView.CreateNode(
                NodeType.Condition, 
                conditional.NodePosition.position, 
                conditional);

            node.Blackboard = conditional.Blackboard;
            node.FactKey = conditional.FactKey;
            node.ConditionOperator = conditional.Operator;
            node.Value = conditional.Value;

            return node;
        }

        /// <summary>
        /// Creates a sentence node from element data.
        /// </summary>
        private static SentenceNode CreateSentenceNode(Sentence sentence, DialogueGraphView graphView)
        {
            SentenceNode node = (SentenceNode)graphView.CreateNode(
                NodeType.SentenceNode, 
                sentence.NodePosition.position, 
                sentence);

            node.Speaker = sentence.Speaker;
            node.Text = sentence.Text;

            return node;
        }

        /// <summary>
        /// Creates an event trigger node from element data.
        /// </summary>
        private static EventTriggerNode CreateEventTriggerNode(EventTrigger eventTrigger, DialogueGraphView graphView)
        {
            EventTriggerNode node = (EventTriggerNode)graphView.CreateNode(
                NodeType.EventTrigger, 
                eventTrigger.NodePosition.position, 
                eventTrigger);

            node.scriptableEvent = eventTrigger.scriptableEvent;

            return node;
        }

        /// <summary>
        /// Creates a connection between two nodes.
        /// </summary>
        private static void MakeConnection(BaseNode fromNode, BaseNode toNode, int priority, DialogueGraphView graphView)
        {
            PriorityPort priorityPort = fromNode.GetPortWithPriority(priority);
            Edge edge = priorityPort.port.ConnectTo(toNode.inputPort);
            graphView.AddElement(edge);
        }
        
        #endregion
    }
}
