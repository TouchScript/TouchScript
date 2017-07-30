/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using UnityEngine;

namespace TouchScript.Examples.RawInput
{
    /// <exclude />
    public class Ball : MonoBehaviour
    {
        public float Speed = 1f;

        private void Update()
        {
            Speed *= 1.01f;
            transform.position += transform.forward * Speed * Time.unscaledDeltaTime;
            if (Speed > 1000) Destroy(gameObject);
        }
    }
}