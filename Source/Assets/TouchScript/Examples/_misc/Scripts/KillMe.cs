/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using UnityEngine;
using System.Collections;

namespace TouchScript.Examples
{
    /// <summary>
    /// When enabled this component destroys the GameObject it is attached to in <see cref="Delay"/> seconds.
    /// </summary>
    public class KillMe : MonoBehaviour
    {
        public float Delay = 1f;

        private IEnumerator Start()
        {
            if (Delay != 0) yield return new WaitForSeconds(Delay);
            Destroy(gameObject);
        }
    }
}