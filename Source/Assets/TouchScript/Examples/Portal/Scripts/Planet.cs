/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using UnityEngine;
using TouchScript.Gestures;
using TouchScript.Gestures.TransformGestures;

namespace TouchScript.Examples.Portal
{
    /// <exclude />
    public class Planet : MonoBehaviour
    {
        private enum PlanetStatus
        {
            Free,
            Manual,
            Falling
        }

        public float Speed = 30f;
        public float RotationSpeed = 30f;
        public float FallSpeed = .01f;

        private PlanetStatus status = PlanetStatus.Free;

        public void Fall()
        {
            status = PlanetStatus.Falling;
            var gesture = GetComponent<TransformGesture>();
            if (gesture != null) gesture.Cancel();
        }

        private void OnEnable()
        {
            GetComponent<PressGesture>().Pressed += pressedhandler;
            GetComponent<ReleaseGesture>().Released += releasedHandler;
        }

        private void OnDisable()
        {
            GetComponent<PressGesture>().Pressed -= pressedhandler;
            GetComponent<ReleaseGesture>().Released -= releasedHandler;
        }

        private void Update()
        {
            switch (status)
            {
                case PlanetStatus.Free:
                    transform.RotateAround(transform.parent.position, Vector3.up,
                        Speed * Time.unscaledDeltaTime / transform.localPosition.sqrMagnitude);
                    break;
                case PlanetStatus.Manual:
                    break;
                case PlanetStatus.Falling:
                    transform.localScale *= 1 - FallSpeed;
                    transform.localPosition = Vector3.Lerp(transform.localPosition, Vector3.zero, FallSpeed);
                    if (transform.localScale.x < 0.01) Destroy(gameObject);
                    break;
            }

            transform.Rotate(0, 0, Time.unscaledDeltaTime * RotationSpeed);
        }

        void pressedhandler(object sender, System.EventArgs e)
        {
            status = PlanetStatus.Manual;
        }

        void releasedHandler(object sender, System.EventArgs e)
        {
            if (status != PlanetStatus.Manual) return;
            status = PlanetStatus.Free;
        }
    }
}