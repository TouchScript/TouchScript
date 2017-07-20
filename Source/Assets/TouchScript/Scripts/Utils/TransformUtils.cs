/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System.Text;
using UnityEngine;

namespace TouchScript.Utils
{
    public static class TransformUtils
    {

        private static StringBuilder sb;

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