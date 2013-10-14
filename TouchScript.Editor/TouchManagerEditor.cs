/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using TouchScript.Editor.Utils;
using TouchScript.Layers;
using UnityEditor;
using UnityEngine;

namespace TouchScript.Editor
{
    [CustomEditor(typeof(TouchManager))]
    public class TouchManagerEditor : UnityEditor.Editor
    {
        public const string TEXT_LIVEDPI = "DPI used in built app runing on target device.";
        public const string TEXT_EDITORDPI = "DPI used in the editor.";
        public const string TEXT_MOVEDOWN = "Move down.";

        private bool showLayers = false;

        private SerializedProperty liveDPI;
        private SerializedProperty editorDPI;
        private SerializedProperty layers;

        private GUIStyle layerButtonStyle;

        private void OnEnable()
        {
            liveDPI = serializedObject.FindProperty("liveDpi");
            editorDPI = serializedObject.FindProperty("editorDpi");
            layers = serializedObject.FindProperty("layers");
        }

        public override void OnInspectorGUI()
        {
            if (layerButtonStyle == null)
            {
                layerButtonStyle = new GUIStyle(EditorStyles.miniButton);
                layerButtonStyle.fontSize = 9;
                layerButtonStyle.padding = new RectOffset(-4, 0, -3, 0);
            }

            EditorGUIUtility.LookLikeInspector();

            serializedObject.Update();
            GUI.changed = false;

            EditorGUILayout.PropertyField(liveDPI, new GUIContent("Live DPI", TEXT_LIVEDPI));
            EditorGUILayout.PropertyField(editorDPI, new GUIContent("Editor DPI", TEXT_EDITORDPI));

            showLayers = GUIElements.Foldout(showLayers, new GUIContent(String.Format("Layers ({0})", layers.arraySize)), () =>
                {
                    EditorGUILayout.BeginVertical();
                    for (var i = 0; i < layers.arraySize; i++)
                    {
                        var layer = layers.GetArrayElementAtIndex(i).objectReferenceValue as TouchLayer;
                        string name;
                        if (layer == null) name = "Unknown";
                        else name = layer.Name;

                        var rect = EditorGUILayout.BeginHorizontal(GUIElements.BoxStyle, GUILayout.Height(23));
                        
                        EditorGUILayout.LabelField(name, GUIElements.BoxLabelStyle, GUILayout.ExpandWidth(true));
                        if (GUILayout.Button(new GUIContent("Move Down", TEXT_MOVEDOWN), layerButtonStyle, GUILayout.Width(70), GUILayout.Height(18)))
                        {
                            layers.MoveArrayElement(i, i + 1);
                        }
                        else if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
                        {
                            EditorGUIUtility.PingObject(layer);
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                    EditorGUILayout.EndVertical();

                    GUILayout.Space(5f);
                    if (GUILayout.Button("Refresh", GUILayout.MaxWidth(100))) refresh();
                });

            serializedObject.ApplyModifiedProperties();
        }

        private void refresh()
        {
            layers.ClearArray();
            var allLayers = FindObjectsOfType(typeof(TouchLayer));
            var i = 0;
            layers.arraySize = allLayers.Length;
            foreach (TouchLayer l in allLayers)
            {
                layers.GetArrayElementAtIndex(i).objectReferenceValue = l;
                i++;
            }
        }
    }
}