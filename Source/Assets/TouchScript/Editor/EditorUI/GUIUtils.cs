/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using UnityEngine;

namespace TouchScript.Editor.EditorUI
{

    public static class GUIUtils
    {

		public static Rect GetPaddedRect(int minHeight, int padding, bool expandHeight = false)
		{
			Rect rect;
			if (expandHeight)
				rect = GUILayoutUtility.GetRect(padding * 2, minHeight + padding * 2, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
			else
				rect = GUILayoutUtility.GetRect(padding * 2, minHeight + padding * 2, GUILayout.ExpandWidth(true));
			ContractRect(ref rect, padding);
			return rect;
		}

		public static void ContractRect(ref Rect rect, int delta)
		{
			rect.x += delta;
			rect.y += delta;
			rect.width -= delta * 2;
			rect.height -= delta * 2;
		}

	}

}