/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using TouchScript.Gestures.TransformGestures;
using UnityEditor;
using UnityEngine;

namespace TouchScript.Editor.Gestures.TransformGestures.Base
{
	internal class TwoPointTransformGestureBaseEditor : TransformGestureBaseEditor
    {

		protected override void drawBasic()
		{
			var typeValue = type.intValue;
			int newType = 0;
			EditorGUILayout.LabelField(TEXT_TYPE);
			EditorGUI.indentLevel++;
			EditorGUILayout.BeginHorizontal();
            {
                var rect = GUILayoutUtility.GetRect(86, 20);
                if (EditorGUI.ToggleLeft(rect, TEXT_TYPE_TRANSLATION,
                    (typeValue & (int)TransformGesture.TransformType.Translation) != 0))
                    newType |= (int)TransformGesture.TransformType.Translation;
                rect = GUILayoutUtility.GetRect(70, 20);
                if (EditorGUI.ToggleLeft(rect, TEXT_TYPE_ROTATION,
                    (typeValue & (int)TransformGesture.TransformType.Rotation) != 0))
                    newType |= (int)TransformGesture.TransformType.Rotation;
                rect = GUILayoutUtility.GetRect(64, 20);
                if (EditorGUI.ToggleLeft(rect, TEXT_TYPE_SCALING,
                    (typeValue & (int)TransformGesture.TransformType.Scaling) != 0))
                    newType |= (int)TransformGesture.TransformType.Scaling;
                GUILayoutUtility.GetRect(0, 20, GUILayout.ExpandWidth(true));
                type.intValue = newType;
            }
			EditorGUILayout.EndHorizontal();
			EditorGUI.indentLevel--;
		}

        protected override void drawGeneral()
        {
			var typeValue = type.intValue;
			int newType = 0;
			EditorGUILayout.LabelField(TEXT_TYPE);
			EditorGUI.indentLevel++;
			EditorGUILayout.BeginVertical();
			if (EditorGUILayout.ToggleLeft(TEXT_TYPE_TRANSLATION,
				(typeValue & (int)TransformGesture.TransformType.Translation) != 0))
				newType |= (int)TransformGesture.TransformType.Translation;
			if (EditorGUILayout.ToggleLeft(TEXT_TYPE_ROTATION,
				(typeValue & (int)TransformGesture.TransformType.Rotation) != 0))
				newType |= (int)TransformGesture.TransformType.Rotation;
			if (EditorGUILayout.ToggleLeft(TEXT_TYPE_SCALING,
				(typeValue & (int)TransformGesture.TransformType.Scaling) != 0))
				newType |= (int)TransformGesture.TransformType.Scaling;
			type.intValue = newType;
			EditorGUILayout.EndVertical();
			EditorGUI.indentLevel--;

            EditorGUIUtility.labelWidth = 160;
            EditorGUILayout.PropertyField(minScreenPointsDistance, TEXT_MIN_SCREEN_POINTS_DISTANCE);
            EditorGUILayout.PropertyField(screenTransformThreshold, TEXT_SCREEN_TRANSFORM_THRESHOLD);

			base.drawGeneral();
        }
    }
}
