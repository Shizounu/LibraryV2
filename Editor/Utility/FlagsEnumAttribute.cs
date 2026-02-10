using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Shizounu.Library.Editor.Utility
{
    [AttributeUsage(AttributeTargets.Field, Inherited = true)]
    public class FlagsEnumAttribute : PropertyAttribute
    {
    }

    #if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(FlagsEnumAttribute))]
    public class FlagsEnumAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.Enum || fieldInfo == null)
            {
                EditorGUI.PropertyField(position, property, label, true);
                return;
            }

            Type enumType = fieldInfo.FieldType;
            if (!enumType.IsEnum)
            {
                EditorGUI.PropertyField(position, property, label, true);
                return;
            }

            EditorGUI.BeginProperty(position, label, property);
            Enum currentValue = (Enum)Enum.ToObject(enumType, property.intValue);
            Enum newValue = EditorGUI.EnumFlagsField(position, label, currentValue);
            property.intValue = Convert.ToInt32(newValue);
            EditorGUI.EndProperty();
        }
    }
    #endif
}
