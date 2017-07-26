/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using UnityEngine;
using TouchScript.Layers;

namespace TouchScript.Examples.Cube 
{
    /// <exclude />
    public class Init : MonoBehaviour 
    {
        void Start () {
            var d = GetComponent<LayerDelegate>();
			var go = GameObject.Find("Scene Camera");
            go.GetComponent<TouchLayer>().Delegate = d;
            go = GameObject.Find("Camera");
			go.GetComponent<TouchLayer>().Delegate = d;
        }
    }
}