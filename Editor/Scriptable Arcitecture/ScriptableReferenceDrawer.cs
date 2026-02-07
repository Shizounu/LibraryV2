using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Shizounu.Library.Editor
{
    public class ScriptableReferenceDrawer : PropertyDrawer {
        private readonly string[] popupOptions = {
            "Use Constant",
            "Use Variable"
        };

        private GUIStyle popupStyle;


        private bool isExpanded;


        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label){
            if(popupStyle == null){
                popupStyle = new GUIStyle(GUI.skin.GetStyle("PaneOptions"));
                popupStyle.imagePosition = ImagePosition.ImageOnly;
            }

            label = EditorGUI.BeginProperty(position, label, property);
            position = EditorGUI.PrefixLabel(position, label);
            EditorGUI.BeginChangeCheck();

            // Get properties
            SerializedProperty useConstant = property.FindPropertyRelative("useConstant");
            SerializedProperty constantValue = property.FindPropertyRelative("ConstantValue");
            SerializedProperty variable = property.FindPropertyRelative("Variable");

            // Calculate rect for configuration button
            Rect buttonRect = new Rect(position);
            buttonRect.yMin += popupStyle.margin.top;
            buttonRect.width = popupStyle.fixedWidth + popupStyle.margin.right;
            position.xMin = buttonRect.xMax;

            // Store old indent level and set it to 0, the PrefixLabel takes care of it
            int indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            int result = EditorGUI.Popup(buttonRect, useConstant.boolValue ? 0 : 1, popupOptions, popupStyle);
            useConstant.boolValue = result == 0;

            EditorGUI.PropertyField(position, 
                useConstant.boolValue ? constantValue : variable, 
                GUIContent.none);

            if (EditorGUI.EndChangeCheck())
                property.serializedObject.ApplyModifiedProperties();

            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return base.GetPropertyHeight(property, label);
        }
    }
    
    [CustomPropertyDrawer(typeof(Shizounu.Library.ScriptableArchitecture.Vector3Reference))]
    public class ScriptableVector3Drawer : ScriptableReferenceDrawer{}

    [CustomPropertyDrawer(typeof(Shizounu.Library.ScriptableArchitecture.FloatReference))]
    public class ScriptableFloatDrawer : ScriptableReferenceDrawer{}

    [CustomPropertyDrawer(typeof(Shizounu.Library.ScriptableArchitecture.IntReference))]
    public class ScriptableIntDrawer : ScriptableReferenceDrawer{}

    [CustomPropertyDrawer(typeof(Shizounu.Library.ScriptableArchitecture.BoolReference))]
    public class ScriptablBoolDrawer : ScriptableReferenceDrawer{}
}