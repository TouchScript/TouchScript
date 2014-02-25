using TouchScript.Gestures;
using TouchScript.Hit;
using UnityEngine;

public class BasicExample_Spawner : MonoBehaviour
{
    public Transform CubePrefab;
    public Transform Container;
    public float Scale = .5f;

    private void OnEnable()
    {
        GetComponent<TapGesture>().StateChanged += tapStateChangedHandler;
    }

    private void OnDisable()
    {
        GetComponent<TapGesture>().StateChanged -= tapStateChangedHandler;
    }

    private void tapStateChangedHandler(object sender, TouchScript.Events.GestureStateChangeEventArgs e)
    {
        if (e.State == Gesture.GestureState.Recognized)
        {
            var gesture = sender as TapGesture;
            ITouchHit hit;
            gesture.GetTargetHitResult(out hit);
            var hit3d = hit as ITouchHit3D;
            if (hit3d == null) return;

            Color color = new Color(Random.value, Random.value, Random.value);
            var c = Instantiate(CubePrefab) as Transform;
            c.parent = Container;
            c.name = "Cube";
            c.localScale = Vector3.one*Scale*c.localScale.x;
            c.position = hit3d.Point + hit3d.Normal * 2;
            c.renderer.material.color = color;

        }
    }
}