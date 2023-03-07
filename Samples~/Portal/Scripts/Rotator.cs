/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using UnityEngine;

namespace TouchScript.Examples.Portal
{
    /// <exclude />
    public class Rotator : MonoBehaviour
    {
        public float RotationSpeed = 1f;

        void Update()
        {
            transform.localRotation *= Quaternion.Euler(0, 0, Time.unscaledDeltaTime * RotationSpeed);
        }
    }
}