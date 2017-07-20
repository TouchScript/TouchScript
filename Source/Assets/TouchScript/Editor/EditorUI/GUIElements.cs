/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using UnityEngine;
using UnityEditor;
using System.Reflection;

namespace TouchScript.Editor.EditorUI
{
    internal static class GUIElements
    {
		public static GUIStyle BoxStyle;
		public static GUIStyle BoxLabelStyle;

		public static GUIStyle HelpBox;
		public static GUIStyle HeaderStyle;
		public static GUIStyle HeaderCheckbox;
		public static GUIStyle HeaderFoldout;

		public static Texture2D PaneOptionsIcon;

        static GUIElements()
        {
			BoxStyle = new GUIStyle(GUI.skin.box)
			{
				margin = new RectOffset(0, 0, 1, 0),
				padding = new RectOffset(0, 0, 0, 0),
				contentOffset = new Vector2(0, 0),
				alignment = TextAnchor.MiddleCenter,
			};
			BoxStyle.normal.textColor = GUI.skin.label.normal.textColor;

			BoxLabelStyle = new GUIStyle(GUI.skin.label)
			{
				fontSize = 9,
				padding = new RectOffset(0, 0, 5, 0),
			};

			HelpBox = new GUIStyle("HelpBox")
			{
				wordWrap = true,
			};

			HeaderStyle = new GUIStyle("ShurikenModuleTitle")
			{
				font = (new GUIStyle("Label")).font,
				border = new RectOffset(15, 7, 4, 4),
				fixedHeight = 22,
				contentOffset = new Vector2(20f, -2f),
			};

			HeaderCheckbox = new GUIStyle("ShurikenCheckMark");
			HeaderFoldout = new GUIStyle("Foldout");

			if (EditorGUIUtility.isProSkin)
				PaneOptionsIcon = (Texture2D)EditorGUIUtility.LoadRequired("Builtin Skins/DarkSkin/Images/pane options.png");
			else
				PaneOptionsIcon = (Texture2D)EditorGUIUtility.LoadRequired("Builtin Skins/LightSkin/Images/pane options.png");
        }

		public static bool Header(GUIContent title, SerializedProperty expanded, SerializedProperty enabled = null, PropertyInfo enabledProp = null)
		{
			var rect = GUILayoutUtility.GetRect(16f, 22f, HeaderStyle);
			GUI.Box(rect, title, HeaderStyle);

			var display = expanded == null || expanded.isExpanded;

			var foldoutRect = new Rect(rect.x + 4f, rect.y + 3f, 13f, 13f);
			var e = Event.current;

			if (e.type == EventType.Repaint)
			{
				if (enabled == null) HeaderFoldout.Draw(foldoutRect, false, false, display, false);
				else HeaderCheckbox.Draw(foldoutRect, false, false, enabled.boolValue, false);
			}

			if (e.type == EventType.MouseDown)
			{
				if (enabled != null)
				{
					const float kOffset = 2f;
					foldoutRect.x -= kOffset;
					foldoutRect.y -= kOffset;
					foldoutRect.width += kOffset * 2f;
					foldoutRect.height += kOffset * 2f;

					if (foldoutRect.Contains(e.mousePosition))
					{
						enabled.boolValue = !enabled.boolValue;
						if (enabledProp != null) enabledProp.SetValue(enabled.serializedObject.targetObject, enabled.boolValue, null);
						e.Use();
						return display;
					}
				}
				if (rect.Contains(e.mousePosition))
				{
					display = !display;
					expanded.isExpanded = !expanded.isExpanded;
					e.Use();
				}
			}

			return display;
		}
    }
}
