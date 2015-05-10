/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using UnityEngine;

namespace TouchScript.Layers
{
    public struct ProjectionParams
    {

        public bool IsValid
        {
            get { return func != null; }
        }

        private readonly Func<Vector2, Ray> func;

        public Vector3 Project(Vector2 position, Plane projectionPlane)
        {
            var ray = GetRay(position);
            float distance;
            var result = projectionPlane.Raycast(ray, out distance);
            if (!result && Mathf.Approximately(distance, 0f))
                return -projectionPlane.normal * projectionPlane.GetDistanceToPoint(Vector3.zero); // perpendicular to the screen
            return ray.origin + ray.direction * distance;
        }

        public Ray GetRay(Vector2 screenPosition)
        {
            return func(screenPosition);
        }

        public ProjectionParams(Func<Vector2, Ray> func = null)
        {
            if (func == null) this.func = TouchLayer.DefaultLayerProjection;
            else this.func = func;
        }

    }
}
