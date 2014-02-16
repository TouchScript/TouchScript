/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using TouchScript.Editor.Utils;
using TouchScript.Gestures;
using UnityEditor;
using UnityEngine;

namespace TouchScript.Editor.Gestures
{
    [CustomEditor(typeof(Gesture), true)]
    public class GestureEditor : UnityEditor.Editor
    {
        private const string TEXT_FRIENDLY_HEADER = "List gestures which can work together with this gesture.";
        private const string FRIENDLY_GESTURES_PROP = "friendlyGestures";

        private static readonly GUIContent USE_SEND_MESSAGE = new GUIContent("Use SendMessage", "If you use UnityScript or prefer using Unity Messages you can turn them on with this option.");
        private static readonly GUIContent SEND_MESSAGE_TARGET = new GUIContent("Target", "The GameObject target of Unity Messages. If null, host GameObject is used.");
        private static readonly GUIContent SEND_STATE_CHANGE_MESSAGES = new GUIContent("Send State Change Messages", "If checked, the gesture will send a message for every state change. Gestures usually have their own more specific messages, so you should keep this toggle unchecked unless you really want state change messages.");
        private static readonly GUIContent COMBINE_TOUCH_POINTS = new GUIContent("Combine Touch Points", "When several fingers are used to perform a tap, touch points released not earlier than <CombineInterval> seconds ago are used to calculate gesture's final screen position.");
        private static readonly GUIContent COMBINE_TOUCH_POINTS_INTERVAL = new GUIContent("Combine Interval (sec)", COMBINE_TOUCH_POINTS.tooltip);
        private static readonly GUIContent REQUIRE_GESTURE_TO_FAIL = new GUIContent("Require Other Gesture to Fail", "Gesture which must fail for this gesture to start.");

        protected bool shouldDrawCombineTouchPoints = false;

        private Gesture gestureInstance;
        private SerializedProperty friendlyGestures;
        private SerializedProperty requireGestureToFail;
        private SerializedProperty combineTouchPoints, combineTouchPointsInterval;
        private SerializedProperty useSendMessage, sendMessageTarget, sendStateChangeMessages;

        private bool friendlyShown, requireToFailShown, requireToFailChecked;

        protected virtual void OnEnable()
        {
            hideFlags = HideFlags.HideAndDontSave;
            gestureInstance = target as Gesture;

            friendlyGestures = serializedObject.FindProperty("friendlyGestures");
            requireGestureToFail = serializedObject.FindProperty("requireGestureToFail");

            combineTouchPoints = serializedObject.FindProperty("combineTouchPoints");
            combineTouchPointsInterval = serializedObject.FindProperty("combineTouchPointsInterval");

            useSendMessage = serializedObject.FindProperty("useSendMessage");
            sendMessageTarget = serializedObject.FindProperty("sendMessageTarget");
            sendStateChangeMessages = serializedObject.FindProperty("sendStateChangeMessages");

            requireToFailChecked = requireGestureToFail.objectReferenceValue != null;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfDirtyOrScript();

            drawSendMessage();
            drawCombineTouchPoints();
            drawRequireToFail();
            drawFriendlyGestures();

            serializedObject.ApplyModifiedProperties();
        }

        private void drawSendMessage()
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

        private void drawCombineTouchPoints()
        {
            if (shouldDrawCombineTouchPoints)
            {
                EditorGUILayout.PropertyField(combineTouchPoints, COMBINE_TOUCH_POINTS);
                if (combineTouchPoints.boolValue)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label(GUIContent.none, GUILayout.Width(30));
                    EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
                    EditorGUILayout.PropertyField(combineTouchPointsInterval, COMBINE_TOUCH_POINTS_INTERVAL);
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();
                }
            }
        }

        private void drawRequireToFail()
        {
            EditorGUILayout.PropertyField(requireGestureToFail, REQUIRE_GESTURE_TO_FAIL);
        }

        private void drawFriendlyGestures()
        {
            EditorGUI.BeginChangeCheck();
            var _friendlyGestures_expanded = GUIElements.BeginFoldout(friendlyGestures.isExpanded, new GUIContent(string.Format("Friendly gestures ({0})", friendlyGestures.arraySize), TEXT_FRIENDLY_HEADER));
            if (EditorGUI.EndChangeCheck())
            {
                friendlyGestures.isExpanded = _friendlyGestures_expanded;
            }
            if (_friendlyGestures_expanded)
            {
                GUILayout.BeginVertical(GUIElements.FoldoutStyle);
                drawGestureList(friendlyGestures, addFriendlyGesture, removeFriendlyGestureAt);
                GUILayout.EndVertical();
            }
            GUIElements.EndFoldout();
        }

        #region Gesture List

        private void drawGestureList(SerializedProperty prop, Action<SerializedProperty, Gesture> addGesture, Func<SerializedProperty, int, Gesture> removeGestureAt)
        {
            int gestureIndexToRemove = -1;
            int gesturesDrawn = 0;

            GUILayout.BeginVertical();
            for (int i = 0; i < prop.arraySize; i++)
            {
                var gesture = prop.GetArrayElementAtIndex(i).objectReferenceValue as Gesture;

                if (gesture == null)
                {
                    gestureIndexToRemove = i;
                }
                else
                {
                    Rect rect = EditorGUILayout.BeginHorizontal(GUIElements.BoxStyle, GUILayout.Height(23));
                    EditorGUILayout.LabelField(
                        string.Format("{0} @ {1}", gesture.GetType().Name, gesture.name),
                        GUIElements.BoxLabelStyle, GUILayout.ExpandWidth(true));
                    if (GUILayout.Button("remove", GUILayout.Width(60), GUILayout.Height(16)))
                    {
                        gestureIndexToRemove = i;
                    }
                    else if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
                    {
                        EditorGUIUtility.PingObject(gesture);
                    }
                    EditorGUILayout.EndHorizontal();
                    gesturesDrawn++;
                }
            }
            GUILayout.EndVertical();
            if (gesturesDrawn > 0) GUILayout.Space(9);

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

            if (gestureIndexToRemove > -1)
            {
                removeGestureAt(prop, gestureIndexToRemove);
            }
        }

        private void addGesture(SerializedProperty prop, Gesture value)
        {
            if (value == null || value == target) return;

            for (int i = 0; i < prop.arraySize; i++)
            {
                if (prop.GetArrayElementAtIndex(i).objectReferenceValue == value) return;
            }

            prop.arraySize++;
            prop.GetArrayElementAtIndex(friendlyGestures.arraySize - 1).objectReferenceValue = value;
        }

        private Gesture removeGestureAt(SerializedProperty prop, int index)
        {
            Gesture gesture = prop.GetArrayElementAtIndex(index).objectReferenceValue as Gesture;
            // I don't know why this doesn't work
            //friendlyGestures.DeleteArrayElementAtIndex(index);
            removeFromArray(prop, index);

            return gesture;
        }

        private void addFriendlyGesture(SerializedProperty prop, Gesture value)
        {
            if (value == null || value == target) return;

            addGesture(prop, value);

            var so = new SerializedObject(value);
            so.Update();
            SerializedProperty p = so.FindProperty(FRIENDLY_GESTURES_PROP);
            p.arraySize++;
            p.GetArrayElementAtIndex(p.arraySize - 1).objectReferenceValue = target;

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(value);
        }

        private Gesture removeFriendlyGestureAt(SerializedProperty prop, int index)
        {
            Gesture gesture = removeGestureAt(prop, index);

            if (gesture != null)
            {
                var so = new SerializedObject(gesture);
                so.Update();
                SerializedProperty p = so.FindProperty(FRIENDLY_GESTURES_PROP);
                for (int j = 0; j < p.arraySize; j++)
                {
                    if (p.GetArrayElementAtIndex(j).objectReferenceValue == target)
                    {
                        // I don't know why this doesn't work
                        //prop.DeleteArrayElementAtIndex(j);
                        removeFromArray(p, j);
                        break;
                    }
                }

                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(gesture);
            }

            return gesture;
        }

        // A hack to remove a gesture from a list
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