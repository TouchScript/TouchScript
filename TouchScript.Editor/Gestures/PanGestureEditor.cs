/**
 * @author Valentin Simonov / http://va.lent.in/
 */

using TouchScript.Gestures;
using UnityEditor;

namespace TouchScript.Editor.Gestures
{
    [CustomEditor(typeof(PanGesture))]
    public class PanGestureEditor : Transform2DGestureBaseEditor
    {
        public override void OnInspectorGUI()
        {
            var instance = target as PanGesture;

            serializedObject.Update();
            EditorGUIUtility.LookLikeControls();
            instance.MovementThreshold = EditorGUILayout.FloatField("Movement threshold", instance.MovementThreshold);
            serializedObject.ApplyModifiedProperties();
            base.OnInspectorGUI();
        }
    }
}