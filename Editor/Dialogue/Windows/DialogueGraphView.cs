using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using System.Collections.Generic;
using System.Linq;

using Shizounu.Library.Dialogue.Data;
using Shizounu.Library.Editor.DialogueEditor.Elements;
using Shizounu.Library.Editor.DialogueEditor.Utilities;

namespace Shizounu.Library.Editor.DialogueEditor.Windows
{
    /// <summary>
    /// The main graph view for editing dialogue node graphs.
    /// Handles node creation, connections, and graph interactions.
    /// </summary>
    public class DialogueGraphView : GraphView
    {
        #region Constants
        
        private const string DefaultGroupTitle = "Dialogue Group";
        private const string ContextMenuAddSlide = "Add Slide";
        private const string ContextMenuAddGroup = "Add Group";
        
        #endregion

        #region Fields
        
        private readonly DialogueEditorWindow editorWindow;
        private GraphSearchWindow searchWindow;
        private DialogueGraphUndo undoService;
        private string runtimeActiveNodeId;
        private bool suppressGraphEvents;
        
        public EntryNode entryNode;
        public Dictionary<string, BaseNode> NodeCache { get; private set; }
        public List<Group> Groups { get; private set; }
        
        #endregion

        #region Initialization
        
        public DialogueGraphView(DialogueEditorWindow editorWindow)
        {
            this.editorWindow = editorWindow;
            graphViewChanged += OnGraphViewChanged;
            Initialize();
        }

        /// <summary>
        /// Initializes the graph view with default setup.
        /// </summary>
        private void Initialize()
        {
            NodeCache = new Dictionary<string, BaseNode>();
            Groups = new List<Group>();

            AddManipulators();
            AddGridBackground();
            ApplyStyles();
            AddSearchWindow();
            AddStartNode();

            undoService = new DialogueGraphUndo(this);
            undoService.Initialize();
        }

        /// <summary>
        /// Creates and adds the entry node to the graph.
        /// </summary>
        private void AddStartNode()
        {
            entryNode = (EntryNode)CreateNode(NodeType.StartNode, new Vector2(100, 300));
            AddElement(entryNode);
        }
        
        #endregion

        #region Search Window
        
        /// <summary>
        /// Sets up the search window for node creation.
        /// </summary>
        private void AddSearchWindow()
        {
            if (searchWindow == null)
            {
                searchWindow = ScriptableObject.CreateInstance<GraphSearchWindow>();
            }
            
            searchWindow.Initialize(this);
            nodeCreationRequest = context => 
                SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), searchWindow);
        }
        
        #endregion

        #region Manipulators and Context Menus
        
        /// <summary>
        /// Adds all graph manipulators (dragging, zooming, selection, context menus).
        /// </summary>
        private void AddManipulators()
        {
            // Add standard manipulators
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            // Setup zoom
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

            // Add custom context menus
            this.AddManipulator(CreateNodeContextMenu(ContextMenuAddSlide, NodeType.SentenceNode));
            this.AddManipulator(CreateGroupContextMenu());
        }

        /// <summary>
        /// Creates a context menu manipulator for adding groups.
        /// </summary>
        private IManipulator CreateGroupContextMenu()
        {
            return new ContextualMenuManipulator(menuEvent =>
                menuEvent.menu.AppendAction(
                    ContextMenuAddGroup,
                    actionEvent => AddElement(CreateGroup(
                        DefaultGroupTitle,
                        GetLocalMousePosition(actionEvent.eventInfo.localMousePosition)))));
        }

        /// <summary>
        /// Creates a context menu manipulator for adding nodes.
        /// </summary>
        private IManipulator CreateNodeContextMenu(string actionTitle, NodeType nodeType)
        {
            return new ContextualMenuManipulator(menuEvent =>
                menuEvent.menu.AppendAction(
                    actionTitle,
                    actionEvent => AddElement(CreateNode(
                        nodeType,
                        GetLocalMousePosition(actionEvent.eventInfo.localMousePosition)))));
        }
        
        #endregion

        #region Element Creation
        
        /// <summary>
        /// Creates a new group at the specified position.
        /// </summary>
        /// <param name="title">The group title.</param>
        /// <param name="position">The local position for the group.</param>
        /// <returns>The created Group element.</returns>
        public GraphElement CreateGroup(string title, Vector2 position, bool recordUndo = true)
        {
            Group group = new Group
            {
                title = title
            };
            
            group.SetPosition(new Rect(position, Vector2.zero));

            // Add selected nodes to the group
            foreach (GraphElement selectedElement in selection)
            {
                if (selectedElement is BaseNode node)
                {
                    group.AddElement(node);
                }
            }

            Groups.Add(group);

            if (recordUndo)
                RecordUndo("Create Group");

            return group;
        }

        /// <summary>
        /// Creates a new node of the specified type at the given position.
        /// </summary>
        /// <param name="type">The type of node to create.</param>
        /// <param name="position">The position for the node.</param>
        /// <param name="elementToLoad">Optional dialogue element to load data from.</param>
        /// <returns>The created node.</returns>
        public BaseNode CreateNode(NodeType type, Vector2 position, DialogueElement elementToLoad = null, bool recordUndo = true)
        {
            BaseNode node = InstantiateNode(type);
            
            if (node == null)
            {
                Debug.LogError($"[DialogueGraphView] Failed to create node of type: {type}");
                return null;
            }

            node.Initialize(position, this);

            if (elementToLoad != null)
            {
                node.LoadData(elementToLoad);
                node.UID = elementToLoad.ID;
            }
            else
            {
                node.UID = DialogueData.GetID();
            }

            node.Draw();
            NodeCache.Add(node.UID, node);

            if (recordUndo)
                RecordUndo("Create Node");

            return node;
        }

        /// <summary>
        /// Instantiates a node instance based on the node type.
        /// </summary>
        private BaseNode InstantiateNode(NodeType type)
        {
            return type switch
            {
                NodeType.StartNode => new EntryNode(),
                NodeType.SentenceNode => new SentenceNode(),
                NodeType.ChoiceNode => new ChoiceNode(),
                NodeType.Condition => new ConditionalNode(),
                NodeType.Information => new InformationNode(),
                NodeType.EventTrigger => new EventTriggerNode(),
                _ => null
            };
        }
        
        #endregion

        #region Visual Setup
        
        /// <summary>
        /// Adds the grid background to the graph view.
        /// </summary>
        private void AddGridBackground()
        {
            GridBackground grid = new GridBackground();
            grid.StretchToParentSize();
            Insert(0, grid);
        }

        /// <summary>
        /// Applies stylesheets to the graph view.
        /// </summary>
        private void ApplyStyles()
        {
            this.AddStyleSheets("ViewStyles.uss", "NodeStyle.uss");
        }
        
        #endregion

        #region Port Compatibility
        
        /// <summary>
        /// Determines which ports can be connected to the given start port.
        /// </summary>
        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            List<Port> compatiblePorts = new List<Port>();

            ports.ForEach(port =>
            {
                if (port.direction != startPort.direction)
                {
                    compatiblePorts.Add(port);
                }
            });

            return compatiblePorts;
        }
        
        #endregion

        #region Utility Methods
        
        /// <summary>
        /// Converts screen mouse position to local graph position.
        /// </summary>
        /// <param name="mousePosition">The screen mouse position.</param>
        /// <param name="isSearchWindow">Whether the position is from the search window.</param>
        /// <returns>The local position in the graph.</returns>
        public Vector2 GetLocalMousePosition(Vector2 mousePosition, bool isSearchWindow = false)
        {
            Vector2 worldMousePosition = mousePosition;
            
            if (isSearchWindow)
            {
                worldMousePosition -= editorWindow.position.position;
            }

            return contentViewContainer.WorldToLocal(worldMousePosition);
        }

        public void BeginGraphUpdate()
        {
            suppressGraphEvents = true;
            undoService?.BeginRestore();
        }

        public void EndGraphUpdate()
        {
            undoService?.EndRestore();
            suppressGraphEvents = false;

            foreach (var node in NodeCache.Values)
            {
                node.RefreshBranchPreviews();
            }
        }

        public void RecordUndo(string actionName)
        {
            if (suppressGraphEvents)
                return;

            undoService?.RecordSnapshot(actionName);
        }

        public void ApplySearchFilter(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                ClearSearchFilter();
                return;
            }

            string lowered = query.ToLowerInvariant();
            foreach (var node in NodeCache.Values)
            {
                string searchText = node.GetSearchText().ToLowerInvariant();
                bool match = searchText.Contains(lowered);

                if (match)
                    node.RemoveFromClassList("ds-node--hidden");
                else
                    node.AddToClassList("ds-node--hidden");
            }
        }

        public void ClearSearchFilter()
        {
            foreach (var node in NodeCache.Values)
            {
                node.RemoveFromClassList("ds-node--hidden");
            }
        }

        public void AutoLayout()
        {
            const float xSpacing = 320f;
            const float ySpacing = 220f;

            Dictionary<BaseNode, int> depths = new Dictionary<BaseNode, int>();
            Queue<BaseNode> queue = new Queue<BaseNode>();

            if (entryNode != null)
            {
                depths[entryNode] = 0;
                queue.Enqueue(entryNode);
            }

            while (queue.Count > 0)
            {
                BaseNode current = queue.Dequeue();
                int depth = depths[current];

                foreach (var next in GetOutgoingNodes(current))
                {
                    if (!depths.ContainsKey(next))
                    {
                        depths[next] = depth + 1;
                        queue.Enqueue(next);
                    }
                }
            }

            foreach (var node in NodeCache.Values)
            {
                if (!depths.ContainsKey(node))
                    depths[node] = 0;
            }

            var levels = depths.GroupBy(pair => pair.Value)
                .OrderBy(group => group.Key)
                .ToDictionary(group => group.Key, group => group.Select(pair => pair.Key).ToList());

            foreach (var level in levels)
            {
                for (int i = 0; i < level.Value.Count; i++)
                {
                    BaseNode node = level.Value[i];
                    Rect rect = node.GetPosition();
                    rect.position = new Vector2(level.Key * xSpacing, i * ySpacing);
                    node.SetPosition(rect);
                }
            }

            RecordUndo("Auto Layout");
        }

        public void SetRuntimeActiveNode(string nodeId)
        {
            if (!string.IsNullOrWhiteSpace(runtimeActiveNodeId) && NodeCache.TryGetValue(runtimeActiveNodeId, out BaseNode oldNode))
                oldNode.RemoveFromClassList("ds-node--active");

            runtimeActiveNodeId = nodeId;

            if (!string.IsNullOrWhiteSpace(runtimeActiveNodeId) && NodeCache.TryGetValue(runtimeActiveNodeId, out BaseNode newNode))
                newNode.AddToClassList("ds-node--active");
        }

        private IEnumerable<BaseNode> GetOutgoingNodes(BaseNode node)
        {
            foreach (var priorityPort in node.BranchPorts)
            {
                foreach (var edge in priorityPort.port.connections)
                {
                    if (edge.input?.node is BaseNode targetNode)
                        yield return targetNode;
                }
            }
        }

        public void Dispose()
        {
            undoService?.Dispose();
        }
        
        #endregion

        #region Graph Change Handling
        
        /// <summary>
        /// Handles changes to the graph, such as element removal.
        /// </summary>
        private GraphViewChange OnGraphViewChanged(GraphViewChange change)
        {
            if (suppressGraphEvents)
                return change;

            if (change.elementsToRemove != null)
            {
                foreach (var element in change.elementsToRemove)
                {
                    if (element is BaseNode node)
                    {
                        NodeCache.Remove(node.UID);
                    }
                    else if (element is Group group)
                    {
                        Groups.Remove(group);
                    }
                }
            }

            foreach (var node in NodeCache.Values)
            {
                node.RefreshBranchPreviews();
            }

            if (!undoService.IsRestoring)
                RecordUndo("Graph Change");

            return change;
        }
        
        #endregion
    }
}
