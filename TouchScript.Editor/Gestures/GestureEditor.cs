using System;
using TouchScript.Gestures;
using UnityEditor;
using UnityEngine;

namespace TouchScript.Editor.Gestures {

    [CustomEditor(typeof(Gesture))]
    public class GestureEditor : UnityEditor.Editor {
        public override void OnInspectorGUI() {
            var instance = target as Gesture;

            EditorGUIUtility.LookLikeInspector();
            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("WillRecognizeWith"), true);
            serializedObject.ApplyModifiedProperties();
        }

    }
}
