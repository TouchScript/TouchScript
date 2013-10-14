/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using TouchScript.Editor.Utils;
using TouchScript.Gestures;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TouchScript.Editor.Gestures
{
    [CustomEditor(typeof(Gesture))]
    public class GestureEditor : UnityEditor.Editor
    {
        public const string TEXT_FRIENDLY_HEADER = "Gestures which can work together with this gesture.";

        private const string FRIENDLY_GESTURES_PROPERTY_NAME = "friendlyGestures";

        private SerializedProperty serializedGestures;
        private bool shouldRecognizeShown;

        protected virtual void OnEnable()
        {
            hideFlags = HideFlags.HideAndDontSave;
            serializedGestures = serializedObject.FindProperty(FRIENDLY_GESTURES_PROPERTY_NAME);
        }

        public override void OnInspectorGUI()
        {
            shouldRecognizeShown =
                GUIElements.Foldout(shouldRecognizeShown, new GUIContent("Friendly gestures", TEXT_FRIENDLY_HEADER),
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
                                                //Debug.Log(string.Format("Gesture at {0} on {1} is null!!", i, target));
                                                gestureIndexToRemove = i;
                                            } else
                                            {
                                                Rect rect = EditorGUILayout.BeginHorizontal(GUIElements.BoxStyle, GUILayout.Height(23));
                                                EditorGUILayout.LabelField(string.Format("{0} @ {1}", gesture.GetType().Name, gesture.name), GUIElements.BoxLabelStyle, GUILayout.ExpandWidth(true));
                                                if (GUILayout.Button("remove", GUILayout.Width(60), GUILayout.Height(16)))
                                                {
                                                    gestureIndexToRemove = i;
                                                } else if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
                                                {
                                                    EditorGUIUtility.PingObject(gesture);
                                                }
                                                EditorGUILayout.EndHorizontal();
                                                gesturesDrawn++;
                                            }
                                        }
                                        GUILayout.EndVertical();
                                        if (gesturesDrawn > 0) GUILayout.Space(9);

                                        Rect dropArea = GUILayoutUtility.GetRect(0.0f, 50.0f, GUIElements.BoxStyle, GUILayout.ExpandWidth(true));
                                        GUI.Box(dropArea, "Drag a Gesture Here", GUIElements.BoxStyle);
                                        switch (Event.current.type)
                                        {
                                            case EventType.DragUpdated:
                                                if (dropArea.Contains(Event.current.mousePosition)) DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
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
            var prop = so.FindProperty(FRIENDLY_GESTURES_PROPERTY_NAME);
            prop.arraySize++;
            prop.GetArrayElementAtIndex(prop.arraySize - 1).objectReferenceValue = target;

            serializedObject.ApplyModifiedProperties();
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(value);
        }

        private void removeGestureAt(int index)
        {
            var gesture = serializedGestures.GetArrayElementAtIndex(index).objectReferenceValue;
            // I don't know why this doesn't work
            //serializedGestures.DeleteArrayElementAtIndex(index);
            removeFromArray(serializedGestures, index);

            if (gesture != null)
            {
                var so = new SerializedObject(gesture);
                so.Update();
                var prop = so.FindProperty(FRIENDLY_GESTURES_PROPERTY_NAME);
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
                array.GetArrayElementAtIndex(index).objectReferenceValue = array.GetArrayElementAtIndex(array.arraySize - 1).objectReferenceValue;
            }
            array.arraySize--;
        }
    }
}