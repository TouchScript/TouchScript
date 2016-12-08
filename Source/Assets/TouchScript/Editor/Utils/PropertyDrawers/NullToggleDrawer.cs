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
        private bool? expanded = null;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            updateExpanded(property);
            if (expanded == false) return 16;
            if (property.propertyType == SerializedPropertyType.ObjectReference && property.objectReferenceValue != null) return 16 * 3 + 2 * 2;
            return 16 * 2 + 2;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            updateExpanded(property);

            EditorGUIUtility.labelWidth = 60;
            Begin(position, property, label);
            if (expanded == false)
            {
                switch (property.propertyType)
                {
                    case SerializedPropertyType.ObjectReference:
                        property.objectReferenceValue = (Object)getNullValue(property);
                        break;
                    case SerializedPropertyType.Integer:
                        property.intValue = (int)getNullValue(property);
                        break;
                    case SerializedPropertyType.Float:
                        property.floatValue = (float)getNullValue(property);
                        break;
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


            //    case SerializedPropertyType.Float:
            //        {
            //            EditorGUI.BeginChangeCheck();
            //            float floatValue = EditorGUI.FloatField(position, label, property.floatValue);
            //            if (EditorGUI.EndChangeCheck())
            //            {
            //                property.floatValue = floatValue;
            //            }
            //            break;
            //        }
            //    case SerializedPropertyType.String:
            //        {
            //            EditorGUI.BeginChangeCheck();
            //            string stringValue = EditorGUI.TextField(position, label, property.stringValue);
            //            if (EditorGUI.EndChangeCheck())
            //            {
            //                property.stringValue = stringValue;
            //            }
            //            break;
            //        }
            //    case SerializedPropertyType.Color:
            //        {
            //            EditorGUI.BeginChangeCheck();
            //            Color colorValue = EditorGUI.ColorField(position, label, property.colorValue);
            //            if (EditorGUI.EndChangeCheck())
            //            {
            //                property.colorValue = colorValue;
            //            }
            //            break;
            //        }
            //    case SerializedPropertyType.LayerMask:
            //        EditorGUI.LayerMaskField(position, property, label);
            //        break;
            //    case SerializedPropertyType.Enum:
            //        EditorGUI.Popup(position, property, label);
            //        break;
            //    case SerializedPropertyType.Vector2:
            //        EditorGUI.Vector2Field(position, property, label);
            //        break;
            //    case SerializedPropertyType.Vector3:
            //        EditorGUI.Vector3Field(position, property, label);
            //        break;
            //    case SerializedPropertyType.Rect:
            //        EditorGUI.RectField(position, property, label);
            //        break;
            //    case SerializedPropertyType.AnimationCurve:
            //        {
            //            int controlID = GUIUtility.GetControlID(EditorGUI.s_CurveHash, EditorGUIUtility.native, position);
            //            EditorGUI.DoCurveField(EditorGUI.PrefixLabel(position, controlID, label), controlID, null, EditorGUI.kCurveColor, default(Rect), property);
            //            break;
            //        }
            //    case SerializedPropertyType.Bounds:
            //        EditorGUI.BoundsField(position, property, label);
            //        break;
            //    case SerializedPropertyType.Gradient:
            //        {
            //            int controlID2 = GUIUtility.GetControlID(EditorGUI.s_CurveHash, EditorGUIUtility.native, position);
            //            EditorGUI.DoGradientField(EditorGUI.PrefixLabel(position, controlID2, label), controlID2, null, property);
            //            break;
            //        }
        }

        private void Begin(Rect position, SerializedProperty property, GUIContent label)
        {
            label = EditorGUI.BeginProperty(position, label, property);
            label.text = " " + label.text;
            position.height = 16;
			EditorGUIUtility.labelWidth = 180;
            expanded = EditorGUI.ToggleLeft(position, label, expanded == true);
        }

        private void End()
        {
            EditorGUI.EndProperty();
        }

        private void updateExpanded(SerializedProperty property)
        {
            if (expanded != null) return;
            expanded = !isNull(property);
        }

        private bool isNull(SerializedProperty property)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.ObjectReference:
                    return ReferenceEquals(property.objectReferenceValue, getNullValue(property));
                case SerializedPropertyType.Integer:
                    return property.intValue == (int)getNullValue(property);
                case SerializedPropertyType.Float:
                    return property.floatValue == (float)getNullValue(property);
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
