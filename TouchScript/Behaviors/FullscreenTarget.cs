/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using UnityEngine;

namespace TouchScript.Behaviors
{
    /// <summary>
    /// Fullscreen plane collider attached to a camera.
    /// </summary>
    /// <remarks>
    /// Creates a fullscreen collider and changes its parameters dynamically with Camera it is attached to. Can be either at far clipping plane or near clipping plane of the Camera.
    /// </remarks>
    [AddComponentMenu("TouchScript/Behaviors/Fullscreen Target")]
    [ExecuteInEditMode]
    [RequireComponent(typeof(BoxCollider))]
    [RequireComponent(typeof(Camera))]
    public class FullscreenTarget : MonoBehaviour
    {
        /// <summary>
        /// Type of the Fullscreen Target.
        /// </summary>
        public enum TargetType
        {
            /// <summary>
            /// The target is attached to camera's far plane.
            /// </summary>
            Background,

            /// <summary>
            /// The target is attached to camera's near plane.
            /// </summary>
            Foreground
        }

        /// <summary>
        /// Type of the Fullscreen Target.
        /// </summary>
        public TargetType Type = TargetType.Background;

        private void Update()
        {
            var box = GetComponent<BoxCollider>();

            var h = 2*Mathf.Tan(camera.fieldOfView/360*Mathf.PI);
            if (Type == TargetType.Background)
            {
                h *= camera.farClipPlane;
                box.center = new Vector3(0, 0, camera.farClipPlane);
            } else if (Type == TargetType.Foreground)
            {
                h *= camera.nearClipPlane + .0051f;
                box.center = new Vector3(0, 0, camera.nearClipPlane + .0051f);
            }
            var w = (float)Screen.width/Screen.height*h;

            box.size = new Vector3(w, h, .01f);
        }
    }
}