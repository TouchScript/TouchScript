/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using UnityEngine;
using TouchScript.Gestures;
using Random = UnityEngine.Random;

namespace TouchScript.Examples.Tap
{
    /// <exclude />
    public class Break : MonoBehaviour
    {
        public float Power = 10.0f;

        private LongPressGesture longPressGesture;
        private PressGesture pressGesture;
        private MeshRenderer rnd;
        private bool growing = false;
        private float growingTime = 0;

        private Vector3[] directions =
        {
            new Vector3(1, -1, 1),
            new Vector3(-1, -1, 1),
            new Vector3(-1, -1, -1),
            new Vector3(1, -1, -1),
            new Vector3(1, 1, 1),
            new Vector3(-1, 1, 1),
            new Vector3(-1, 1, -1),
            new Vector3(1, 1, -1)
        };

        private void OnEnable()
        {
            rnd = GetComponent<MeshRenderer>();
            longPressGesture = GetComponent<LongPressGesture>();
            pressGesture = GetComponent<PressGesture>();

            longPressGesture.StateChanged += longPressedHandler;
            pressGesture.Pressed += pressedHandler;
        }

        private void OnDisable()
        {
            longPressGesture.StateChanged -= longPressedHandler;
            pressGesture.Pressed -= pressedHandler;
        }

        private void Update()
        {
            if (growing)
            {
                growingTime += Time.unscaledDeltaTime;
                rnd.material.color = Color.Lerp(Color.white, Color.red, growingTime);
            }
        }

        private void startGrowing()
        {
            growing = true;
        }

        private void stopGrowing()
        {
            growing = false;
            growingTime = 0;
            rnd.material.color = Color.white;
        }

        private void pressedHandler(object sender, EventArgs e)
        {
            startGrowing();
        }

        private void longPressedHandler(object sender, GestureStateChangeEventArgs e)
        {
            if (e.State == Gesture.GestureState.Recognized)
            {
                // if we are not too small
                if (transform.localScale.x > 0.05f)
                {
                    // break this cube into 8 parts
                    for (int i = 0; i < 8; i++)
                    {
                        var obj = Instantiate(gameObject) as GameObject;
                        var cube = obj.transform;
                        cube.parent = transform.parent;
                        cube.name = "Cube";
                        cube.localScale = 0.5f*transform.localScale;
                        cube.position = transform.TransformPoint(directions[i]/4);
                        cube.GetComponent<Rigidbody>().AddForce(Power*Random.insideUnitSphere, ForceMode.Impulse);
                        cube.GetComponent<Renderer>().material.color = Color.white;
                    }
                    Destroy(gameObject);
                }
            }
            else if (e.State == Gesture.GestureState.Failed)
            {
                stopGrowing();
            }
        }
    }
}