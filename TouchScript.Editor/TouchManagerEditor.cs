/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using TouchScript.Editor.Utils;
using TouchScript.Layers;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TouchScript.Editor
{
    [CustomEditor(typeof(TouchManager))]
    public class TouchManagerEditor : UnityEditor.Editor
    {
        public const string TEXT_LIVEDPI = "DPI used in built app runing on target device.";
        public const string TEXT_EDITORDPI = "DPI used in the editor.";
        public const string TEXT_MOVEDOWN = "Move down.";
        public const string TEXT_USESENDMESSAGE = "If you use UnityScript or prefer using Unity Messages you can turn them on with this option.";
        public const string TEXT_SENDMESSAGETARGET = "The GameObject target of Unity Messages. If null, host GameObject is used.";
        public const string TEXT_SENDMESSAGEEVENTS = "Which events should be sent as Unity Messages.";

        private GUIStyle layerButtonStyle;

        private TouchManager instance;
        private SerializedProperty layers;
        private bool showLayers;

        private void OnEnable()
        {
            instance = target as TouchManager;
            layers = serializedObject.FindProperty("layers");
        }

        public override void OnInspectorGUI()
        {
            if (layerButtonStyle == null)
            {
                layerButtonStyle = new GUIStyle(EditorStyles.miniButton);
                layerButtonStyle.fontSize = 9;
                layerButtonStyle.contentOffset = new Vector2(0, 0);
            }

            serializedObject.Update();

            EditorGUI.BeginChangeCheck();
            var newLiveDPI = EditorGUILayout.IntField(new GUIContent("Live DPI", TEXT_LIVEDPI), (int)instance.LiveDPI);
            var newEditorDPI = EditorGUILayout.IntField(new GUIContent("Editor DPI", TEXT_EDITORDPI), (int)instance.EditorDPI);
            if (EditorGUI.EndChangeCheck())
            {
                instance.LiveDPI = newLiveDPI;
                instance.EditorDPI = newEditorDPI;
            }

            EditorGUI.BeginChangeCheck();
            var useSendMessage = EditorGUILayout.Toggle(new GUIContent("Use SendMessage", TEXT_USESENDMESSAGE), instance.UseSendMessage);
            var sTarget = instance.SendMessageTarget;
            var sMask = instance.SendMessageEvents;
            if (useSendMessage)
            {
                sTarget = EditorGUILayout.ObjectField(new GUIContent("SendMessage Target", TEXT_SENDMESSAGETARGET), sTarget, typeof(GameObject), true) as GameObject;
                sMask = (TouchManager.MessageTypes)EditorGUILayout.EnumMaskField(new GUIContent("SendMessage Events", TEXT_SENDMESSAGEEVENTS), instance.SendMessageEvents);
            }
            if (EditorGUI.EndChangeCheck())
            {
                instance.UseSendMessage = useSendMessage;
                instance.SendMessageTarget = sTarget;
                instance.SendMessageEvents = sMask;
            }

            if (Application.isPlaying) GUI.enabled = false;
            showLayers = GUIElements.Foldout(showLayers, new GUIContent(String.Format("Layers ({0})", layers.arraySize)),
                () =>
                {
                    EditorGUILayout.BeginVertical();
                    for (int i = 0; i < layers.arraySize; i++)
                    {
                        var layer = layers.GetArrayElementAtIndex(i).objectReferenceValue as TouchLayer;
                        string name;
                        if (layer == null) name = "Unknown";
                        else name = layer.Name;

                        var rect = EditorGUILayout.BeginHorizontal(GUIElements.BoxStyle, GUILayout.Height(23));

                        EditorGUILayout.LabelField(name, GUIElements.BoxLabelStyle, GUILayout.ExpandWidth(true));
                        if (GUILayout.Button(new GUIContent("v", TEXT_MOVEDOWN), layerButtonStyle, GUILayout.Width(20), GUILayout.Height(18)))
                        {
                            layers.MoveArrayElement(i, i + 1);
                        } else if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
                        {
                            EditorGUIUtility.PingObject(layer);
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                    EditorGUILayout.EndVertical();

                    GUILayout.Space(5f);
                    if (GUILayout.Button("Refresh", GUILayout.MaxWidth(100))) refresh();
                });

            GUI.enabled = true;
            serializedObject.ApplyModifiedProperties();
        }

        private void refresh()
        {
            layers.ClearArray();
            Object[] allLayers = FindObjectsOfType(typeof(TouchLayer));
            int i = 0;
            layers.arraySize = allLayers.Length;
            foreach (TouchLayer l in allLayers)
            {
                layers.GetArrayElementAtIndex(i).objectReferenceValue = l;
                i++;
            }
        }
    }
}