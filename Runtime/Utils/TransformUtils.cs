/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System.Text;
using UnityEngine;

namespace TouchScript.Utils
{
    /// <summary>
    /// Utility methods to work with Transforms.
    /// </summary>
    public static class TransformUtils
    {
        private static StringBuilder sb;

        /// <summary>
        /// Converts a global position of a transform to local position in its parent's coordinate system.
        /// </summary>
        /// <param name="transform">The transform.</param>
        /// <param name="global">The global position.</param>
        /// <returns>Local position in transform parent's coordinate system.</returns>
        public static Vector3 GlobalToLocalPosition(Transform transform, Vector3 global)
        {
            if (transform.parent == null) return global;
            return transform.parent.InverseTransformPoint(global);
        }

        /// <summary>
        /// Converts a global direction of a transform to local direction in its parent's coordinate system.
        /// </summary>
        /// <param name="transform">The transform.</param>
        /// <param name="global">The global direction.</param>
        /// <returns>Local direction in transform parent's coordinate system.</returns>
        public static Vector3 GlobalToLocalDirection(Transform transform, Vector3 global)
        {
            if (transform.parent == null) return global;
            return transform.parent.InverseTransformDirection(global);
        }

        /// <summary>
        /// Converts a global vector of a transform to local vector in its parent's coordinate system. The difference from <see cref="GlobalToLocalDirection"/> is that this vector has length.
        /// </summary>
        /// <param name="transform">The transform.</param>
        /// <param name="global">The global vector.</param>
        /// <returns>Local vector in transform parent's coordinate system.</returns>
        public static Vector3 GlobalToLocalVector(Transform transform, Vector3 global)
        {
            var parent = transform.parent;
            if (parent == null) return global;

            var scale = parent.localScale;
            var vector = GlobalToLocalVector(parent, global);
            vector = Quaternion.Inverse(parent.localRotation) * vector;
            vector.x = Mathf.Approximately(scale.x, 0) ? 0 : vector.x / scale.x;
            vector.y = Mathf.Approximately(scale.y, 0) ? 0 : vector.y / scale.y;
            vector.z = Mathf.Approximately(scale.z, 0) ? 0 : vector.z / scale.z;

            return vector;
        }

        /// <summary>
        /// Returns the string path of the transform in the hierarchy, i.g. "GameObject/ChildGameObject".
        /// </summary>
        /// <param name="transform">The transform.</param>
        /// <returns>The path in the hierarchy.</returns>
        public static string GetHeirarchyPath(Transform transform)
        {
            initStringBuilder();

            if (transform == null) return null;

            while (transform != null)
            {
                sb.Insert(0, transform.name);
                sb.Insert(0, "/");
                transform = transform.parent;
            }
            return sb.ToString();
        }

        private static void initStringBuilder()
        {
            if (sb == null) sb = new StringBuilder();
            sb.Length = 0;
        }
    }
}