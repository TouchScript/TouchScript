/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using TouchScript.Layers;
using UnityEditor;
using UnityEngine;

namespace TouchScript.Editor
{
    [CustomEditor(typeof(TouchManager))]
    public class TouchManagerEditor : UnityEditor.Editor
    {
        private bool showLayers = false;

        private SerializedProperty liveDPI;
        private SerializedProperty editorDPI;
        private SerializedProperty layers;

        private void OnEnable()
        {
            liveDPI = serializedObject.FindProperty("liveDpi");
            editorDPI = serializedObject.FindProperty("editorDpi");
            layers = serializedObject.FindProperty("layers");
        }

        public override void OnInspectorGUI()
        {
            EditorGUIUtility.LookLikeInspector();

            var instance = target as TouchManager;

            serializedObject.Update();
            GUI.changed = false;

            liveDPI.floatValue = EditorGUILayout.FloatField("Live DPI", liveDPI.floatValue);
            editorDPI.floatValue = EditorGUILayout.FloatField("Editor DPI", editorDPI.floatValue);

            showLayers = EditorGUILayout.Foldout(showLayers, String.Format("Layers ({0})", layers.arraySize));
            if (showLayers)
            {
                EditorGUILayout.BeginVertical();
                for (var i = 0; i < layers.arraySize; i++)
                {
                    var layer = layers.GetArrayElementAtIndex(i).objectReferenceValue as TouchLayer;
                    string name;
                    if (layer == null)
                    {
                        name = "Unknown";
                    } else
                    {
                        name = layer.Name;
                    }
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("^", GUILayout.MaxWidth(30)))
                    {
                        layers.MoveArrayElement(i, i - 1);
                    }
                    if (GUILayout.Button("V", GUILayout.MaxWidth(30)))
                    {
                        layers.MoveArrayElement(i, i + 1);
                    }
                    EditorGUILayout.LabelField("Layer: " + name);
                    EditorGUILayout.EndVertical();
                }
                EditorGUILayout.EndHorizontal();

                if (GUILayout.Button("Refresh", GUILayout.MaxWidth(100)))
                {
                    refresh();
                }
            }

            serializedObject.ApplyModifiedProperties();
            if (GUI.changed) EditorUtility.SetDirty(instance);
        }

        private void refresh()
        {
            while (layers.arraySize > 0)
            {
                layers.DeleteArrayElementAtIndex(0);
            }
            var allLayers = FindObjectsOfType(typeof(TouchLayer));
            var i = 0;
            layers.arraySize = allLayers.Length;
            Debug.Log(allLayers.Length);
            foreach (TouchLayer l in allLayers)
            {
                layers.GetArrayElementAtIndex(i).objectReferenceValue = l;
                i++;
            }
        }
    }
}