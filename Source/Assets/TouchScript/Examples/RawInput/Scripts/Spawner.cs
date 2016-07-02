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
                TouchManager.Instance.PointersBegan += pointersBeganHandler;
            }
        }

        private void OnDisable()
        {
            if (TouchManager.Instance != null)
            {
                TouchManager.Instance.PointersBegan -= pointersBeganHandler;
            }
        }

        private void spawnPrefabAt(Vector2 position)
        {
            var obj = Instantiate(Prefab) as GameObject;
            obj.transform.position = Camera.main.ScreenToWorldPoint(new Vector3(position.x, position.y, 10));
            obj.transform.rotation = transform.rotation;
        }

        private void pointersBeganHandler(object sender, PointerEventArgs e)
        {
            foreach (var pointer in e.Pointers)
            {
                spawnPrefabAt(pointer.Position);
            }
        }
    }
}