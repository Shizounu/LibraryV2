using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using Shizounu.Library.ScriptableArchitecture;

namespace Shizounu.Library.Editor.DialogueEditor
{
    /// <summary>
    /// Custom property drawer for IntReference that properly displays 
    /// the useConstant toggle and either the constant value or variable reference.
    /// </summary>
    [CustomPropertyDrawer(typeof(IntReference))]
    public class IntReferencePropertyDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            // Create container
            var container = new VisualElement();
            container.style.flexDirection = FlexDirection.Row;

            // Get properties
            SerializedProperty useConstantProp = property.FindPropertyRelative("useConstant");
            SerializedProperty constantValueProp = property.FindPropertyRelative("ConstantValue");
            SerializedProperty variableProp = property.FindPropertyRelative("Variable");

            // Create toggle for useConstant
            var toggle = new Toggle
            {
                value = useConstantProp.boolValue,
                tooltip = "Use Constant Value"
            };
            toggle.style.width = 20;
            toggle.style.marginRight = 4;

            // Create the value field (either int field or object field)
            var valueContainer = new VisualElement();
            valueContainer.style.flexGrow = 1;

            void UpdateValueField(bool useConstant)
            {
                valueContainer.Clear();

                if (useConstant)
                {
                    var intField = new PropertyField(constantValueProp, "");
                    intField.style.flexGrow = 1;
                    valueContainer.Add(intField);
                }
                else
                {
                    var objectField = new PropertyField(variableProp, "");
                    objectField.style.flexGrow = 1;
                    valueContainer.Add(objectField);
                }
            }

            // Initial setup
            UpdateValueField(useConstantProp.boolValue);

            // Handle toggle changes
            toggle.RegisterValueChangedCallback(evt =>
            {
                useConstantProp.boolValue = evt.newValue;
                useConstantProp.serializedObject.ApplyModifiedProperties();
                UpdateValueField(evt.newValue);
            });

            container.Add(toggle);
            container.Add(valueContainer);

            return container;
        }

        // Fallback for IMGUI
        public override void OnGUI(UnityEngine.Rect position, SerializedProperty property, UnityEngine.GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // Get properties
            SerializedProperty useConstant = property.FindPropertyRelative("useConstant");
            SerializedProperty constantValue = property.FindPropertyRelative("ConstantValue");
            SerializedProperty variable = property.FindPropertyRelative("Variable");

            // Draw label
            position = EditorGUI.PrefixLabel(position, label);

            // Calculate rects
            float toggleWidth = 15;
            UnityEngine.Rect toggleRect = new UnityEngine.Rect(position.x, position.y, toggleWidth, position.height);
            UnityEngine.Rect valueRect = new UnityEngine.Rect(position.x + toggleWidth + 5, position.y, 
                position.width - toggleWidth - 5, position.height);

            // Draw toggle
            useConstant.boolValue = EditorGUI.Toggle(toggleRect, useConstant.boolValue);

            // Draw value field
            if (useConstant.boolValue)
            {
                EditorGUI.PropertyField(valueRect, constantValue, UnityEngine.GUIContent.none);
            }
            else
            {
                EditorGUI.PropertyField(valueRect, variable, UnityEngine.GUIContent.none);
            }

            EditorGUI.EndProperty();
        }
    }
}
