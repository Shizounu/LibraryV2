using System;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;

using Shizounu.Library.Editor.DialogueEditor.Elements;
using Shizounu.Library.ScriptableArchitecture;

namespace Shizounu.Library.Editor.DialogueEditor.Utilities
{
    /// <summary>
    /// Utility class for creating UI Toolkit elements with predefined configurations.
    /// </summary>
    public static class ElementUtility
    {
        #region Button Creation
        
        /// <summary>
        /// Creates a button with specified text and optional click callback.
        /// </summary>
        /// <param name="text">The button text.</param>
        /// <param name="onClick">Optional callback when the button is clicked.</param>
        /// <returns>The created Button element.</returns>
        public static Button CreateButton(string text, Action onClick = null)
        {
            Button button = new Button(onClick)
            {
                text = text
            };

            return button;
        }
        
        #endregion

        #region Container Creation
        
        /// <summary>
        /// Creates a foldout container with specified title and collapse state.
        /// </summary>
        /// <param name="title">The foldout title.</param>
        /// <param name="collapsed">Whether the foldout starts collapsed.</param>
        /// <returns>The created Foldout element.</returns>
        public static Foldout CreateFoldout(string title, bool collapsed = false)
        {
            Foldout foldout = new Foldout()
            {
                text = title,
                value = !collapsed
            };

            return foldout;
        }
        
        #endregion

        #region Port Creation
        
        /// <summary>
        /// Creates a port for connecting nodes in the graph view.
        /// </summary>
        /// <param name="node">The node to create the port on.</param>
        /// <param name="portName">The display name of the port.</param>
        /// <param name="orientation">The orientation of the port.</param>
        /// <param name="direction">The direction (input/output) of the port.</param>
        /// <param name="capacity">The capacity (single/multi) of the port.</param>
        /// <returns>The created Port element.</returns>
        public static Port CreatePort(
            this BaseNode node, 
            string portName = "", 
            Orientation orientation = Orientation.Horizontal, 
            Direction direction = Direction.Output, 
            Port.Capacity capacity = Port.Capacity.Single)
        {
            Port port = node.InstantiatePort(orientation, direction, capacity, typeof(bool));
            port.portName = portName;
            
            if (direction == Direction.Input)
            {
                node.inputPort = port;
            }
            
            return port;
        }
        
        #endregion

        #region Text Field Creation
        
        /// <summary>
        /// Creates a single-line text field.
        /// </summary>
        /// <param name="value">The initial value.</param>
        /// <param name="label">The field label.</param>
        /// <param name="onValueChanged">Optional callback when the value changes.</param>
        /// <returns>The created TextField element.</returns>
        public static TextField CreateTextField(
            string value = null, 
            string label = null, 
            EventCallback<ChangeEvent<string>> onValueChanged = null)
        {
            TextField textField = new TextField()
            {
                value = value ?? string.Empty,
                label = label
            };

            if (onValueChanged != null)
            {
                textField.RegisterValueChangedCallback(onValueChanged);
            }

            return textField;
        }

        /// <summary>
        /// Creates a multi-line text area field.
        /// </summary>
        /// <param name="value">The initial value.</param>
        /// <param name="label">The field label.</param>
        /// <param name="onValueChanged">Optional callback when the value changes.</param>
        /// <returns>The created TextField element configured as text area.</returns>
        public static TextField CreateTextArea(
            string value = null, 
            string label = null, 
            EventCallback<ChangeEvent<string>> onValueChanged = null)
        {
            TextField textArea = CreateTextField(value, label, onValueChanged);
            textArea.multiline = true;

            return textArea;
        }
        
        #endregion

        #region Object Field Creation
        
        /// <summary>
        /// Creates a ScriptableObject field for selecting assets.
        /// </summary>
        /// <typeparam name="T">The ScriptableObject type.</typeparam>
        /// <param name="label">The field label.</param>
        /// <param name="initialValue">The initial value.</param>
        /// <param name="onValueChanged">Optional callback when the value changes.</param>
        /// <returns>The created ObjectField element.</returns>
        public static ObjectField CreateSOField<T>(
            string label, 
            T initialValue = null, 
            EventCallback<ChangeEvent<UnityEngine.Object>> onValueChanged = null) 
            where T : ScriptableObject
        {
            ObjectField objectField = new ObjectField()
            {
                label = label,
                objectType = typeof(T),
                allowSceneObjects = false,
                value = initialValue
            };
            
            if (onValueChanged != null)
            {
                objectField.RegisterValueChangedCallback(onValueChanged);
            }

            return objectField;
        }
        
        #endregion

        #region Numeric Field Creation
        
        /// <summary>
        /// Creates an integer field.
        /// </summary>
        /// <param name="value">The initial value.</param>
        /// <param name="label">The field label.</param>
        /// <param name="onValueChanged">Optional callback when the value changes.</param>
        /// <returns>The created IntegerField element.</returns>
        public static IntegerField CreateIntField(
            int value = 0, 
            string label = null, 
            EventCallback<ChangeEvent<int>> onValueChanged = null)
        {
            IntegerField intField = new IntegerField()
            {
                label = label,
                value = value,
            };

            if (onValueChanged != null)
            {
                intField.RegisterValueChangedCallback(onValueChanged);
            }

            return intField;
        }
        
        #endregion

        #region Enum Field Creation
        
        /// <summary>
        /// Creates an enum dropdown field.
        /// </summary>
        /// <typeparam name="T">The enum type.</typeparam>
        /// <param name="value">The initial enum value.</param>
        /// <param name="label">The field label.</param>
        /// <param name="onValueChanged">Optional callback when the value changes.</param>
        /// <returns>The created EnumField element.</returns>
        public static EnumField CreateEnumField<T>(
            T value, 
            string label = null, 
            EventCallback<ChangeEvent<Enum>> onValueChanged = null) 
            where T : Enum
        {
            EnumField enumField = new EnumField()
            {
                label = label,
                value = value,
            };
            
            enumField.Init(value, true);

            if (onValueChanged != null)
            {
                enumField.RegisterValueChangedCallback(onValueChanged);
            }

            return enumField;
        }
        
        #endregion

        #region Custom Property Field Creation
        
        /// <summary>
        /// Creates a visual element for editing an IntReference.
        /// This creates a custom UI that allows choosing between a constant value and a variable reference.
        /// </summary>
        /// <param name="intReference">The IntReference to edit.</param>
        /// <param name="label">The field label.</param>
        /// <param name="onValueChanged">Optional callback when the value changes.</param>
        /// <returns>A container with the IntReference editing controls.</returns>
        public static VisualElement CreateIntReferenceField(
            IntReference intReference,
            string label,
            Action onValueChanged = null)
        {
            if (intReference == null)
            {
                Debug.LogError("[ElementUtility] IntReference cannot be null for CreateIntReferenceField.");
                return new Label("Error: IntReference is null");
            }

            // Create main container
            var container = new VisualElement();
            container.style.flexDirection = FlexDirection.Column;
            container.style.marginTop = 2;
            container.style.marginBottom = 2;

            // Create label
            if (!string.IsNullOrWhiteSpace(label))
            {
                var labelElement = new Label(label);
                labelElement.style.marginBottom = 2;
                container.Add(labelElement);
            }

            // Create control row
            var controlRow = new VisualElement();
            controlRow.style.flexDirection = FlexDirection.Row;

            // Create toggle for useConstant
            var toggle = new Toggle("Use Constant")
            {
                value = intReference.useConstant
            };
            toggle.style.width = 100;
            toggle.style.marginRight = 4;

            // Create value field container
            var valueContainer = new VisualElement();
            valueContainer.style.flexGrow = 1;

            void UpdateValueField()
            {
                valueContainer.Clear();

                if (intReference.useConstant)
                {
                    var intField = new IntegerField()
                    {
                        value = intReference.ConstantValue
                    };
                    intField.RegisterValueChangedCallback(evt =>
                    {
                        intReference.ConstantValue = evt.newValue;
                        onValueChanged?.Invoke();
                    });
                    valueContainer.Add(intField);
                }
                else
                {
                    var objectField = new ObjectField()
                    {
                        objectType = typeof(ScriptableInt),
                        allowSceneObjects = false,
                        value = intReference.Variable as ScriptableInt
                    };
                    objectField.RegisterValueChangedCallback(evt =>
                    {
                        intReference.Variable = evt.newValue as ScriptableVariable<int>;
                        onValueChanged?.Invoke();
                    });
                    valueContainer.Add(objectField);
                }
            }

            // Initial setup
            UpdateValueField();

            // Handle toggle changes
            toggle.RegisterValueChangedCallback(evt =>
            {
                intReference.useConstant = evt.newValue;
                UpdateValueField();
                onValueChanged?.Invoke();
            });

            controlRow.Add(toggle);
            controlRow.Add(valueContainer);
            container.Add(controlRow);

            return container;
        }
        
        #endregion
    }
}
