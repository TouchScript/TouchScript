/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using TouchScript.Gestures;
using UnityEditor;

namespace TouchScript.Editor.Gestures
{
    [CustomEditor(typeof(Gesture))]
    public class GestureEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUIUtility.LookLikeInspector();
            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("WillRecognizeWith"), true);
            serializedObject.ApplyModifiedProperties();
        }
    }
}