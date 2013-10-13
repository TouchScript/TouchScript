/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using TouchScript.Gestures;
using UnityEditor;
using UnityEngine;

namespace TouchScript.Editor.Gestures
{
    [CustomEditor(typeof(Gesture))]
    public class GestureEditor : UnityEditor.Editor
    {

        private SerializedProperty serializedGestures;
        private GUIStyle boxStyle;
        private bool shouldRecognizeFolded = true;

        protected virtual void OnEnable()
        {
            serializedGestures = serializedObject.FindProperty("shouldRecognizeWith");
        }

        public override void OnInspectorGUI()
        {
            EditorGUIUtility.LookLikeInspector();

            if (boxStyle == null)
            {
                boxStyle = new GUIStyle(GUI.skin.box);
                boxStyle.margin = new RectOffset(10, 10, 10, 10);
                boxStyle.normal.textColor = GUI.skin.label.normal.textColor;
                boxStyle.alignment = TextAnchor.MiddleCenter;
            }

            GUILayout.BeginVertical("ShurikenEffectBg", GUILayout.MinHeight(20.0f));
            var rect = GUILayoutUtility.GetRect(0, 15f);
            var style = "ShurikenModuleTitle";

            //Rect position = EditorGUILayout.BeginVertical();
            //position.y -= 4f;
            //position.height += 4f;
            //GUI.Label(position, GUIContent.none, "ShurikenModuleBg");
            ////moduleUI.OnInspectorGUI(this.m_ParticleSystem);
            //EditorGUILayout.LabelField("Bla");
            //EditorGUILayout.EndVertical();

            shouldRecognizeFolded = EditorGUILayout.Toggle("Should Recognize with", shouldRecognizeFolded, "ShurikenModuleTitle", GUILayout.ExpandWidth(true));

            if (shouldRecognizeFolded)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.BeginVertical();
                for (var i = 0; i < serializedGestures.arraySize; i++)
                {
                    var item = serializedGestures.GetArrayElementAtIndex(i);
                    var gesture = EditorUtility.InstanceIDToObject(item.intValue) as Gesture;
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(string.Format("{0} @ {1}", gesture.GetType().Name, gesture.name), GUILayout.ExpandWidth(true));
                    GUILayout.Button("remove", GUILayout.Width(60), GUILayout.Height(16));
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndVertical();

                Rect dropArea = GUILayoutUtility.GetRect(0.0f, 50.0f, boxStyle, GUILayout.ExpandWidth(true));
                GUI.Box(dropArea, "Drag Gesture Here", boxStyle);
                switch (Event.current.type)
                {
                    case EventType.DragUpdated:
                    case EventType.DragPerform:
                        if (!dropArea.Contains(Event.current.mousePosition))
                            return;

                        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                        if (Event.current.type == EventType.DragPerform)
                        {
                            DragAndDrop.AcceptDrag();

                            foreach (var obj in DragAndDrop.objectReferences)
                            {
                                if (obj is GameObject)
                                {
                                    var go = obj as GameObject;
                                    var gestures = go.GetComponents<Gesture>();
                                    foreach (var gesture in gestures)
                                    {
                                        addGesture(gesture);
                                    }
                                } else if (obj is Gesture)
                                {
                                    addGesture(obj as Gesture);
                                }
                            }
                        }
                        break;
                }
            }

            GUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();
        }

        private void addGesture(Gesture value)
        {
            Debug.Log(string.Format("Adding {0}", value));

            for (var i = 0; i < serializedGestures.arraySize; i++)
            {
                var item = serializedGestures.GetArrayElementAtIndex(i);
                Debug.Log(item.intValue);
                if (item.intValue == value.GetInstanceID())
                {
                    Debug.Log("Got this gesture already");
                    return;
                }
            }

            serializedGestures.arraySize++;
            serializedGestures.GetArrayElementAtIndex(serializedGestures.arraySize - 1).intValue = value.GetInstanceID();
        }

    }
}