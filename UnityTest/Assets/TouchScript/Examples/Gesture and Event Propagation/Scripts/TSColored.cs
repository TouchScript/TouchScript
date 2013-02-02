using TouchScript.Gestures;
using UnityEngine;

public class TSColored : MonoBehaviour {
    private void Start() {
        var tap = GetComponent<TapGesture>();
        if (tap == null) return;
        tap.StateChanged += (sender, args) => { if (args.State == Gesture.GestureState.Recognized) renderer.material.color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f)); };
    }
}