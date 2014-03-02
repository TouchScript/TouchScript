using UnityEngine;

namespace TouchScript.Hit
{
    internal sealed class TouchHit2D : TouchHit, ITouchHit2D
    {

        public Collider2D Collider2D { get; private set; }

        public float Fraction { get; private set; }

        public Vector3 Point { get; private set; }

        public Rigidbody2D Rigidbody2D { get; private set; }

        internal void InitWith(RaycastHit2D value)
        {
            Collider2D = value.collider;
            Fraction = value.fraction;
            Point = value.point;
            Rigidbody2D = value.rigidbody;

            InitWith(value.collider.transform);
        }

    }
}
