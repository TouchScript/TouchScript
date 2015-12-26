/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using TouchScript.Gestures;
using TouchScript.Hit;
using UnityEngine;

namespace TouchScript.Examples.Tap
{
    public class Spawn : MonoBehaviour
    {
        public Transform CubePrefab;
        public Transform Container;
        public float Scale = .5f;

        private void OnEnable()
        {
            GetComponent<TapGesture>().Tapped += tappedHandler;
        }

        private void OnDisable()
        {
            GetComponent<TapGesture>().Tapped -= tappedHandler;
        }

        private void tappedHandler(object sender, EventArgs e)
        {
            var gesture = sender as TapGesture;
            TouchHit hit;
            gesture.GetTargetHitResult(out hit);

            var cube = Instantiate(CubePrefab) as Transform;
            cube.parent = Container;
            cube.name = "Cube";
            cube.localScale = Vector3.one*Scale*cube.localScale.x;
            cube.position = hit.Point + hit.Normal*.5f;
        }
    }
}