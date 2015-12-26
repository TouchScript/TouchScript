/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using TouchScript.Editor.Utils;
using TouchScript.Gestures;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace TouchScript.Editor.Gestures
{
    [CustomEditor(typeof(Gesture), true)]
    internal class GestureEditor : UnityEditor.Editor
    {
        private const string FRIENDLY_GESTURES_PROP = "friendlyGestures";
        private const string TEXT_FRIENDLY_HEADER = "List of gestures which can work together with this gesture.";

        private static readonly GUIContent TEXT_ADVANCED_HEADER = new GUIContent("Advanced", "Advanced properties.");
        private static readonly GUIContent DEBUG_MODE = new GUIContent("Debug", "Turns on gesture debug mode.");
        private static readonly GUIContent USE_SEND_MESSAGE = new GUIContent("Use SendMessage", "If you use UnityScript or prefer using Unity Messages you can turn them on with this option.");
        private static readonly GUIContent SEND_STATE_CHANGE_MESSAGES = new GUIContent("Send State Change Messages", "If checked, the gesture will send a message for every state change. Gestures usually have their own more specific messages, so you should keep this toggle unchecked unless you really want state change messages.");
        private static readonly GUIContent SEND_MESSAGE_TARGET = new GUIContent("Target", "The GameObject target of Unity Messages. If null, host GameObject is used.");
        private static readonly GUIContent COMBINE_TOUCH_POINTS = new GUIContent("Combine Touch Points", "When several fingers are used to perform a tap, touch points released not earlier than <CombineInterval> seconds ago are used to calculate gesture's final screen position.");
        private static readonly GUIContent COMBINE_TOUCH_POINTS_INTERVAL = new GUIContent("Combine Interval (sec)", COMBINE_TOUCH_POINTS.tooltip);
        private static readonly GUIContent REQUIRE_GESTURE_TO_FAIL = new GUIContent("Require Other Gesture to Fail", "Gesture which must fail for this gesture to start.");
        private static readonly GUIContent LIMIT_TOUCHES = new GUIContent("Limit Touches", "");

        protected bool shouldDrawCombineTouches = false;

        private SerializedProperty advanced;
        private SerializedProperty debugMode;
        private SerializedProperty friendlyGestures;
        private SerializedProperty requireGestureToFail;
        private SerializedProperty minTouches, maxTouches;
        private SerializedProperty combineTouches, combineTouchesInterval;
        private SerializedProperty useSendMessage, sendMessageTarget, sendStateChangeMessages;

        private ReorderableList friendlyGesturesList;
        private int indexToRemove = -1;
        private float minTouchesFloat, maxTouchesFloat;

        protected virtual void OnEnable()
        {
            hideFlags = HideFlags.HideAndDontSave;

            advanced = serializedObject.FindProperty("advancedProps");
            debugMode = serializedObject.FindProperty("debugMode");
            friendlyGestures = serializedObject.FindProperty("friendlyGestures");
            requireGestureToFail = serializedObject.FindProperty("requireGestureToFail");
            combineTouches = serializedObject.FindProperty("combineTouches");
            combineTouchesInterval = serializedObject.FindProperty("combineTouchesInterval");
            useSendMessage = serializedObject.FindProperty("useSendMessage");
            sendMessageTarget = serializedObject.FindProperty("sendMessageTarget");
            sendStateChangeMessages = serializedObject.FindProperty("sendStateChangeMessages");
            minTouches = serializedObject.FindProperty("minTouches");
            maxTouches = serializedObject.FindProperty("maxTouches");

            minTouchesFloat = minTouches.intValue;
            maxTouchesFloat = maxTouches.intValue;

            friendlyGesturesList = new ReorderableList(serializedObject, friendlyGestures, false, false, false, true);
            friendlyGesturesList.headerHeight = 0;
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
                EditorGUI.LabelField(rect, string.Format("{0} @ {1}", gesture.GetType().Name, gesture.name), GUIElements.BoxLabelStyle);
            };
            friendlyGesturesList.onRemoveCallback += list => { indexToRemove = list.index; };
        }

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfDirtyOrScript();

            EditorGUI.BeginChangeCheck();
            var expanded = GUIElements.BeginFoldout(advanced.isExpanded, TEXT_ADVANCED_HEADER);
            if (EditorGUI.EndChangeCheck())
            {
                advanced.isExpanded = expanded;
            }
            if (expanded)
            {
                GUILayout.BeginVertical(GUIElements.FoldoutStyle);
                drawAdvanced();
                GUILayout.EndVertical();
            }
            GUIElements.EndFoldout();

            serializedObject.ApplyModifiedProperties();
        }

        protected virtual void drawDebug()
        {
            if (debugMode == null) return;
            EditorGUILayout.PropertyField(debugMode, DEBUG_MODE);
        }

        protected virtual void drawSendMessage()
        {
            EditorGUILayout.PropertyField(useSendMessage, USE_SEND_MESSAGE);
            if (useSendMessage.boolValue)
            {
                EditorGUIUtility.labelWidth = 70;

                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(GUIContent.none, GUILayout.Width(10));
                EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
                EditorGUILayout.PropertyField(sendMessageTarget, SEND_MESSAGE_TARGET);
                EditorGUILayout.PropertyField(sendStateChangeMessages, SEND_STATE_CHANGE_MESSAGES);
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
            }
        }

        protected virtual void drawCombineTouches()
        {
            if (shouldDrawCombineTouches)
            {
                EditorGUILayout.PropertyField(combineTouches, COMBINE_TOUCH_POINTS);
                if (combineTouches.boolValue)
                {
                    EditorGUIUtility.labelWidth = 160;
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label(GUIContent.none, GUILayout.Width(10));
                    EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
                    EditorGUILayout.PropertyField(combineTouchesInterval, COMBINE_TOUCH_POINTS_INTERVAL);
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();
                }
            }
        }

        protected virtual void drawLimitTouches()
        {
            var limitTouches = (minTouches.intValue > 0) || (maxTouches.intValue > 0);
            var newLimitTouches = EditorGUILayout.ToggleLeft(LIMIT_TOUCHES, limitTouches);
            if (newLimitTouches)
            {
                if (!limitTouches)
                {
                    minTouchesFloat = 0;
                    maxTouchesFloat = 10;
                }
                EditorGUI.indentLevel++;
                EditorGUILayout.LabelField("Min: " + (int)minTouchesFloat + ", Max: " + (int)maxTouchesFloat);
                EditorGUILayout.MinMaxSlider(ref minTouchesFloat, ref maxTouchesFloat, 0, 10);
                EditorGUI.indentLevel--;
            }
            else
            {
                if (limitTouches)
                {
                    minTouchesFloat = 0;
                    maxTouchesFloat = 0;
                }
            }

            minTouches.intValue = (int)minTouchesFloat;
            maxTouches.intValue = (int)maxTouchesFloat;
        }

        protected virtual void drawRequireToFail()
        {
            EditorGUILayout.PropertyField(requireGestureToFail, REQUIRE_GESTURE_TO_FAIL);
        }

        protected virtual void drawAdvanced()
        {
            drawLimitTouches();
            drawCombineTouches();
            drawSendMessage();
            drawRequireToFail();
            drawDebug();
            drawFriendlyGestures();
        }

        protected virtual void drawFriendlyGestures()
        {
            EditorGUI.BeginChangeCheck();
            var expanded = GUIElements.BeginFoldout(friendlyGestures.isExpanded, new GUIContent(string.Format("Friendly gestures ({0})", friendlyGestures.arraySize), TEXT_FRIENDLY_HEADER));
            if (EditorGUI.EndChangeCheck())
            {
                friendlyGestures.isExpanded = expanded;
            }
            if (expanded)
            {
                GUILayout.BeginVertical(GUIElements.FoldoutStyle);
                drawGestureList(friendlyGestures, addFriendlyGesture);
                GUILayout.EndVertical();
            }
            GUIElements.EndFoldout();
        }

        #region Gesture List

        private void drawGestureList(SerializedProperty prop, Action<SerializedProperty, Gesture> addGesture)
        {
            indexToRemove = -1;
            friendlyGesturesList.DoLayoutList();

            GUILayout.Space(9);

            Rect dropArea = GUILayoutUtility.GetRect(0.0f, 50.0f, GUIElements.BoxStyle,
                GUILayout.ExpandWidth(true));
            GUI.Box(dropArea, "Drag a Gesture Here", GUIElements.BoxStyle);
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
