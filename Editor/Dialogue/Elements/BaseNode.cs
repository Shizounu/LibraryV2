using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;

using Shizounu.Library.Dialogue.Data;
using Shizounu.Library.Editor.DialogueEditor.Windows;
using Shizounu.Library.Editor.DialogueEditor.Utilities;

namespace Shizounu.Library.Editor.DialogueEditor.Elements 
{
    /// <summary>
    /// Base class for all dialogue nodes in the graph view.
    /// Provides common functionality for node creation, visualization, and data management.
    /// </summary>
    public abstract class BaseNode : Node
    {
        #region Fields
        
        public string UID;
        public string SlideName;
        public List<PriorityPort> BranchPorts = new List<PriorityPort>();
        public Port inputPort;
        
        protected DialogueGraphView graphView;
        
        #endregion

        #region Initialization
        
        /// <summary>
        /// Initializes the node with position and graph reference.
        /// </summary>
        /// <param name="position">The position in the graph view.</param>
        /// <param name="graphView">Reference to the containing graph view.</param>
        public virtual void Initialize(Vector2 position, DialogueGraphView graphView)
        {
            SlideName = "SlideName";
            BranchPorts = new List<PriorityPort>();
            SetPosition(new Rect(position, Vector2.zero));

            CreatePriorityPort();

            mainContainer.AddToClassList("ds-node__main-container");
            extensionContainer.AddToClassList("ds-node__extension-container");

            this.graphView = graphView;
        }
        
        #endregion

        #region Drawing
        
        /// <summary>
        /// Draws the complete node UI including all sections.
        /// </summary>
        public virtual void Draw()
        {
            DrawTitle();
            DrawMainContent();
            DrawInputPort();
            DrawOutputPorts();
            DrawExtensionContent();

            RefreshExpandedState();
        }

        /// <summary>
        /// Draws the title section of the node.
        /// </summary>
        protected virtual void DrawTitle()
        {
            TextField slideNameTextField = ElementUtility.CreateTextField(SlideName);
            titleContainer.Insert(0, slideNameTextField);

            slideNameTextField.AddClasses(
                "ds-node__text-field",
                "ds-node__text-field__hidden",
                "ds-node__filename-text-field");
        }

        /// <summary>
        /// Draws the input port section.
        /// </summary>
        protected virtual void DrawInputPort()
        {
            Port port = this.CreatePort(
                "Incoming", 
                Orientation.Horizontal, 
                Direction.Input, 
                Port.Capacity.Multi);
            
            inputContainer.Add(port);
        }

        /// <summary>
        /// Draws the main content section. Override in derived classes.
        /// </summary>
        protected virtual void DrawMainContent() { }

        /// <summary>
        /// Draws the output ports section. Override in derived classes.
        /// </summary>
        protected virtual void DrawOutputPorts() { }

        /// <summary>
        /// Draws the extension content section. Override in derived classes.
        /// </summary>
        protected virtual void DrawExtensionContent() { }
        
        #endregion

        #region Priority Ports
        
        /// <summary>
        /// Gets or creates a port with the specified priority.
        /// </summary>
        /// <param name="priority">The priority value for the port.</param>
        /// <returns>The priority port with the specified priority.</returns>
        public PriorityPort GetPortWithPriority(int priority)
        {
            PriorityPort existingPort = BranchPorts.FirstOrDefault(port => port.priority == priority);
            
            if (existingPort != null)
            {
                return existingPort;
            }

            return CreatePriorityPort(priority);
        }

        /// <summary>
        /// Creates a new priority port with optional base priority.
        /// </summary>
        /// <param name="basePriority">The base priority value for the port.</param>
        /// <returns>The created priority port.</returns>
        protected PriorityPort CreatePriorityPort(int basePriority = 0)
        {
            Port port = this.CreatePort(
                "", 
                Orientation.Horizontal, 
                Direction.Output, 
                Port.Capacity.Multi);

            PriorityPort priorityPort = new PriorityPort(basePriority, port);

            // Create delete button
            Button deleteButton = ElementUtility.CreateButton("X", () => HandlePortDelete(priorityPort, port));
            deleteButton.AddToClassList("ds-node__button");

            // Create priority field
            IntegerField priorityField = ElementUtility.CreateIntField(
                priorityPort.priority, 
                null, 
                evt => priorityPort.priority = evt.newValue);

            priorityField.AddClasses(
                "ds-node__text-field",
                "ds-node__text-field__hidden",
                "ds-node__choice-text-field");

            port.Add(priorityField);
            port.Add(deleteButton);

            BranchPorts.Add(priorityPort);
            outputContainer.Add(port);

            return priorityPort;
        }

        /// <summary>
        /// Handles the deletion of a priority port.
        /// </summary>
        private void HandlePortDelete(PriorityPort priorityPort, Port port)
        {
            if (port.connected)
            {
                graphView.DeleteElements(port.connections);
            }

            BranchPorts.Remove(priorityPort);
            graphView.RemoveElement(port);
        }
        
        #endregion

        #region Abstract Methods
        
        /// <summary>
        /// Gets the dialogue element data from this node.
        /// </summary>
        /// <returns>The dialogue element containing this node's data.</returns>
        public abstract DialogueElement GetElement();

        /// <summary>
        /// Loads data from a dialogue element into this node.
        /// </summary>
        /// <param name="element">The dialogue element to load data from.</param>
        public abstract void LoadData(DialogueElement element);
        
        #endregion

        #region Deprecated Method Names
        
        // These methods are kept for backwards compatibility but redirect to new names
        protected virtual void MakeTitle() => DrawTitle();
        protected virtual void MakeInput() => DrawInputPort();
        protected virtual void MakeMain() => DrawMainContent();
        protected virtual void MakeOutput() => DrawOutputPorts();
        protected virtual void MakeExtension() => DrawExtensionContent();
        
        #endregion
    }

    /// <summary>
    /// Represents a port with an associated priority value for dialogue branching.
    /// </summary>
    public class PriorityPort
    {
        public int priority;
        public Port port;

        public PriorityPort(int priority, Port port)
        {
            this.priority = priority;
            this.port = port;
        }
    }
}
