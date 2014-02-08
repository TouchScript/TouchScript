/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using TouchScript.Editor.Utils;
using TouchScript.Gestures;
using UnityEditor;
using UnityEngine;

namespace TouchScript.Editor.Gestures
{
    [CustomEditor(typeof(Gesture), true)]
    public class GestureEditor : UnityEditor.Editor
    {
        public const string TEXT_FRIENDLY_HEADER = "Gestures which can work together with this gesture.";
        public const string TEXT_USESENDMESSAGE = "If you use UnityScript or prefer using Unity Messages you can turn them on with this option.";
        public const string TEXT_SENDMESSAGETARGET = "The GameObject target of Unity Messages. If null, host GameObject is used.";
        public const string TEXT_SENDSTATECHANGEMESSAGES = "If checked, the gesture will send a message for every state change. Gestures usually have their own more specific messages, so you should keep this toggle unchecked unless you really want state change messages.";
        public const string TEXT_COMBINETOUCHPOINTSINTERVAL = "When several fingers are used to perform a tap, touch points released not earlier than <CombineInterval> seconds ago are used to calculate gesture's final screen position.";

        private const string FRIENDLY_GESTURES_PROP = "friendlyGestures";

        protected bool shouldDrawCombineTouchPoints = false;

        private Gesture gestureInstance;
        private SerializedProperty serializedGestures;
        private SerializedProperty combineTouchPoints;
        private SerializedProperty combineTouchPointsInterval;
        private bool shouldRecognizeShown;

        protected virtual void OnEnable()
        {
            hideFlags = HideFlags.HideAndDontSave;
            gestureInstance = target as Gesture;
            serializedGestures = serializedObject.FindProperty("friendlyGestures");
            combineTouchPoints = serializedObject.FindProperty("combineTouchPoints");
            combineTouchPointsInterval = serializedObject.FindProperty("combineTouchPointsInterval");
        }

        public override void OnInspectorGUI()
        {
            EditorGUIUtility.labelWidth = 160;

            EditorGUI.BeginChangeCheck();
            var useSendMessage = GUILayout.Toggle(gestureInstance.UseSendMessage, new GUIContent("Use SendMessage", TEXT_USESENDMESSAGE));
            var sTarget = gestureInstance.SendMessageTarget;
            var sendStateChangeMessages = gestureInstance.SendStateChangeMessages;
            if (useSendMessage)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(GUIContent.none, GUILayout.Width(30));
                EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
                sTarget = EditorGUILayout.ObjectField(new GUIContent("SendMessage Target", TEXT_SENDMESSAGETARGET), sTarget, typeof(GameObject), true) as GameObject;
                sendStateChangeMessages = GUILayout.Toggle(gestureInstance.SendStateChangeMessages, new GUIContent("Send State Change Messages", TEXT_SENDSTATECHANGEMESSAGES));
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
            }
            if (EditorGUI.EndChangeCheck())
            {
                gestureInstance.UseSendMessage = useSendMessage;
                gestureInstance.SendMessageTarget = sTarget;
                gestureInstance.SendStateChangeMessages = sendStateChangeMessages;
                EditorUtility.SetDirty(gestureInstance);
            }

            if (shouldDrawCombineTouchPoints)
            {
                combineTouchPoints.boolValue = GUILayout.Toggle(combineTouchPoints.boolValue, new GUIContent("Combine Touch Points", TEXT_COMBINETOUCHPOINTSINTERVAL));
                if (combineTouchPoints.boolValue)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label(GUIContent.none, GUILayout.Width(30));
                    EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
                    EditorGUILayout.PropertyField(combineTouchPointsInterval, new GUIContent("Combine Interval (sec)", TEXT_COMBINETOUCHPOINTSINTERVAL));
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();
                }
            }

            shouldRecognizeShown =
                GUIElements.Foldout(shouldRecognizeShown, new GUIContent(string.Format("Friendly gestures ({0})", serializedGestures.arraySize), TEXT_FRIENDLY_HEADER),
                    () =>
                    {
                        int gestureIndexToRemove = -1;
                        int gesturesDrawn = 0;

                        serializedObject.UpdateIfDirtyOrScript();

                        GUILayout.BeginVertical();
                        for (int i = 0; i < serializedGestures.arraySize; i++)
                        {
                            var gesture = serializedGestures.GetArrayElementAtIndex(i).objectReferenceValue as Gesture;

                            if (gesture == null)
                            {
                                gestureIndexToRemove = i;
                            } else
                            {
                                Rect rect = EditorGUILayout.BeginHorizontal(GUIElements.BoxStyle, GUILayout.Height(23));
                                EditorGUILayout.LabelField(
                                    string.Format("{0} @ {1}", gesture.GetType().Name, gesture.name),
                                    GUIElements.BoxLabelStyle, GUILayout.ExpandWidth(true));
                                if (GUILayout.Button("remove", GUILayout.Width(60), GUILayout.Height(16)))
                                {
                                    gestureIndexToRemove = i;
                                } else if (Event.current.type == EventType.MouseDown &&
                                           rect.Contains(Event.current.mousePosition))
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

                                    foreach (Object obj in DragAndDrop.objectReferences)
                                    {
                                        if (obj is GameObject)
                                        {
                                            var go = obj as GameObject;
                                            Gesture[] gestures = go.GetComponents<Gesture>();
                                            foreach (Gesture gesture in gestures)
                                            {
                                                addGesture(gesture);
                                            }
                                        } else if (obj is Gesture)
                                        {
                                            addGesture(obj as Gesture);
                                        }
                                    }

                                    Event.current.Use();
                                }
                                break;
                        }

                        if (gestureIndexToRemove > -1)
                        {
                            removeGestureAt(gestureIndexToRemove);
                        }
                    });

            serializedObject.ApplyModifiedProperties();
        }

        private void addGesture(Gesture value)
        {
            if (value == null || value == target) return;

            for (int i = 0; i < serializedGestures.arraySize; i++)
            {
                if (serializedGestures.GetArrayElementAtIndex(i).objectReferenceValue == value) return;
            }

            serializedGestures.arraySize++;
            serializedGestures.GetArrayElementAtIndex(serializedGestures.arraySize - 1).objectReferenceValue = value;

            var so = new SerializedObject(value);
            so.Update();
            SerializedProperty prop = so.FindProperty(FRIENDLY_GESTURES_PROP);
            prop.arraySize++;
            prop.GetArrayElementAtIndex(prop.arraySize - 1).objectReferenceValue = target;

            serializedObject.ApplyModifiedProperties();
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(value);
        }

        private void removeGestureAt(int index)
        {
            Object gesture = serializedGestures.GetArrayElementAtIndex(index).objectReferenceValue;
            // I don't know why this doesn't work
            //serializedGestures.DeleteArrayElementAtIndex(index);
            removeFromArray(serializedGestures, index);

            if (gesture != null)
            {
                var so = new SerializedObject(gesture);
                so.Update();
                SerializedProperty prop = so.FindProperty(FRIENDLY_GESTURES_PROP);
                for (int j = 0; j < prop.arraySize; j++)
                {
                    if (prop.GetArrayElementAtIndex(j).objectReferenceValue == target)
                    {
                        // I don't know why this doesn't work
                        //prop.DeleteArrayElementAtIndex(j);
                        removeFromArray(prop, j);
                        break;
                    }
                }

                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(gesture);
            }
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
    }
}