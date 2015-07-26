using System;
using TouchScript.Gestures;
using UnityEngine;
using Random = UnityEngine.Random;

public class DoubleTap_Sprite : MonoBehaviour
{
    private static Color[] COLORS = new[] {Color.yellow, Color.red, Color.magenta, Color.green, Color.cyan, Color.blue};

    private void OnEnable()
    {
        foreach (var tap in GetComponents<TapGesture>())
        {
            tap.Tapped += tappedHandler;
        }
    }

    private void OnDisable()
    {
        foreach (var tap in GetComponents<TapGesture>())
        {
            tap.Tapped -= tappedHandler;
        }
    }

    private void changeColor()
    {
        Color newColor = COLORS[Random.Range(0, COLORS.Length)];
        while (newColor == GetComponent<Renderer>().material.color) newColor = COLORS[Random.Range(0, COLORS.Length)];

        GetComponent<Renderer>().material.color = newColor;
    }

    private void breakObject()
    {
        for (var i = 0; i < 3; i++)
        {
            var angle = Quaternion.Euler(0, 0, 360/3*i);
            var rb = createCopy(transform.localPosition + angle*Vector3.right*transform.localScale.x*1.3f);
            rb.mass = GetComponent<Rigidbody2D>().mass/3;
            rb.AddForce(angle*Vector2.right*500*rb.mass);
        }
        Destroy(gameObject);
    }

    private Rigidbody2D createCopy(Vector3 position)
    {
        var obj = Instantiate(gameObject) as GameObject;
        obj.name = "Logo";
        var t = obj.transform;
        t.parent = transform.parent;
        t.position = position;
        t.eulerAngles = new Vector3(0, 0, Random.Range(0, 360));
        t.localScale = Vector3.one*transform.localScale.x*.6f;
        return obj.GetComponent<Rigidbody2D>();
    }

    private void tappedHandler(object sender, EventArgs eventArgs)
    {
        var tap = sender as TapGesture;
        switch (tap.NumberOfTapsRequired)
        {
            case 1:
                // our single tap gesture
                changeColor();
                break;
            case 2:
                // our double tap gesture
                breakObject();
                break;
        }
    }
}