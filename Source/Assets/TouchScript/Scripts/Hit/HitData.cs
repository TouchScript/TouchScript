/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using TouchScript.Layers;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TouchScript.Hit
{
    /// <summary>
    /// An object representing a point hit by a pointer in 3D, 2D or UI space.
    /// </summary>
    public struct HitData
    {
        #region Consts

        /// <summary>
        /// Type of hit
        /// </summary>
        [Flags]
        public enum HitType
        {
            ScreenSpace,

            /// <summary>
            /// 3D hit.
            /// </summary>
            World3D,

            /// <summary>
            /// 2D hit.
            /// </summary>
            World2D,

            /// <summary>
            /// UI hit.
            /// </summary>
            UI
        }

        #endregion

        #region Public properties

        /// <summary>
        /// Gets the type of the hit.
        /// </summary>
        /// <value> The type. </value>
        public HitType Type
        {
            get { return type; }
        }

        /// <summary>
        /// Gets target Target the hit.
        /// </summary>
        /// <value> Hit Target. </value>
        public Transform Target
        {
            get { return target; }
        }

        /// <summary>
        /// Gets the layer which detected the hit.
        /// </summary>
        /// <value> Hit layer. </value>
        public TouchLayer Layer
        {
            get { return layer; }
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

        public bool ScreenSpace
        {
            get { return screenSpace; }
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
                    case HitType.World3D:
                        return RaycastHit.point;
                    case HitType.World2D:
                        return RaycastHit2D.point;
                    case HitType.UI:
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
                    case HitType.World3D:
                        return RaycastHit.normal;
                    case HitType.World2D:
                        return RaycastHit2D.normal;
                    case HitType.UI:
                        return RaycastResult.worldNormal;
                }
                return Vector3.forward;
            }
        }

        #endregion

        #region Private variables

        private HitType type;
        private Transform target;
        private bool screenSpace;
        private TouchLayer layer;
        private RaycastHit raycastHit;
        private RaycastHit2D raycastHit2D;
        private RaycastResult raycastResult;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="HitData"/> struct.
        /// </summary>
        /// <param name="target"> Target Target. </param>
        public HitData(Transform target, TouchLayer layer, bool screenSpace = false)
        {
            this.target = target;
            this.layer = layer;
            this.screenSpace = screenSpace;
            raycastHit = default(RaycastHit);
            raycastHit2D = default(RaycastHit2D);
            raycastResult = default(RaycastResult);
            type = HitType.ScreenSpace;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HitData"/> struct from a 3D raycast.
        /// </summary>
        /// <param name="value"> 3D raycast value. </param>
        public HitData(RaycastHit value, TouchLayer layer, bool screenSpace = false) : this(value.collider.transform, layer, screenSpace)
        {
            raycastHit = value;
            type = HitType.World3D;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HitData"/> struct from a 2D raycast.
        /// </summary>
        /// <param name="value"> 2D raycast value. </param>
        public HitData(RaycastHit2D value, TouchLayer layer, bool screenSpace = false) :
            this(value.collider.transform, layer, screenSpace)
        {
            raycastHit2D = value;
            type = HitType.World2D;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HitData"/> struct from a UI raycast.
        /// </summary>
        /// <param name="value"> UI raycast value. </param>
        public HitData(RaycastResult value, TouchLayer layer, bool screenSpace = false) :
            this(value.gameObject.transform, layer, screenSpace)
        {
            raycastResult = value;
            type = HitType.UI;
        }

        #endregion
    }
}