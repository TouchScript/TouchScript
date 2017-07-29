/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using TouchScript.Layers;
using UnityEngine;

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
            /// <summary>
            /// An unknown hit.
            /// </summary>
            Unknown,

            /// <summary>
            /// Nothing hit, but some object grabbed the pointer.
            /// </summary>
            Screen,

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
        public RaycastHitUI RaycastHitUI
        {
            get { return raycastHitUI; }
        }

        /// <summary>
        /// Indicates if this is a Screen Space hit.
        /// </summary>
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
                        return raycastHit.point;
                    case HitType.World2D:
                        return raycastHit2D.point;
                    case HitType.UI:
                        return raycastHitUI.WorldPosition;
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
                        return raycastHit.normal;
                    case HitType.World2D:
                        return raycastHit2D.normal;
                    case HitType.UI:
                        return raycastHitUI.WorldNormal;
                }
                return Vector3.forward;
            }
        }

        /// <summary>
        /// Distance to the hit point.
        /// </summary>
        public float Distance
        {
            get
            {
                switch (type)
                {
                    case HitType.World3D:
                        return raycastHit.distance;
                    case HitType.World2D:
                        return raycastHit2D.distance;
                    case HitType.UI:
                        return raycastHitUI.Distance;
                }
                return 0f;
            }
        }

        /// <summary>
        /// Sorting layer of the hit target.
        /// </summary>
        public int SortingLayer
        {
            get
            {
                switch (type)
                {
                    case HitType.World3D:
                        return 0;
                    case HitType.World2D:
                        if (sortingLayer == -1) updateSortingValues();
                        return sortingLayer;
                    case HitType.UI:
                        return raycastHitUI.SortingLayer;
                }
                return 0;
            }
        }

        /// <summary>
        /// Sorting order of the hit target.
        /// </summary>
        public int SortingOrder
        {
            get
            {
                switch (type)
                {
                    case HitType.World3D:
                        return 0;
                    case HitType.World2D:
                        if (sortingLayer == -1) updateSortingValues();
                        return sortingOrder;
                    case HitType.UI:
                        return raycastHitUI.SortingOrder;
                }
                return 0;
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
        private RaycastHitUI raycastHitUI;

        private int sortingLayer;
        private int sortingOrder;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="HitData"/> struct.
        /// </summary>
        /// <param name="target"> Target Target. </param>
        /// <param name="layer"> Touch layer this hit came from. </param>
        /// <param name="screenSpace"> If the hit is screenspace UI. </param>
        public HitData(Transform target, TouchLayer layer, bool screenSpace = false)
        {
            this.target = target;
            this.layer = layer;
            this.screenSpace = screenSpace;

            sortingLayer = -1;
            sortingOrder = -1;
            raycastHit = default(RaycastHit);
            raycastHit2D = default(RaycastHit2D);
            raycastHitUI = default(RaycastHitUI);
            type = HitType.Screen;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HitData"/> struct from a 3D raycast.
        /// </summary>
        /// <param name="value"> 3D raycast value. </param>
        /// <param name="layer"> Touch layer this hit came from. </param>
        /// <param name="screenSpace"> If the hit is screenspace UI. </param>
        public HitData(RaycastHit value, TouchLayer layer, bool screenSpace = false) : this(value.collider.transform, layer, screenSpace)
        {
            raycastHit = value;
            type = HitType.World3D;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HitData"/> struct from a 2D raycast.
        /// </summary>
        /// <param name="value"> 2D raycast value. </param>
        /// <param name="layer"> Touch layer this hit came from. </param>
        /// <param name="screenSpace"> If the hit is screenspace UI. </param>
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
        /// <param name="layer"> Touch layer this hit came from. </param>
        /// <param name="screenSpace"> If the hit is screenspace UI. </param>
        public HitData(RaycastHitUI value, TouchLayer layer, bool screenSpace = false) :
            this(value.Target, layer, screenSpace)
        {
            raycastHitUI = value;
            type = HitType.UI;
        }

        #endregion

        #region Private functions

        private void updateSortingValues()
        {
            var sprite = target.GetComponent<SpriteRenderer>();
            if (sprite == null)
            {
                sortingLayer = 0;
                sortingOrder = 0;
            }
            else
            {
                sortingLayer = sprite.sortingLayerID;
                sortingOrder = sprite.sortingOrder;
            }
        }

        #endregion
    }
}