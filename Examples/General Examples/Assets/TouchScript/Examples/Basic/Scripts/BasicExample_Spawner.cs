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
            TouchHit hit;
            gesture.GetTargetHitResult(out hit);

            Color color = new Color(Random.value, Random.value, Random.value);
            var c = Instantiate(CubePrefab) as Transform;
            c.parent = Container;
            c.name = "Cube";
            c.localScale = Vector3.one*Scale*c.localScale.x;
            c.position = hit.Point + hit.Normal * 2;
            c.renderer.material.color = color;

        }
    }
}