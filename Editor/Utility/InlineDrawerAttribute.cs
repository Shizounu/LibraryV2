using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Shizounu.Library.Editor
{
    /// <summary>
    /// Attribute to draw a reference field and its contents inline in the inspector.
    /// </summary>
    public class InlineDrawerAttribute : PropertyAttribute
    {
        
    }

    [CustomPropertyDrawer(typeof(InlineDrawerAttribute))]
    public class InlineDrawerAttributeDrawer : PropertyDrawer
    {
        private const float SPACE_BETWEEN_ELEMENTS = 3f;
        private const float SPACE_AFTER_ELEMENT = 10f;
        //private bool _isFolded = true;

        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {

            int count = CountChildren(property);
            rect.height -= (count != 1 ? SPACE_AFTER_ELEMENT : 0);
            rect.height /= count;
            EditorGUI.PropertyField(rect, property, label); //draw the field for the reference

            //EditorGUI.Foldout(rect, _isFolded, "contents");
            if (count == 1 || property.objectReferenceValue == null) return;

            SerializedObject serializedObject = new(property.objectReferenceValue);
            SerializedProperty internalProperty = serializedObject.GetIterator();
            serializedObject.Update();

            if (!internalProperty.NextVisible(true)) return;
            while (internalProperty.NextVisible(false))
            {
                EditorGUI.indentLevel = 1;
                rect.y += rect.height + SPACE_BETWEEN_ELEMENTS;
                EditorGUI.PropertyField(rect, internalProperty, internalProperty.hasChildren);
                if (internalProperty.isArray)
                {
                    rect.y = EditorGUI.GetPropertyHeight(internalProperty, true) - rect.height;
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        private int CountChildren(SerializedProperty prop)
        {
            int c = 1;

            if (prop.propertyType != SerializedPropertyType.ObjectReference) return c;
            if (prop.objectReferenceValue == null) return c;

            SerializedObject serializedObject = new(prop.objectReferenceValue);
            SerializedProperty internalProperty = serializedObject.GetIterator();

            if (!internalProperty.hasVisibleChildren) return c;
            if (!internalProperty.NextVisible(true)) return c;
            while (internalProperty.NextVisible(false))
            {
                c++;
                if (internalProperty.isArray && internalProperty.isExpanded)
                {
                    c += internalProperty.arraySize + 1;
                }
            }
            return c;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = EditorGUI.GetPropertyHeight(property);
            if (property.objectReferenceValue == null) return EditorGUIUtility.singleLineHeight;
            SerializedProperty internalProperty = new SerializedObject(property.objectReferenceValue).GetIterator();

            if (!internalProperty.hasVisibleChildren) return height;
            if (!internalProperty.NextVisible(true)) return height;
            while (internalProperty.NextVisible(false))
                height += EditorGUI.GetPropertyHeight(internalProperty) + SPACE_BETWEEN_ELEMENTS;
            return height + SPACE_AFTER_ELEMENT;
        }
    }
}