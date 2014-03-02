using UnityEngine;

namespace TouchScript.Utils
{
    public static class TransformUtils
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
