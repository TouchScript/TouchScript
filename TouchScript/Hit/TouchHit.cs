/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TouchScript.Hit
{
    public struct TouchHit
    {
        #region Public properties

        public Transform Transform
        {
            get { return transform; }
        }

        public Vector3 Point
        {
            get { return point; }
        }

        public Vector3 Normal
        {
            get { return normal; }
        }

        public Collider Collider
        {
            get { return collider; }
        }

        public Rigidbody Rigidbody
        {
            get { return rigidbody; }
        }

        public Collider2D Collider2D
        {
            get { return collider2D; }
        }

        public Rigidbody2D Rigidbody2D
        {
            get { return rigidbody2D; }
        }

        public Graphic Graphic
        {
            get { return graphic; }
        }

        #endregion

        #region Private variables

        private readonly Transform transform;
        private readonly Vector3 point;
        private readonly Vector3 normal;
        private readonly Collider collider;
        private readonly Rigidbody rigidbody;
        private readonly Collider2D collider2D;
        private readonly Rigidbody2D rigidbody2D;
        private readonly Graphic graphic;

        #endregion

        #region Constructors

        public TouchHit(Transform transform, Vector3 point = default(Vector3), Vector3 normal = default(Vector3),
            Collider collider = null, Rigidbody rigidbody = null,
            Collider2D collider2D = null, Rigidbody2D rigidbody2D = null, Graphic graphic = null)
        {
            this.transform = transform;
            this.point = point;
            this.normal = normal == Vector3.zero ? Vector3.forward : normal;
            this.collider = collider;
            this.rigidbody = rigidbody;
            this.collider2D = collider2D;
            this.rigidbody2D = rigidbody2D;
            this.graphic = graphic;
        }

        public TouchHit(RaycastHit value) :
            this(value.collider.transform, value.point, value.normal, value.collider, value.rigidbody) {}

        public TouchHit(RaycastHit2D value) :
            this(value.collider.transform, value.point, value.normal, null, null, value.collider, value.rigidbody) {}

        public TouchHit(RaycastResult value) :
            this(value.gameObject.transform, value.worldPosition, value.worldNormal)
        {
            if (value.module == null) return;
            graphic = value.gameObject.GetComponent<Graphic>();
        }

        #endregion
    }
}