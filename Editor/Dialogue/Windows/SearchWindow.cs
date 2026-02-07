using UnityEngine;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using Shizounu.Library.Editor.DialogueEditor.Elements;

namespace Shizounu.Library.Editor.DialogueEditor.Windows
{
    /// <summary>
    /// Search window provider for creating dialogue nodes and groups in the graph view.
    /// </summary>
    public class GraphSearchWindow : ScriptableObject, ISearchWindowProvider
    {
        #region Constants
        
        private const string DefaultGroupTitle = "Dialogue Group";
        private const string CreateElementTitle = "Create Element";
        private const string DialogueNodeCategory = "Dialogue Node";
        private const string DialogueGroupCategory = "Dialogue Group";
        
        #endregion

        #region Fields
        
        private DialogueGraphView graphView;
        private Texture2D indentationIcon;
        
        #endregion

        #region Initialization
        
        /// <summary>
        /// Initializes the search window with the graph view reference.
        /// </summary>
        /// <param name="graphView">The graph view to create elements in.</param>
        public void Initialize(DialogueGraphView graphView)
        {
            this.graphView = graphView;
            indentationIcon = CreateIndentationIcon();
        }

        /// <summary>
        /// Creates a transparent icon for tree indentation.
        /// </summary>
        private static Texture2D CreateIndentationIcon()
        {
            Texture2D icon = new Texture2D(1, 1);
            icon.SetPixel(0, 0, Color.clear);
            icon.Apply();
            return icon;
        }
        
        #endregion

        #region ISearchWindowProvider Implementation
        
        /// <summary>
        /// Creates the search tree with all available node and group types.
        /// </summary>
        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            List<SearchTreeEntry> entries = new List<SearchTreeEntry>
            {
                new SearchTreeGroupEntry(new GUIContent(CreateElementTitle)),
                new SearchTreeGroupEntry(new GUIContent(DialogueNodeCategory), 1),
                CreateNodeEntry("Sentence Node", NodeType.SentenceNode),
                CreateNodeEntry("Condition Node", NodeType.Condition),
                CreateNodeEntry("Information Node", NodeType.Information),
                CreateNodeEntry("Event Trigger Node", NodeType.EventTrigger),
                new SearchTreeGroupEntry(new GUIContent(DialogueGroupCategory), 1),
                CreateGroupEntry("Single Group")
            };

            return entries;
        }

        /// <summary>
        /// Creates a search tree entry for a node type.
        /// </summary>
        private SearchTreeEntry CreateNodeEntry(string title, NodeType nodeType)
        {
            return new SearchTreeEntry(new GUIContent(title, indentationIcon))
            {
                level = 2,
                userData = nodeType
            };
        }

        /// <summary>
        /// Creates a search tree entry for a group.
        /// </summary>
        private SearchTreeEntry CreateGroupEntry(string title)
        {
            return new SearchTreeEntry(new GUIContent(title, indentationIcon))
            {
                level = 2,
                userData = new Group()
            };
        }

        /// <summary>
        /// Handles the selection of an entry from the search window.
        /// </summary>
        public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
        {
            Vector2 localPosition = graphView.GetLocalMousePosition(context.screenMousePosition, true);

            switch (searchTreeEntry.userData)
            {
                case NodeType nodeType:
                    return CreateAndAddNode(nodeType, localPosition);

                case Group _:
                    return CreateAndAddGroup(localPosition);

                default:
                    Debug.LogWarning($"[GraphSearchWindow] Unknown search entry type: {searchTreeEntry.userData?.GetType().Name}");
                    return false;
            }
        }
        
        #endregion

        #region Element Creation
        
        /// <summary>
        /// Creates and adds a node to the graph view.
        /// </summary>
        private bool CreateAndAddNode(NodeType nodeType, Vector2 position)
        {
            BaseNode node = graphView.CreateNode(nodeType, position);
            
            if (node == null)
            {
                Debug.LogError($"[GraphSearchWindow] Failed to create node of type: {nodeType}");
                return false;
            }

            graphView.AddElement(node);
            return true;
        }

        /// <summary>
        /// Creates and adds a group to the graph view.
        /// </summary>
        private bool CreateAndAddGroup(Vector2 position)
        {
            Group group = (Group)graphView.CreateGroup(DefaultGroupTitle, position);
            
            if (group == null)
            {
                Debug.LogError("[GraphSearchWindow] Failed to create group.");
                return false;
            }

            graphView.AddElement(group);
            return true;
        }
        
        #endregion
    }
}

