/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using UnityEditor;
using UnityEngine;

namespace TouchScript.Editor.Utils
{
    public class GUIElements
    {

        private static GUIStyle foldoutStyle, headerStyle;

        public static GUIStyle BoxStyle
        {
            get
            {
                if (boxStyle == null)
                {
                    boxStyle = new GUIStyle(GUI.skin.box);
                    boxStyle.margin = new RectOffset(0, 0, 1, 0);
                    boxStyle.padding = new RectOffset(0, 0, 0, 0);
                    boxStyle.contentOffset = new Vector2(0, 0);
                    boxStyle.normal.textColor = GUI.skin.label.normal.textColor;
                    boxStyle.alignment = TextAnchor.MiddleCenter;
                }
                return boxStyle;
            }
        }

        public static GUIStyle BoxLabelStyle
        {
            get
            {
                if (boxLabelStyle == null)
                {
                    boxLabelStyle = new GUIStyle(GUI.skin.label);
                    boxLabelStyle.fontSize = 9;
                    boxLabelStyle.padding = new RectOffset(-10, 0, 5, 0);
                }
                return boxLabelStyle;
            }
        }

        private static GUIStyle boxStyle, boxLabelStyle;

        public static bool Foldout(bool open, GUIContent header, Action content)
        {
            if (foldoutStyle == null)
            {
                foldoutStyle = new GUIStyle(GUI.skin.FindStyle("ShurikenModuleBg"));
                foldoutStyle.padding = new RectOffset(10, 10, 10, 10);

                headerStyle = new GUIStyle(GUI.skin.FindStyle("ShurikenModuleTitle"));
                headerStyle.contentOffset = new Vector2(3, -2);
            }

            EditorGUIUtility.LookLikeInspector();
            GUILayout.BeginVertical("ShurikenEffectBg", GUILayout.MinHeight(1f));

            open = GUI.Toggle(GUILayoutUtility.GetRect(0, 16), open, header, headerStyle);
            if (open)
            {
                GUILayout.BeginVertical(foldoutStyle);

                content();

                GUILayout.EndVertical();
            }
            GUILayout.EndVertical();

            return open;
        }

        public static void CompactVector3(GUIContent content, SerializedProperty property)
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(content);
            var x = EditorGUILayout.FloatField(property.vector3Value.x, GUILayout.MaxWidth(80), GUILayout.MinWidth(40));
            var y = EditorGUILayout.FloatField(property.vector3Value.y, GUILayout.MaxWidth(80), GUILayout.MinWidth(40));
            var z = EditorGUILayout.FloatField(property.vector3Value.z, GUILayout.MaxWidth(80), GUILayout.MinWidth(40));
            property.vector3Value = new Vector3(x, y, z);
            GUILayout.EndHorizontal();
        }

    }
}
