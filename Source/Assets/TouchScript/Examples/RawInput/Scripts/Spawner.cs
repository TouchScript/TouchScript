/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using UnityEngine;

namespace TouchScript.Examples.RawInput
{
    public class Spawner : MonoBehaviour
    {
        public GameObject Prefab;

        private void OnEnable()
        {
            if (TouchManager.Instance != null)
            {
                TouchManager.Instance.TouchesBegan += touchesBeganHandler;
            }
        }

        private void OnDisable()
        {
            if (TouchManager.Instance != null)
            {
                TouchManager.Instance.TouchesBegan -= touchesBeganHandler;
            }
        }

        private void spawnPrefabAt(Vector2 position)
        {
            var obj = Instantiate(Prefab) as GameObject;
            obj.transform.position = Camera.main.ScreenToWorldPoint(new Vector3(position.x, position.y, 10));
            obj.transform.rotation = transform.rotation;
        }

        private void touchesBeganHandler(object sender, TouchEventArgs e)
        {
            foreach (var point in e.Touches)
            {
                spawnPrefabAt(point.Position);
            }
        }
    }
}