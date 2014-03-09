/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using UnityEngine;

namespace TouchScript.Hit
{
    internal sealed class TouchHit3D : TouchHit, ITouchHit3D
    {

        public Vector3 BarycentricCoordinate { get; private set; }

        public Collider Collider { get; private set; }

        public float Distance { get; private set; }

        public Vector2 LightmapCoord { get; private set; }

        public Vector3 Normal { get; private set; }

        public Vector3 Point { get; private set; }

        public Rigidbody Rigidbody { get; private set; }

        public Vector2 TextureCoord { get; private set; }

        public Vector2 TextureCoord2 { get; private set; }

        public int TriangleIndex { get; private set; }

        internal void InitWith(RaycastHit value)
        {
            BarycentricCoordinate = value.barycentricCoordinate;
            Collider = value.collider;
            Distance = value.distance;
            LightmapCoord = value.lightmapCoord;
            Normal = value.normal;
            Point = value.point;
            Rigidbody = value.rigidbody;
            TextureCoord = value.textureCoord;
            TextureCoord2 = value.textureCoord2;
            TriangleIndex = value.triangleIndex;

            InitWith(value.collider.transform);
        }

    }
}
