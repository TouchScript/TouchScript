/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using UnityEngine;

namespace TouchScript.Behaviors
{
    /// <summary>
    /// Fullscreen plane collider which is positioned at camera's far clipping plane to recieve all touch points not received by other objects.
    /// </summary>
    [ExecuteInEditMode]
    [RequireComponent(typeof(BoxCollider))]
    [RequireComponent(typeof(Camera))]
    public class FullscreenBackgroundTarget : MonoBehaviour
    {

        private void Awake()
        {
            Debug.LogWarning("FullscreenBackgroundTarget class is deprecated, use FullscreenTarget (type = Background) instead.");
        }

        /// <summary>
        /// Unity Update callback.
        /// </summary>
        protected void Update()
        {
            var box = GetComponent<BoxCollider>();

            var h = 2*camera.farClipPlane*Mathf.Tan(camera.fieldOfView/360*Mathf.PI);
            var w = (float)Screen.width/Screen.height*h;

            box.center = new Vector3(0, 0, camera.farClipPlane);
            box.size = new Vector3(w, h, .1f);
        }
    }
}