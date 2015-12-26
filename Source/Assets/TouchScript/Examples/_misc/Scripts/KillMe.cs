/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using UnityEngine;
using System.Collections;

namespace TouchScript.Examples
{
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