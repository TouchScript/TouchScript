using UnityEngine;

public class HitTestScene : MonoBehaviour {

    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(50, 50, 700, 300));
        GUILayout.Label("Scene to illustrate different object filtering techniques: \n" +
                        "1. 'Untouchable Discarding Behavior' — doesn't let other colliders behind the red cube get touch points.\n" +
                        "2. 'Untouchable Behavior' — doesn't recieve touch input and lets other colliders behind the red cube get touch points.\n" +
                        "3. 'Untouchable Layer' — the whole layer is ignored during hit test calculations.");
        GUILayout.EndArea();
    }

}
