/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using UnityEngine;

namespace TouchScript.Layers
{
    public struct ProjectionParams
    {

        public Camera Camera { get { return camera; } }

        private Camera camera;

        public Ray GetRay(Vector2 screenPosition)
        {
            if (Camera == null) return new Ray(Vector3.zero, Vector3.forward);
            return camera.ScreenPointToRay(screenPosition);
        }

        public ProjectionParams(Camera camera = null)
        {
            this.camera = camera;
        }

    }
}
