/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using UnityEngine;
using System.Collections.Generic;

namespace TouchScript.Examples.Colors
{
    public class Colors : MonoBehaviour
    {
        public Transform Prefab;
        public int Total = 10;

        private List<Color> colors = new List<Color>()
        {
            Color.blue,
            Color.cyan,
            Color.gray,
            Color.green,
            Color.magenta,
            Color.red,
            Color.white,
            Color.yellow,
            Color.black
        };

        void Start()
        {
            var container = transform.Find("Container");
            for (var i = 0; i < Total; i++)
            {
                var obj = Instantiate(Prefab) as Transform;
                obj.SetParent(container);
                obj.localPosition = new Vector3(Random.Range(-100, 100), Random.Range(-100, 100), 0);
                obj.GetComponent<Renderer>().material.color = colors[Random.Range(0, colors.Count)];
            }
        }
    }
}