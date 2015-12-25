/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using UnityEngine;
using UnityEngine.EventSystems;

namespace TouchScript.Hit
{
    /// <summary>
    /// An object representing a point hit by a touch in 3D, 2D or UI space.
    /// </summary>
    public struct TouchHit
    {
        #region Consts

        /// <summary>
        /// Type of hit
        /// </summary>
        public enum TouchHitType
        {
            /// <summary>
            /// 3D hit.
            /// </summary>
            Hit3D,

            /// <summary>
            /// 2D hit.
            /// </summary>
            Hit2D,

            /// <summary>
            /// UI hit.
            /// </summary>
            HitUI
        }

        #endregion

        #region Public properties

        /// <summary>
        /// Gets the type of the hit.
        /// </summary>
        /// <value> The type. </value>
        public TouchHitType Type
        {
            get { return type; }
        }

        /// <summary>
        /// Gets target transform the touch hit.
        /// </summary>
        /// <value> Transform the touch hit. </value>
        public Transform Transform
        {
            get { return transform; }
        }

        /// <summary>
        /// Gets raycast hit object for a 3D hit.
        /// </summary>
        /// <value> Raycast hit object. </value>
        public RaycastHit RaycastHit
        {
            get { return raycastHit; }
        }

        /// <summary>
        /// Gets 2D raycast hit object for a 2D hit.
        /// </summary>
        /// <value> 2D raycast hit object. </value>
        public RaycastHit2D RaycastHit2D
        {
            get { return raycastHit2D; }
        }

        /// <summary>
        /// Gets raycast hit for a UI hit.
        /// </summary>
        /// <value> UI raycast hit object. </value>
        public RaycastResult RaycastResult
        {
            get { return raycastResult; }
        }

        /// <summary>
        /// Gets the point in 3D where raycast hit the object.
        /// </summary>
        /// <value> Point in 3D. </value>
        public Vector3 Point
        {
            get
            {
                switch (type)
                {
                    case TouchHitType.Hit3D:
                        return RaycastHit.point;
                    case TouchHitType.Hit2D:
                        return RaycastHit2D.point;
                    case TouchHitType.HitUI:
                        return RaycastResult.worldPosition;
                }
                return Vector3.zero;
            }
        }

        /// <summary>
        /// Gets the normal at the point in 3D wher eraycast hit the object.
        /// </summary>
        /// <value> Normal vector. </value>
        public Vector3 Normal
        {
            get
            {
                switch (type)
                {
                    case TouchHitType.Hit3D:
                        return RaycastHit.normal;
                    case TouchHitType.Hit2D:
                        return RaycastHit2D.normal;
                    case TouchHitType.HitUI:
                        return RaycastResult.worldNormal;
                }
                return Vector3.forward;
            }
        }

        #endregion

        #region Private variables

        private TouchHitType type;
        private Transform transform;
        private RaycastHit raycastHit;
        private RaycastHit2D raycastHit2D;
        private RaycastResult raycastResult;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="TouchHit"/> struct.
        /// </summary>
        /// <param name="transform"> Target transform. </param>
        public TouchHit(Transform transform)
        {
            this.transform = transform;
            raycastHit = default(RaycastHit);
            raycastHit2D = default(RaycastHit2D);
            raycastResult = default(RaycastResult);
            type = TouchHitType.Hit3D;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TouchHit"/> struct from a 3D raycast.
        /// </summary>
        /// <param name="value"> 3D raycast value. </param>
        public TouchHit(RaycastHit value) : this(value.collider.transform)
        {
            raycastHit = value;
            type = TouchHitType.Hit3D;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TouchHit"/> struct from a 2D raycast.
        /// </summary>
        /// <param name="value"> 2D raycast value. </param>
        public TouchHit(RaycastHit2D value) :
            this(value.collider.transform)
        {
            raycastHit2D = value;
            type = TouchHitType.Hit2D;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TouchHit"/> struct from a UI raycast.
        /// </summary>
        /// <param name="value"> UI raycast value. </param>
        public TouchHit(RaycastResult value) :
            this(value.gameObject.transform)
        {
            raycastResult = value;
            type = TouchHitType.HitUI;
        }

        #endregion
    }
}