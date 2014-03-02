using UnityEditor;
using UnityEngine;

public class KeyWindow : EditorWindow
{

    public SerializedProperty Property;

    private string key = "";

    private void OnEnable()
    {
        title = "Set New Key";
    }

    public void OnGUI()
    {
        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField("Set New Key:");
        key = EditorGUILayout.TextField(key);
        if (GUILayout.Button("Set Key"))
        {
            Property.FindPropertyRelative("Key").stringValue = key;
            Property.serializedObject.ApplyModifiedProperties();
            Close();
        }
        EditorGUILayout.EndVertical();
    }
}