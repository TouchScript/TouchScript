using System;
using TouchScript.Layers;
using UnityEditor;
using UnityEngine;

namespace TouchScript.Editor {

    [CustomEditor(typeof(TouchManager))]
    public class TouchManagerEditor : UnityEditor.Editor {

        private TouchManager instance;
        private bool showLayers = true;

        private void Awake() {
            instance = target as TouchManager;
        }

        public override void OnInspectorGUI() {
            //base.OnInspectorGUI();
            instance.LiveDPI = EditorGUILayout.FloatField("Live DPI", instance.LiveDPI);
            instance.EditorDPI = EditorGUILayout.FloatField("Editor DPI", instance.EditorDPI);
            instance.TouchRadius = EditorGUILayout.FloatField("Touch Radius (cm)", instance.TouchRadius);

            var layers = instance.Layers;
            showLayers = EditorGUILayout.Foldout(showLayers, String.Format("Layers ({0})", layers.Count));
            if (showLayers) {
                var moveDown = -1;
                var moveUp = -1;
                EditorGUILayout.BeginVertical();
                for (var i = 0; i < layers.Count; i++) {
                    var layer = layers[i];
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("^", GUILayout.MaxWidth(30))) moveUp = i;
                    if (GUILayout.Button("V", GUILayout.MaxWidth(30))) moveDown = i;
                    EditorGUILayout.LabelField("Layer: " + layer.Name);
                    EditorGUILayout.EndVertical();
                }
                EditorGUILayout.EndHorizontal();

                if (moveUp > 0) {
                    instance.ChangeLayerIndex(moveUp, moveUp-1);
                }
                if (moveDown != -1 && moveDown < layers.Count-1) {
                    instance.ChangeLayerIndex(moveDown, moveDown+1);
                }
            }

            if (GUI.changed)
                EditorUtility.SetDirty(instance);
        }

    }
}
