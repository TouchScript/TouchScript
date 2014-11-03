/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using UnityEngine;

namespace TouchScript.Hit
{
    internal sealed class TouchHit3D : TouchHit, ITouchHit3D
    {
        public Collider Collider
        {
            get { return hit.collider; }
        }

        public Vector3 Normal
        {
            get { return hit.normal; }
        }

        public Vector3 Point
        {
            get { return hit.point; }
        }

        public Rigidbody Rigidbody
        {
            get { return hit.rigidbody; }
        }

        private RaycastHit hit;

        internal void InitWith(RaycastHit value)
        {
            InitWith(value.collider.transform);
            hit = value;
        }
    }
}
