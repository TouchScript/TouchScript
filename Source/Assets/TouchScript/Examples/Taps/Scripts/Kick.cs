/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using UnityEngine;
using TouchScript.Gestures;

namespace TouchScript.Examples.Tap
{
    public class Kick : MonoBehaviour
    {
        public float Force = 3f;
        public ParticleSystem Particles;

        private TapGesture gesture;
        private Rigidbody rb;

        private void OnEnable()
        {
            rb = GetComponent<Rigidbody>();
            gesture = GetComponent<TapGesture>();
            gesture.Tapped += tappedHandler;
        }

        private void OnDisable()
        {
            gesture.Tapped -= tappedHandler;
        }

        private void tappedHandler(object sender, System.EventArgs e)
        {
            var ray = Camera.main.ScreenPointToRay(gesture.ScreenPosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit) && hit.transform == transform)
            {
                rb.AddForceAtPosition(ray.direction*Force, hit.point, ForceMode.Impulse);
                Instantiate(Particles, hit.point, Quaternion.identity);
            }
        }
    }
}