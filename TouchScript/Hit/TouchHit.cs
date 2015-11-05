/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using UnityEngine;
using UnityEngine.EventSystems;

namespace TouchScript.Hit
{
    public struct TouchHit
    {

        #region Consts

        public enum TouchHitType
        {
            Hit3D,
            Hit2D,
            HitUI
        }

        #endregion

        #region Public properties

        public TouchHitType Type { get { return type; } }

        public Transform Transform
        {
            get { return transform; }
        }

        public RaycastHit RaycastHit
        {
            get { return raycastHit; }
        }

        public RaycastHit2D RaycastHit2D
        {
            get { return raycastHit2D; }
        }

        public RaycastResult RaycastResult
        {
            get { return raycastResult; }
        }

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

        public TouchHit(Transform transform, RaycastHit raycastHit = default(RaycastHit),
            RaycastHit2D raycastHit2D = default(RaycastHit2D), RaycastResult raycastResult = default(RaycastResult))
        {
            this.transform = transform;
            this.raycastHit = raycastHit;
            this.raycastHit2D = raycastHit2D;
            this.raycastResult = raycastResult;
            type = TouchHitType.Hit3D;
        }

        public TouchHit(RaycastHit value) : this(value.collider.transform)
        {
            raycastHit = value;
            type = TouchHitType.Hit3D;
        }

        public TouchHit(RaycastHit2D value) :
            this(value.collider.transform)
        {
            raycastHit2D = value;
            type = TouchHitType.Hit2D;
        }

        public TouchHit(RaycastResult value) :
            this(value.gameObject.transform)
        {
            raycastResult = value;
            type = TouchHitType.HitUI;
        }

        #endregion
    }
}