/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using TouchScript.Devices.Display;
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
        private static readonly GUIContent DISPLAY_DEVICE = new GUIContent("Display Device", "Display device properties where such parameters as target DPI are stored.");
        private static readonly GUIContent USE_SEND_MESSAGE = new GUIContent("Use SendMessage", "If you use UnityScript or prefer using Unity Messages you can turn them on with this option.");
        private static readonly GUIContent SEND_MESSAGE_TARGET = new GUIContent("SendMessage Target", "The GameObject target of Unity Messages. If null, host GameObject is used.");
        private static readonly GUIContent SEND_MESSAGE_EVENTS = new GUIContent("SendMessage Events", "Which events should be sent as Unity Messages.");
        private static readonly GUIContent MOVE_DOWN = new GUIContent("v", "Move down");

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
            var newDevice = EditorGUILayout.ObjectField(DISPLAY_DEVICE, instance.DisplayDevice, typeof(DisplayDevice), true) as DisplayDevice;
            if (EditorGUI.EndChangeCheck())
            {
                instance.DisplayDevice = newDevice;
            }

            EditorGUIUtility.labelWidth = 160;
            EditorGUI.BeginChangeCheck();
            var useSendMessage = GUILayout.Toggle(instance.UseSendMessage, USE_SEND_MESSAGE);
            var sTarget = instance.SendMessageTarget;
            var sMask = instance.SendMessageEvents;
            if (useSendMessage)
            {
                sTarget = EditorGUILayout.ObjectField(SEND_MESSAGE_TARGET, sTarget, typeof(GameObject), true) as GameObject;
                sMask = (TouchManager.MessageTypes)EditorGUILayout.EnumMaskField(SEND_MESSAGE_EVENTS, instance.SendMessageEvents);
            }
            if (EditorGUI.EndChangeCheck())
            {
                instance.UseSendMessage = useSendMessage;
                instance.SendMessageTarget = sTarget;
                instance.SendMessageEvents = sMask;
                EditorUtility.SetDirty(instance);
            }

            if (Application.isPlaying) GUI.enabled = false;

            showLayers = GUIElements.BeginFoldout(showLayers, new GUIContent(String.Format("Layers ({0})", layers.arraySize)));
            if (showLayers)
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
                    if (GUILayout.Button(MOVE_DOWN, layerButtonStyle, GUILayout.Width(20), GUILayout.Height(18)))
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
            }
            GUIElements.EndFoldout();

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