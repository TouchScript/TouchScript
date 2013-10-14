using System;
using UnityEditor;
using UnityEngine;

namespace TouchScript.Editor.Utils
{
    public class GUIElements
    {

        private static GUIStyle foldoutStyle, headerStyle;

        public static bool DrawFoldout(bool open, GUIContent header, Action content)
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

        public static void DrawCompactVector3(GUIContent content, SerializedProperty property)
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
