using UnityEngine;

namespace TouchScript.Behaviors {
    [AddComponentMenu("TouchScript/Fullscreen Background Target")]
    [ExecuteInEditMode]
    [RequireComponent(typeof (BoxCollider))]
    [RequireComponent(typeof (Camera))]
    public class FullscreenBackgroundTarget : MonoBehaviour {
        protected void Update() {
            var box = GetComponent<BoxCollider>();

            var h = 2*camera.farClipPlane*Mathf.Tan(camera.fieldOfView/360*Mathf.PI);
            var w = (float) Screen.width/Screen.height*h;

            box.center = new Vector3(0, 0, camera.farClipPlane);
            box.size = new Vector3(w, h, .1f);
        }
    }
}