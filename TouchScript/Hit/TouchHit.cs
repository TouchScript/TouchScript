using UnityEngine;

namespace TouchScript.Hit
{
    public struct TouchHit
    {
        #region Constants

        public enum HitType
        {
            Hit3D,
            Hit2D
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

        #region Public methods

        public static TouchHit FromRaycastHit(RaycastHit value)
        {
            var result = new TouchHit()
            {
                Type = HitType.Hit3D,
                BarycentricCoordinate = value.barycentricCoordinate,
                Collider = value.collider,
                Distance = value.distance,
                LightmapCoord = value.lightmapCoord,
                Normal = value.normal,
                Point = value.point,
                Rigidbody = value.rigidbody,
                TextureCoord = value.textureCoord,
                TextureCoord2 = value.textureCoord2,
                Transform = value.collider.transform,
                TriangleIndex = value.triangleIndex
            };
            return result;
        }

        public static TouchHit FromRaycastHit2D(RaycastHit2D value)
        {
            var result = new TouchHit()
            {
                Type = HitType.Hit2D,
                Collider2d = value.collider,
                Distance = value.fraction,
                Normal = -Vector3.forward,
                Point = value.point,
                Rigidbody2D = value.rigidbody,
                Transform = value.collider.transform
            };
            return result;
        }

        #endregion
    }
}