using System;
using UnityEngine;
using TouchScript.Gestures;
using Random = UnityEngine.Random;

public class Tap_Breaker : MonoBehaviour
{
    public float Power = 10.0f;

    private Vector3[] directions =
    {
        new Vector3(1, -1, 1),
        new Vector3(-1, -1, 1),
        new Vector3(-1, -1, -1),
        new Vector3(1, -1, -1),
        new Vector3(1, 1, 1),
        new Vector3(-1, 1, 1),
        new Vector3(-1, 1, -1),
        new Vector3(1, 1, -1)
    };

    private void OnEnable()
    {
        // subscribe to gesture's Tapped event
        GetComponent<TapGesture>().Tapped += tappedHandler;
    }

    private void OnDisable()
    {
        // don't forget to unsubscribe
        GetComponent<TapGesture>().Tapped -= tappedHandler;
    }

    private void tappedHandler(object sender, EventArgs e)
    {
        // if we are not too small
        if (transform.localScale.x > 0.05f)
        {
            Color color = new Color(Random.value, Random.value, Random.value);
            // break this cube into 8 parts
            for (int i = 0; i < 8; i++)
            {
                var obj = Instantiate(gameObject) as GameObject;
                var cube = obj.transform;
                cube.parent = transform.parent;
                cube.name = "Cube";
                cube.localScale = 0.5f * transform.localScale;
                cube.position = transform.TransformPoint(directions[i] / 4);
                cube.GetComponent<Rigidbody>().AddForce(Power * Random.insideUnitSphere, ForceMode.VelocityChange);
                cube.GetComponent<Renderer>().material.color = color;
            }
            Destroy(gameObject);
        }
    }
}