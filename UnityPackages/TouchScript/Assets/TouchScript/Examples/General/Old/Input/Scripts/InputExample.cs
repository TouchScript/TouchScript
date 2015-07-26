using TouchScript;
using UnityEngine;

public class InputExample : MonoBehaviour
{
    public GameObject Prefab;

    private void OnEnable()
    {
        if (TouchManager.Instance != null)
        {
            TouchManager.Instance.TouchesBegan += touchBeganHandler;
        }
    }

    private void OnDisable()
    {
        if (TouchManager.Instance != null)
        {
            TouchManager.Instance.TouchesBegan -= touchBeganHandler;
        }
    }

    private void spawnPrefabAt(Vector2 position)
    {
        var obj = Instantiate(Prefab) as GameObject;
        obj.transform.position = Camera.main.ScreenToWorldPoint(new Vector3(position.x, position.y, 10));
    }

    private void touchBeganHandler(object sender, TouchEventArgs e)
    {
        foreach (var point in e.Touches)
        {
            spawnPrefabAt(point.Position);
        }
    }
}