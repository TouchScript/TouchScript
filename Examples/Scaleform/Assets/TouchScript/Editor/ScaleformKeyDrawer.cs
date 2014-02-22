using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ScaleformKey))]
public class ScaleformKeyDrawer : PropertyDrawer {
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return 16;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (GUI.Button(position, "Set Key"))
        {
            var window = ScriptableObject.CreateInstance<KeyWindow>();
            window.Property = property;
            //window.position = new Rect(Screen.width / 2, Screen.height / 2, 250, 50);
            window.ShowAuxWindow();
        }
    }
}


