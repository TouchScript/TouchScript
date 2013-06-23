/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using UnityEngine;

namespace TouchScript.Behaviors
{
    /// <summary>
    /// Fullscreen plane collider
    /// </summary>
    [AddComponentMenu("TouchScript/Behaviors/Fullscreen Target")]
    [ExecuteInEditMode]
    [RequireComponent(typeof(BoxCollider))]
    [RequireComponent(typeof(Camera))]
    public class FullscreenTarget : MonoBehaviour
    {

        public enum TargetType
        {
            Background,
            Foreground
        }

        public TargetType Type = TargetType.Background;

        /// <summary>
        /// Unity Update callback.
        /// </summary>
        protected void Update()
        {
            var box = GetComponent<BoxCollider>();

            var h = 2 * Mathf.Tan(camera.fieldOfView / 360 * Mathf.PI);
            if (Type == TargetType.Background)
            {
                h *= camera.farClipPlane;
                box.center = new Vector3(0, 0, camera.farClipPlane);
            }
            else if (Type == TargetType.Foreground)
            {
                h *= camera.nearClipPlane;
                box.center = new Vector3(0, 0, camera.nearClipPlane + .005f);
            }
            var w = (float)Screen.width / Screen.height * h;

            box.size = new Vector3(w, h, .01f);
        }
    }
}