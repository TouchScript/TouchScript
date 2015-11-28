/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System.Collections.Generic;
using System.Linq;
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
        private static readonly GUIContent CREATE_CAMERA_LAYER = new GUIContent("Create Camera Layer", "Indicates if TouchScript should create a CameraLayer for you if no layers present in a scene. This is usually a desired behavior but sometimes you would want to turn this off if you are using TouchScript only to get touch input from some device.");
        private static readonly GUIContent CREATE_STANDARD_INPUT = new GUIContent("Create Standard Input", "");
        private static readonly GUIContent USE_SEND_MESSAGE = new GUIContent("Use SendMessage", "If you use UnityScript or prefer using Unity Messages you can turn them on with this option.");
        private static readonly GUIContent SEND_MESSAGE_TARGET = new GUIContent("SendMessage Target", "The GameObject target of Unity Messages. If null, host GameObject is used.");
        private static readonly GUIContent SEND_MESSAGE_EVENTS = new GUIContent("SendMessage Events", "Which events should be sent as Unity Messages.");
        private static readonly GUIContent LAYERS_HEADER = new GUIContent("Touch Layers", "Sorted array of Touch Layers in the scene.");

        private TouchManager instance;
        private ReorderableList layersList;
        private SerializedProperty layers, displayDevice, shouldCreateCameraLayer, shouldCreateStandardInput, useSendMessage, sendMessageTarget, sendMessageEvents;

        private void OnEnable()
        {
            instance = target as TouchManager;
            layers = serializedObject.FindProperty("layers");
            displayDevice = serializedObject.FindProperty("displayDevice");
            shouldCreateCameraLayer = serializedObject.FindProperty("shouldCreateCameraLayer");
            shouldCreateStandardInput = serializedObject.FindProperty("shouldCreateStandardInput");
            useSendMessage = serializedObject.FindProperty("useSendMessage");
            sendMessageTarget = serializedObject.FindProperty("sendMessageTarget");
            sendMessageEvents = serializedObject.FindProperty("sendMessageEvents");

            refresh();

            layersList = new ReorderableList(serializedObject, layers, true, true, false, false);
            layersList.drawHeaderCallback += rect => GUI.Label(rect, LAYERS_HEADER);
            layersList.drawElementCallback += (rect, index, active, focused) =>
            {
                rect.height = 16;
                rect.y += 2;
                if (index >= layers.arraySize) return;
                var layer = layers.GetArrayElementAtIndex(index).objectReferenceValue as TouchLayer;
                if (layer == null)
                {
                    EditorGUI.LabelField(rect, "null");
                    return;
                }
                EditorGUI.LabelField(rect, layer.Name);
            };
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

            if (Application.isPlaying) GUI.enabled = false;
            EditorGUILayout.PropertyField(shouldCreateCameraLayer, CREATE_CAMERA_LAYER);
            EditorGUILayout.PropertyField(shouldCreateStandardInput, CREATE_STANDARD_INPUT);
            GUI.enabled = true;

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
            var allLayers = FindObjectsOfType(typeof(TouchLayer)).Cast<TouchLayer>().ToList();
            var toRemove = new List<int>();
            for (var i = 0; i < layers.arraySize; i++)
            {
                var layer = layers.GetArrayElementAtIndex(i).objectReferenceValue as TouchLayer;
                if (layer == null || allLayers.IndexOf(layer) == -1) toRemove.Add(i);
                else allLayers.Remove(layer);
            }

            for (var i = toRemove.Count - 1; i >= 0; i--)
            {
                var index = toRemove[i];
                layers.GetArrayElementAtIndex(index).objectReferenceValue = null;
                layers.DeleteArrayElementAtIndex(index);
            }

            for (var i = 0; i < allLayers.Count; i++)
            {
                layers.arraySize++;
                layers.GetArrayElementAtIndex(layers.arraySize - 1).objectReferenceValue = allLayers[i];
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
