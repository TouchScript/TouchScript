using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ScaleformKey))]
public class ScaleformKeyEditor : Editor
{

    private ScaleformKey instance;
    private SerializedProperty key;

    private void OnEnable()
    {
        instance = target as ScaleformKey;
        key = serializedObject.FindProperty("Key");
    }

    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("Set Key"))
        {
            var window = ScriptableObject.CreateInstance<KeyWindow>();
            window.Property = key;
            window.ShowAuxWindow();
        }
    }

}
