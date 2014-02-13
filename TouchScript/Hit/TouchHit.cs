using UnityEngine;

namespace TouchScript.Hit
{
    public class TouchHit
    {
        #region Constants

        public enum HitType
        {
            /// <summary>
            /// The hit is assigned to an object with Collider in 3D space.
            /// </summary>
            Hit3D,

            /// <summary>
            /// The hit is assigned to a 2D object with Collider2D.
            /// </summary>
            Hit2D,

            /// <summary>
            /// The hit exists in screen space and doesn't necessary correspond to 3D object with Collider.
            /// </summary>
            Screen
        }

        #endregion

        #region Public properties

        public HitType Type { get; private set; }
        public Vector3 BarycentricCoordinate { get; private set; }
        public Collider Collider { get; private set; }
        public Collider2D Collider2d { get; private set; }
        public float Distance { get; private set; }
        public Vector2 LightmapCoord { get; private set; }
        public Vector3 Normal { get; private set; }
        public Vector3 Point { get; private set; }
        public Rigidbody Rigidbody { get; private set; }
        public Rigidbody2D Rigidbody2D { get; private set; }
        public Vector2 TextureCoord { get; private set; }
        public Vector2 TextureCoord2 { get; private set; }
        public Transform Transform { get; private set; }
        public int TriangleIndex { get; private set; }

        #endregion

        public static int count;

        public static TouchHit GetTouchHit(RaycastHit value)
        {
            var result = new TouchHit(HitType.Hit3D);
            result.InitWith(value);
            return result;
        }

        public static TouchHit GetTouchHit(RaycastHit2D value)
        {
            var result = new TouchHit(HitType.Hit2D);
            result.InitWith(value);
            return result;
        }

        public static TouchHit GetTouchHit(Transform value)
        {
            var result = new TouchHit(HitType.Screen);
            result.InitWith(value);
            return result;
        }

        #region Constructors

        private TouchHit(HitType type)
        {
            Type = type;
        }

        #endregion

        #region Internal methods

        internal void InitWith(RaycastHit value)
        {
            Type = HitType.Hit3D;

            BarycentricCoordinate = value.barycentricCoordinate;
            Collider = value.collider;
            Distance = value.distance;
            LightmapCoord = value.lightmapCoord;
            Normal = value.normal;
            Point = value.point;
            Rigidbody = value.rigidbody;
            TextureCoord = value.textureCoord;
            TextureCoord2 = value.textureCoord2;
            Transform = value.collider.transform;
            TriangleIndex = value.triangleIndex;
        }

        internal void InitWith(RaycastHit2D value)
        {
            Type = HitType.Hit2D;

            Collider2d = value.collider;
            Distance = value.fraction;
            Normal = -Vector3.forward;
            Point = value.point;
            Rigidbody2D = value.rigidbody;
            Transform = value.collider.transform;
        }

        internal void InitWith(Transform value)
        {
            Type = HitType.Screen;

            Transform = value;
        }

        #endregion
    }
}