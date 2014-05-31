/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using TouchScript.Devices.Display;
using TouchScript.Layers;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TouchScript.Editor
{
    [CustomEditor(typeof(TouchManager))]
    internal sealed class TouchManagerEditor : UnityEditor.Editor
    {
        private static readonly GUIContent DISPLAY_DEVICE = new GUIContent("Display Device", "Display device properties where such parameters as target DPI are stored.");
        private static readonly GUIContent USE_SEND_MESSAGE = new GUIContent("Use SendMessage", "If you use UnityScript or prefer using Unity Messages you can turn them on with this option.");
        private static readonly GUIContent SEND_MESSAGE_TARGET = new GUIContent("SendMessage Target", "The GameObject target of Unity Messages. If null, host GameObject is used.");
        private static readonly GUIContent SEND_MESSAGE_EVENTS = new GUIContent("SendMessage Events", "Which events should be sent as Unity Messages.");
        private static readonly GUIContent LAYERS_HEADER = new GUIContent("Touch Layers", "Sorted array of Touch Layers in the scene.");

        private TouchManager instance;
        private ReorderableList layersList;
        private SerializedProperty layers, displayDevice, useSendMessage, sendMessageTarget, sendMessageEvents;

        private void OnEnable()
        {
            instance = target as TouchManager;
            layers = serializedObject.FindProperty("layers");
            displayDevice = serializedObject.FindProperty("displayDevice");
            useSendMessage = serializedObject.FindProperty("useSendMessage");
            sendMessageTarget = serializedObject.FindProperty("sendMessageTarget");
            sendMessageEvents = serializedObject.FindProperty("sendMessageEvents");

            layersList = new ReorderableList(serializedObject, layers, true, true, false, false);
            layersList.drawHeaderCallback += rect => GUI.Label(rect, LAYERS_HEADER);
            layersList.drawElementCallback += (rect, index, active, focused) =>
            {
                rect.height = 16;
                rect.y += 2;
                EditorGUI.LabelField(rect, (layersList.serializedProperty.GetArrayElementAtIndex(index).objectReferenceValue as TouchLayer).Name);
            };

            refresh();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var r = EditorGUILayout.GetControlRect(true, 16f, EditorStyles.objectField);
            var label = EditorGUI.BeginProperty(r, DISPLAY_DEVICE, displayDevice);
            EditorGUI.BeginChangeCheck();
            r = EditorGUI.PrefixLabel(r, label);
            var newDevice = EditorGUI.ObjectField(r, instance.DisplayDevice as Object, typeof(IDisplayDevice), true) as IDisplayDevice;
            if (EditorGUI.EndChangeCheck())
            {
                instance.DisplayDevice = newDevice;
                EditorUtility.SetDirty(instance);
            }
            EditorGUI.EndProperty();

            EditorGUIUtility.labelWidth = 160;
            EditorGUILayout.PropertyField(useSendMessage, USE_SEND_MESSAGE);
            if (useSendMessage.boolValue)
            {
                EditorGUILayout.PropertyField(sendMessageTarget, SEND_MESSAGE_TARGET);

                r = EditorGUILayout.GetControlRect(true, 16f, EditorStyles.layerMaskField);
                label = EditorGUI.BeginProperty(r, SEND_MESSAGE_EVENTS, sendMessageEvents);
                EditorGUI.BeginChangeCheck();
                r = EditorGUI.PrefixLabel(r, label);
                var sMask = (TouchManager.MessageType)EditorGUI.EnumMaskField(r, instance.SendMessageEvents);
                if (EditorGUI.EndChangeCheck())
                {
                    instance.SendMessageEvents = sMask;
                    EditorUtility.SetDirty(instance);
                }
                EditorGUI.EndProperty();
            }

            if (Application.isPlaying) GUI.enabled = false;

            layersList.DoLayoutList();

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

            serializedObject.ApplyModifiedProperties();
        }
    }
}