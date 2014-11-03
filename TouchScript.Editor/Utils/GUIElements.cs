/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using UnityEngine;

namespace TouchScript.Editor.Utils
{
    internal static class GUIElements
    {
        public static GUIStyle BoxStyle
        {
            get { return boxStyle; }
        }

        public static GUIStyle BoxLabelStyle
        {
            get { return boxLabelStyle; }
        }

        public static GUIStyle FoldoutStyle
        {
            get { return foldoutStyle; }
        }

        public static GUIStyle HeaderStyle
        {
            get { return foldoutStyle; }
        }

        private static GUIStyle boxStyle, boxLabelStyle;
        private static GUIStyle foldoutStyle, headerStyle;

        static GUIElements()
        {
            boxStyle = new GUIStyle(GUI.skin.box);
            boxStyle.margin = new RectOffset(0, 0, 1, 0);
            boxStyle.padding = new RectOffset(0, 0, 0, 0);
            boxStyle.contentOffset = new Vector2(0, 0);
            boxStyle.normal.textColor = GUI.skin.label.normal.textColor;
            boxStyle.alignment = TextAnchor.MiddleCenter;

            boxLabelStyle = new GUIStyle(GUI.skin.label);
            boxLabelStyle.fontSize = 9;
            boxLabelStyle.padding = new RectOffset(0, 0, 5, 0);

            foldoutStyle = new GUIStyle(GUI.skin.FindStyle("ShurikenModuleBg"));
            foldoutStyle.padding = new RectOffset(10, 10, 10, 10);

            headerStyle = new GUIStyle(GUI.skin.FindStyle("ShurikenModuleTitle"));
            headerStyle.contentOffset = new Vector2(3, -2);
        }

        public static bool BeginFoldout(bool open, GUIContent header)
        {
            GUILayout.BeginVertical("ShurikenEffectBg", GUILayout.MinHeight(16f));

            return GUI.Toggle(GUILayoutUtility.GetRect(0, 16), open, header, headerStyle);
        }

        public static void EndFoldout()
        {
            GUILayout.EndVertical();
        }
    }
}
