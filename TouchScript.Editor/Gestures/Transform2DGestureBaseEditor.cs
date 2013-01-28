using System;
using TouchScript.Gestures;
using UnityEditor;
using UnityEngine;

namespace TouchScript.Editor.Gestures {
    public class Transform2DGestureBaseEditor : GestureEditor {
        public override void OnInspectorGUI() {
            var instance = target as Transform2DGestureBase;

            serializedObject.Update();
            EditorGUIUtility.LookLikeControls();
            instance.Projection = (Transform2DGestureBase.ProjectionType)EditorGUILayout.EnumPopup("Projection type", instance.Projection);
            if (instance.Projection != Transform2DGestureBase.ProjectionType.Camera) {
                instance.ProjectionNormal = EditorGUILayout.Vector3Field("Projection normal", instance.ProjectionNormal);
            }
            serializedObject.ApplyModifiedProperties();
            base.OnInspectorGUI();
        }

    }
}
