using UnityEngine;
using System.Collections;
using TouchScript.Layers;

namespace TouchScript.Examples.Cube 
{
    public class Init : MonoBehaviour 
    {
        void Start () {
            var d = GetComponent<LayerDelegate>();
            var go = GameObject.Find("RenderTexture Camera");
            go.GetComponent<CameraLayer>().Delegate = d;
            go = GameObject.Find("Camera");
            go.GetComponent<CameraLayer>().Delegate = d;
        }
    }
}