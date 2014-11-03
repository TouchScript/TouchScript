/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using UnityEngine;

namespace TouchScript.Hit
{
    internal sealed class TouchHit2D : TouchHit, ITouchHit2D
    {
        public Collider2D Collider2D
        {
            get { return hit.collider; }
        }

        public Vector3 Point
        {
            get { return hit.point; }
        }

        public Rigidbody2D Rigidbody2D
        {
            get { return hit.rigidbody; }
        }

        private RaycastHit2D hit;

        internal void InitWith(RaycastHit2D value)
        {
            InitWith(value.collider.transform);
            hit = value;
        }
    }
}
