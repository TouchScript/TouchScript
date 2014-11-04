/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using TouchScript.Layers;
using UnityEditor;
using UnityEngine;

namespace TouchScript.Editor.Layers
{
    [CustomEditor(typeof(FullscreenLayer))]
    internal sealed class FullscreenLayerEditor : UnityEditor.Editor
    {
        private SerializedProperty type, camera;
        private FullscreenLayer instance;

        private void OnEnable()
        {
            instance = target as FullscreenLayer;

            type = serializedObject.FindProperty("type");
            camera = serializedObject.FindProperty("_camera");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(type);
            if (EditorGUI.EndChangeCheck())
            {
                instance.Type = (FullscreenLayer.LayerType)type.enumValueIndex;
            }

            if (type.enumValueIndex == (int)FullscreenLayer.LayerType.Camera)
            {
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(camera);
                if (EditorGUI.EndChangeCheck())
                {
                    instance.Camera = camera.objectReferenceValue as Camera;
                }
            }
        }
    }
}
