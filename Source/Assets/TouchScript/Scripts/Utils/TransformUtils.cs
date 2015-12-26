/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using UnityEngine;

namespace TouchScript.Utils
{
    internal static class TransformUtils
    {
        public static Vector3 GlobalToLocalPosition(Transform transform, Vector3 global)
        {
            if (transform.parent == null) return global;
            return transform.parent.InverseTransformPoint(global);
        }

        public static Vector3 GlobalToLocalDirection(Transform transform, Vector3 global)
        {
            if (transform.parent == null) return global;
            return transform.parent.InverseTransformDirection(global);
        }

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
    }
}