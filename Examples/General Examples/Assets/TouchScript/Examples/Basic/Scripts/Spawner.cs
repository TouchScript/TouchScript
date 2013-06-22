using TouchScript.Gestures;
using UnityEngine;
using System.Collections;

public class Spawner : MonoBehaviour
{
    public Transform CubePrefab;
    public Transform Container;
    public float Scale = .5f;

    private void Start()
    {
        GetComponent<TapGesture>().StateChanged += HandleStateChanged;
    }

    private void HandleStateChanged(object sender, TouchScript.Events.GestureStateChangeEventArgs e)
    {
        if (e.State == Gesture.GestureState.Recognized)
        {
            var gesture = sender as TapGesture;
            RaycastHit hit;
            gesture.GetTargetHitResult(out hit);

            Color color = new Color(Random.value, Random.value, Random.value);
            var c = Instantiate(CubePrefab) as Transform;
            c.parent = Container;
            c.name = "Cube";
            c.localScale = Vector3.one*Scale*c.localScale.x;
            c.position = hit.point + hit.normal * 2;
            c.renderer.material.color = color;

        }
    }
}