/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using TouchScript.Utils.Attributes;
using UnityEditor;
using UnityEngine;

namespace TouchScript.Editor.Utils.PropertyDrawers
{
    [CustomPropertyDrawer(typeof(NullToggleAttribute))]
    internal sealed class NullToggleDrawer : PropertyDrawer
    {
        private class SharedData
        {
            internal bool expanded = false;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var data = updateExpanded(property);
            if (data.expanded == false) return 16;
            if (property.propertyType == SerializedPropertyType.ObjectReference && property.objectReferenceValue != null) return 16 * 3 + 2 * 2;
            return 16 * 2 + 2;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var data = updateExpanded(property);

            EditorGUIUtility.labelWidth = 60;
            bool expandedChanged = Begin(data, position, property, label);
            if (data.expanded == false)
            {
                if (expandedChanged)
                {
                    switch (property.propertyType)
                    {
                        case SerializedPropertyType.ObjectReference:
                            property.objectReferenceValue = (Object) getNullValue(property);
                            break;
                        case SerializedPropertyType.Integer:
                            property.intValue = (int) getNullValue(property);
                            break;
                        case SerializedPropertyType.Float:
                            property.floatValue = (float) getNullValue(property);
                            break;
                    }
                }
            }
            else
            {
                EditorGUI.BeginChangeCheck();
                EditorGUI.LabelField(new Rect(position.x + 14, position.y + 18, 50, 16), new GUIContent("Value", label.tooltip));
                position = new Rect(position.x + 54, position.y + 18, Mathf.Min(position.width - 54, 100), 16);
                switch (property.propertyType)
                {
                    case SerializedPropertyType.ObjectReference:
                        var objValue = EditorGUI.ObjectField(position, GUIContent.none, property.objectReferenceValue, fieldInfo.FieldType, true);
                        if (EditorGUI.EndChangeCheck()) property.objectReferenceValue = objValue;
                        if (objValue != null)
                        {
                            position.y += 18;
                            position.width -= 18;
                            EditorGUI.LabelField(position, string.Format("of type {0}", objValue.GetType().Name), GUI.skin.FindStyle("ShurikenModuleTitle"));
                        }
                        break;
                    case SerializedPropertyType.Integer:
                        int intValue = EditorGUI.IntField(position, GUIContent.none, property.intValue);
                        if (EditorGUI.EndChangeCheck()) property.intValue = intValue;
                        break;
                    case SerializedPropertyType.Float:
                        float floatValue = EditorGUI.FloatField(position, GUIContent.none, property.floatValue);
                        if (EditorGUI.EndChangeCheck()) property.floatValue = floatValue;
                        break;
                }
            }
            End();
        }

        private bool Begin(SharedData data, Rect position, SerializedProperty property, GUIContent label)
        {
            label = EditorGUI.BeginProperty(position, label, property);
            label.text = " " + label.text;
            position.height = 16;
            EditorGUIUtility.labelWidth = 180;
            EditorGUI.BeginChangeCheck();
            data.expanded = EditorGUI.ToggleLeft(position, label, data.expanded == true);
            return EditorGUI.EndChangeCheck();
        }

        private void End()
        {
            EditorGUI.EndProperty();
        }

        private SharedData updateExpanded(SerializedProperty property)
        {
            var storage = SerializedPropertyUserData<SharedData>.Instance;
            var data = storage[property];
            if (data == null) storage[property] = data = new SharedData() { expanded = !isNull(property) };
            return data;
        }

        private bool isNull(SerializedProperty property)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.ObjectReference:
                    return ReferenceEquals(property.objectReferenceValue, getNullValue(property));
                case SerializedPropertyType.Integer:
                    return property.intValue == (int) getNullValue(property);
                case SerializedPropertyType.Float:
                    return property.floatValue == (float) getNullValue(property);
            }
            return false;
        }

        private object getNullValue(SerializedProperty property)
        {
            var attr = attribute as NullToggleAttribute;
            switch (property.propertyType)
            {
                case SerializedPropertyType.ObjectReference:
                    return attr.NullObjectValue;
                case SerializedPropertyType.Integer:
                    return attr.NullIntValue;
                case SerializedPropertyType.Float:
                    return attr.NullFloatValue;
            }
            return null;
        }
    }
}