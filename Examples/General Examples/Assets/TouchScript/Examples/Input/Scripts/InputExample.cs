using System;
using TouchScript;
using TouchScript.Events;
using UnityEngine;

public class InputExample : MonoBehaviour
{
    public GameObject Prefab;

    private void Start()
    {
        TouchManager.Instance.TouchesBegan += touchBeganHandler;
    }

    private void spawnPrefabAt(Vector2 position)
    {
        var obj = Instantiate(Prefab) as GameObject;
        obj.transform.position = Camera.main.ScreenToWorldPoint(new Vector3(position.x, position.y, 10));
    }

    private void touchBeganHandler(object sender, TouchEventArgs e)
    {
        foreach (var point in e.TouchPoints)
        {
            spawnPrefabAt(point.Position);
        }
    }
}