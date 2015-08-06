/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using System.Collections.Generic;
using TouchScript.Gestures;
using UnityEngine;

namespace TouchScript.Behaviors
{

    [AddComponentMenu("TouchScript/Behaviors/Transformer")]
    public class Transformer : MonoBehaviour
    {
        private Transform cachedTransform;
        private List<ITransformGesture> gestures;

        private void Awake()
        {
            cachedTransform = transform;
        }

        private void OnEnable()
        {
            var g = GetComponents<Gesture>();
            gestures = new List<ITransformGesture>(g.Length);
            for (var i = 0; i < g.Length; i++)
            {
                var transformGesture = g[i] as ITransformGesture;
                if (transformGesture == null) continue;

                gestures.Add(transformGesture);
                transformGesture.TransformStarted += transformStartedHandler;
                transformGesture.Transformed += transformHandler;
                transformGesture.TransformCompleted += transformCompletedHandler;
            }
        }

        private void OnDisable()
        {
            for (var i = 0; i < gestures.Count; i++)
            {
                var transformGesture = gestures[i];
                transformGesture.TransformStarted -= transformStartedHandler;
                transformGesture.Transformed -= transformHandler;
                transformGesture.TransformCompleted -= transformCompletedHandler;
            }
        }

        private void transformStartedHandler(object sender, EventArgs eventArgs)
        {
        }

        private void transformHandler(object sender, EventArgs e)
        {
            var gesture = sender as ITransformGesture;
            gesture.ApplyTransform(cachedTransform);
        }

        private void transformCompletedHandler(object sender, EventArgs eventArgs)
        {
        }

    }
}
