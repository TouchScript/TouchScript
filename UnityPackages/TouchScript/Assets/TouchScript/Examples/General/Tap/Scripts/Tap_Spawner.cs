using System;
using TouchScript.Gestures;
using TouchScript.Hit;
using UnityEngine;
using Random = UnityEngine.Random;

public class Tap_Spawner : MonoBehaviour
{
    public Transform CubePrefab;
    public Transform Container;
    public float Scale = .5f;

    private void OnEnable()
    {
        GetComponent<TapGesture>().Tapped += tappedHandler;
    }

    private void OnDisable()
    {
        GetComponent<TapGesture>().Tapped -= tappedHandler;
    }

    private void tappedHandler(object sender, EventArgs e)
    {
        var gesture = sender as TapGesture;
        TouchHit hit;
        gesture.GetTargetHitResult(out hit);

        Color color = new Color(Random.value, Random.value, Random.value);
        var cube = Instantiate(CubePrefab) as Transform;
        cube.parent = Container;
        cube.name = "Cube";
        cube.localScale = Vector3.one*Scale*cube.localScale.x;
        cube.position = hit.Point + hit.Normal*.5f;
        cube.GetComponent<Renderer>().material.color = color;
    }
}