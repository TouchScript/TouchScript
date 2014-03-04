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
            if (transform.parent != null)
            {
                return transform.parent.InverseTransformPoint(global);
            }
            return global;
        }
    }
}