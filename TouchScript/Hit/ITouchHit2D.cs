using UnityEngine;

namespace TouchScript.Hit
{
    public interface ITouchHit2D : ITouchHit
    {
        Collider2D Collider2D { get; }

        float Fraction { get; }

        Vector3 Point { get; }

        Rigidbody2D Rigidbody2D { get; }
    }
}