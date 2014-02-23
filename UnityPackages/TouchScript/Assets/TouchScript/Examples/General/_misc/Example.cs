using UnityEngine;

[ExecuteInEditMode]
public class Example : MonoBehaviour {

    [Multiline(16)]
    public string Text;

    private GUIStyle style;
    private Color color = new Color(0, 0, 0, .9f);

    private void OnGUI()
    {
        if (style == null)
        {
            style = new GUIStyle(GUI.skin.label);
            style.normal.background = new Texture2D(1, 1);
            style.normal.background.SetPixel(0, 0, Color.black);
            style.padding = new RectOffset(5, 5, 5, 5);
        }

        var c = GUI.backgroundColor;
        GUI.backgroundColor = color;
        var rect = GUI.skin.label.CalcSize(new GUIContent(Text));
        GUILayout.BeginArea(new Rect(50, Screen.height - rect.y - 50, rect.x + 20, rect.y + 20));
        GUILayout.Label(Text, style);
        GUILayout.EndArea();
        GUI.backgroundColor = c;
    }

}
