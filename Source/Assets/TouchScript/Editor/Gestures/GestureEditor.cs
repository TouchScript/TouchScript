/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using TouchScript.Editor.EditorUI;
using TouchScript.Gestures;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using System.Reflection;

namespace TouchScript.Editor.Gestures
{
    [CustomEditor(typeof(Gesture), true)]
    internal class GestureEditor : UnityEditor.Editor
    {
        private const string FRIENDLY_GESTURES_PROP = "friendlyGestures";
        
		public static readonly GUIContent TEXT_GENERAL_HEADER = new GUIContent("General settings", "General settings.");
		public static readonly GUIContent TEXT_LIMITS_HEADER = new GUIContent("Limits", "Properties that limit the gesture.");
		public static readonly GUIContent TEXT_GESTURES_HEADER = new GUIContent("Interaction with other Gestures", "Settings which allow this gesture to interact with other gestures.");
		public static readonly GUIContent TEXT_ADVANCED_HEADER = new GUIContent("Advanced", "Advanced properties.");
		public static readonly GUIContent TEXT_USE_SEND_MESSAGE_HEADER = new GUIContent("Use SendMessage", "Enables sending events through SendMessage. Warnning: this method is slow!");
		public static readonly GUIContent TEXT_USE_UNITY_EVENTS_HEADER = new GUIContent("Use Unity Events", "Enables sending events through Unity Events.");

		public static readonly GUIContent TEXT_FRIENDLY = new GUIContent("Friendly Gestures", "List of gestures which can work together with this gesture.");
		public static readonly GUIContent TEXT_DEBUG_MODE = new GUIContent("Debug", "Turns on gesture debug mode.");
		public static readonly GUIContent TEXT_SEND_STATE_CHANGE_MESSAGES = new GUIContent("Send State Change Messages", "If checked, the gesture will send a message for every state change. Gestures usually have their own more specific messages, so you should keep this toggle unchecked unless you really want state change messages.");
		public static readonly GUIContent TEXT_SEND_MESSAGE_TARGET = new GUIContent("Target", "The GameObject target of Unity Messages. If null, host GameObject is used.");
		public static readonly GUIContent TEXT_SEND_STATE_CHANGE_EVENTS = new GUIContent("Send State Change Events", "If checked, the gesture will send a events for every state change. Gestures usually have their own more specific messages, so you should keep this toggle unchecked unless you really want state change events.");
		public static readonly GUIContent TEXT_REQUIRE_GESTURE_TO_FAIL = new GUIContent("Require Other Gesture to Fail", "Another gesture must fail for this gesture to start.");
		public static readonly GUIContent TEXT_LIMIT_POINTERS = new GUIContent(" Limit Pointers", "");

		protected bool shouldDrawAdvanced = false;
		protected bool shouldDrawGeneral = true;

		private Gesture instance;

        private SerializedProperty basicEditor;
        private SerializedProperty debugMode, friendlyGestures, requireGestureToFail,
        	minPointers, maxPointers, 
        	useSendMessage, sendMessageTarget, sendStateChangeMessages,
			useUnityEvents, sendStateChangeEvents;
		private SerializedProperty OnStateChange;
		private SerializedProperty advancedProps, limitsProps, generalProps;
		private PropertyInfo useUnityEvents_prop, useSendMessage_prop;

        private ReorderableList friendlyGesturesList;
        private int indexToRemove = -1;
        private float minPointersFloat, maxPointersFloat;

        protected virtual void OnEnable()
        {
			instance = target as Gesture;

            advancedProps = serializedObject.FindProperty("advancedProps");
			limitsProps = serializedObject.FindProperty("limitsProps");
			generalProps = serializedObject.FindProperty("generalProps");
            basicEditor = serializedObject.FindProperty("basicEditor");

            debugMode = serializedObject.FindProperty("debugMode");
            friendlyGestures = serializedObject.FindProperty("friendlyGestures");
            requireGestureToFail = serializedObject.FindProperty("requireGestureToFail");
            useSendMessage = serializedObject.FindProperty("useSendMessage");
            sendMessageTarget = serializedObject.FindProperty("sendMessageTarget");
            sendStateChangeMessages = serializedObject.FindProperty("sendStateChangeMessages");
			useUnityEvents = serializedObject.FindProperty("useUnityEvents");
			sendStateChangeEvents = serializedObject.FindProperty("sendStateChangeEvents");
            minPointers = serializedObject.FindProperty("minPointers");
            maxPointers = serializedObject.FindProperty("maxPointers");

			OnStateChange = serializedObject.FindProperty("OnStateChange");

			var type = instance.GetType();
			useUnityEvents_prop = type.GetProperty("UseUnityEvents", BindingFlags.Instance | BindingFlags.Public);
			useSendMessage_prop = type.GetProperty("UseSendMessage", BindingFlags.Instance | BindingFlags.Public);

            minPointersFloat = minPointers.intValue;
            maxPointersFloat = maxPointers.intValue;

            friendlyGesturesList = new ReorderableList(serializedObject, friendlyGestures, false, true, false, true);
            friendlyGesturesList.drawHeaderCallback += (rect) => GUI.Label(rect, TEXT_FRIENDLY);
            friendlyGesturesList.drawElementCallback += (rect, index, active, focused) =>
            {
                rect.height = 16;
                var gesture = friendlyGestures.GetArrayElementAtIndex(index).objectReferenceValue as Gesture;
                if (gesture == null)
                {
                    // Killing null elements.
                    indexToRemove = index;
                    EditorGUI.LabelField(rect, GUIContent.none);
                    return;
                }
                EditorGUI.LabelField(rect, string.Format("{0} @ {1}", gesture.GetType().Name, gesture.name), GUIElements.BoxLabel);
            };
            friendlyGesturesList.onRemoveCallback += list => { indexToRemove = list.index; };
        }

        public override void OnInspectorGUI()
        {
#if UNITY_5_6_OR_NEWER
			serializedObject.UpdateIfRequiredOrScript();
#else
			serializedObject.UpdateIfDirtyOrScript();
#endif

			GUILayout.Space(5);
			bool display;

			if (basicEditor.boolValue)
			{
                drawBasic();
				if (GUIElements.BasicHelpBox(getHelpText()))
				{
					basicEditor.boolValue = false;
					Repaint();
				}
			}
			else
			{
				if (shouldDrawGeneral)
				{
					display = GUIElements.Header(TEXT_GENERAL_HEADER, generalProps);
					if (display)
					{
						EditorGUI.indentLevel++;
						drawGeneral();
						EditorGUI.indentLevel--;
					}
				}

				drawOtherGUI();

				display = GUIElements.Header(TEXT_LIMITS_HEADER, limitsProps);
				if (display)
				{
					EditorGUI.indentLevel++;
					drawLimits();
					EditorGUI.indentLevel--;
				}

				display = GUIElements.Header(TEXT_GESTURES_HEADER, friendlyGestures);
				if (display)
				{
					EditorGUI.indentLevel++;
					drawFriendlyGestures();
					drawRequireToFail();
					GUILayout.Space(5);
					EditorGUI.indentLevel--;
				}

				display = GUIElements.Header(TEXT_USE_UNITY_EVENTS_HEADER, useUnityEvents, useUnityEvents, useUnityEvents_prop);
				if (display)
				{
					EditorGUI.indentLevel++;
					using (new EditorGUI.DisabledGroupScope(!useUnityEvents.boolValue))
					{
						drawUnityEvents();
					}
					EditorGUI.indentLevel--;
				}

				display = GUIElements.Header(TEXT_USE_SEND_MESSAGE_HEADER, useSendMessage, useSendMessage, useSendMessage_prop);
				if (display)
				{
					EditorGUI.indentLevel++;
					using (new EditorGUI.DisabledGroupScope(!useSendMessage.boolValue))
					{
						drawSendMessage();
					}
					EditorGUI.indentLevel--;
				}

				if (shouldDrawAdvanced)
				{
					display = GUIElements.Header(TEXT_ADVANCED_HEADER, advancedProps);
					if (display)
					{
						EditorGUI.indentLevel++;
						drawAdvanced();
						EditorGUI.indentLevel--;
					}
				}

                drawDebug();
			}

            serializedObject.ApplyModifiedProperties();
        }

        protected virtual void drawBasic()
        {
            
        }

        protected virtual GUIContent getHelpText()
        {
            return new GUIContent("");
        }

		protected virtual void drawOtherGUI()
		{

		}

		protected virtual void drawGeneral()
		{

		}

		protected virtual void drawLimits()
		{
			var limitPointers = (minPointers.intValue > 0) || (maxPointers.intValue > 0);
			var newLimitPointers = EditorGUILayout.ToggleLeft(TEXT_LIMIT_POINTERS, limitPointers);
			if (newLimitPointers)
			{
				if (!limitPointers)
				{
					minPointersFloat = 0;
					maxPointersFloat = 10;
				}
				else
				{
					minPointersFloat = (float) minPointers.intValue;
					maxPointersFloat = (float) maxPointers.intValue;
				}
				//or this values doesn't change from script properly
				EditorGUI.indentLevel++;
				EditorGUILayout.LabelField("Min: " + (int)minPointersFloat + ", Max: " + (int)maxPointersFloat);
				EditorGUILayout.MinMaxSlider(ref minPointersFloat, ref maxPointersFloat, 0, 10, GUILayout.MaxWidth(150));
				EditorGUI.indentLevel--;
			}
			else
			{
				if (limitPointers)
				{
					minPointersFloat = 0;
					maxPointersFloat = 0;
				}
			}

			minPointers.intValue = (int)minPointersFloat;
			maxPointers.intValue = (int)maxPointersFloat;
		}

		protected virtual void drawFriendlyGestures()
		{
			GUILayout.Space(5);
			drawGestureList(friendlyGestures, addFriendlyGesture);
			GUILayout.Space(5);
		}

		protected virtual void drawUnityEvents()
		{
			EditorGUILayout.PropertyField(OnStateChange);
			EditorGUILayout.PropertyField(sendStateChangeEvents, TEXT_SEND_STATE_CHANGE_EVENTS);
		}

        protected virtual void drawSendMessage()
        {
            EditorGUILayout.PropertyField(sendMessageTarget, TEXT_SEND_MESSAGE_TARGET);
            EditorGUILayout.PropertyField(sendStateChangeMessages, TEXT_SEND_STATE_CHANGE_MESSAGES);
        }

        protected virtual void drawAdvanced()
        {
        }

		protected virtual void drawDebug()
		{
			if (debugMode == null) return;
			EditorGUILayout.PropertyField(debugMode, TEXT_DEBUG_MODE);
		}

		protected virtual void drawRequireToFail()
		{
			EditorGUILayout.PropertyField(requireGestureToFail, TEXT_REQUIRE_GESTURE_TO_FAIL);
		}

        #region Gesture List

        private void drawGestureList(SerializedProperty prop, Action<SerializedProperty, Gesture> addGesture)
        {
            indexToRemove = -1;

//			Rect listRect = EditorGUI.IndentedRect(GUILayoutUtility.GetRect(0.0f, (prop.arraySize == 0 ? 0 : prop.arraySize - 1) * 16 + 60, GUILayout.ExpandWidth(true)));
//			friendlyGesturesList.DoList(listRect);
			friendlyGesturesList.DoLayoutList();

            GUILayout.Space(9);

			Rect dropArea = GUILayoutUtility.GetRect(0.0f, 50.0f, GUIElements.Box, GUILayout.ExpandWidth(true));
            GUI.Box(dropArea, "Drag a Gesture Here", GUIElements.Box);
            switch (Event.current.type)
            {
                case EventType.DragUpdated:
                    if (dropArea.Contains(Event.current.mousePosition))
                        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    break;
                case EventType.DragPerform:
                    if (dropArea.Contains(Event.current.mousePosition))
                    {
                        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                        DragAndDrop.AcceptDrag();

                        foreach (UnityEngine.Object obj in DragAndDrop.objectReferences)
                        {
                            if (obj is GameObject)
                            {
                                var go = obj as GameObject;
                                Gesture[] gestures = go.GetComponents<Gesture>();
                                foreach (Gesture gesture in gestures)
                                {
                                    addGesture(prop, gesture);
                                }
                            }
                            else if (obj is Gesture)
                            {
                                addGesture(prop, obj as Gesture);
                            }
                        }

                        Event.current.Use();
                    }
                    break;
            }

            if (indexToRemove > -1)
            {
                removeFriendlyGestureAt(prop, indexToRemove);
            }
        }

        private void addFriendlyGesture(SerializedProperty prop, Gesture value)
        {
            if (value == null || value == target) return;

            // Adding that gesture to this gesture.
            var shouldAdd = true;
            for (int i = 0; i < prop.arraySize; i++)
            {
                if (prop.GetArrayElementAtIndex(i).objectReferenceValue == value)
                {
                    shouldAdd = false;
                    break;
                }
            }

            if (shouldAdd)
            {
                prop.arraySize++;
                prop.GetArrayElementAtIndex(prop.arraySize - 1).objectReferenceValue = value;
            }

            // Adding this gesture to that gesture.
            shouldAdd = true;
            var so = new SerializedObject(value);
            so.Update();
            SerializedProperty p = so.FindProperty(FRIENDLY_GESTURES_PROP);
            for (int i = 0; i < p.arraySize; i++)
            {
                if (p.GetArrayElementAtIndex(i).objectReferenceValue == target)
                {
                    shouldAdd = false;
                    break;
                }
            }

            if (shouldAdd)
            {
                p.arraySize++;
                p.GetArrayElementAtIndex(p.arraySize - 1).objectReferenceValue = target;
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(value);
            }
        }

        private Gesture removeFriendlyGestureAt(SerializedProperty prop, int index)
        {
            // Removing that gesture from this gesture.
            var gesture = prop.GetArrayElementAtIndex(index).objectReferenceValue as Gesture;
            removeFromArray(prop, index);

            if (gesture == null) return null;

            // Removing this gesture from that gesture.
            var so = new SerializedObject(gesture);
            so.Update();
            SerializedProperty p = so.FindProperty(FRIENDLY_GESTURES_PROP);
            for (int j = 0; j < p.arraySize; j++)
            {
                if (p.GetArrayElementAtIndex(j).objectReferenceValue == target)
                {
                    removeFromArray(p, j);
                    break;
                }
            }

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(gesture);

            return gesture;
        }

        // A hack to remove a gesture from a list.
        // Was needed because array.DeleteArrayElementAtIndex() wasn't actually deleting an item.
        private void removeFromArray(SerializedProperty array, int index)
        {
            if (index != array.arraySize - 1)
            {
                array.GetArrayElementAtIndex(index).objectReferenceValue =
                    array.GetArrayElementAtIndex(array.arraySize - 1).objectReferenceValue;
            }
            array.arraySize--;
        }

        #endregion
    }
}