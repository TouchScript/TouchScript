using UnityEngine;

namespace TouchScript.Hit
{
    public interface ITouchHit3D : ITouchHit
    {
        Vector3 BarycentricCoordinate { get; }

        Collider Collider { get; }

        float Distance { get; }

        Vector2 LightmapCoord { get; }

        Vector3 Normal { get; }

        Vector3 Point { get; }

        Rigidbody Rigidbody { get; }

        Vector2 TextureCoord { get; }

        Vector2 TextureCoord2 { get; }

        int TriangleIndex { get; }
    }
}