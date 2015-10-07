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
        #region Private variables

        private Transform cachedTransform;
        private List<ITransformGesture> gestures = new List<ITransformGesture>();

        #endregion

        #region Unity methods

        private void Awake()
        {
            cachedTransform = transform;
        }

        private void OnEnable()
        {
            var g = GetComponents<Gesture>();
            for (var i = 0; i < g.Length; i++)
            {
                var transformGesture = g[i] as ITransformGesture;
                if (transformGesture == null) continue;

                gestures.Add(transformGesture);
                transformGesture.Transformed += transformHandler;
            }
        }

        private void OnDisable()
        {
            for (var i = 0; i < gestures.Count; i++)
            {
                var transformGesture = gestures[i];
                transformGesture.Transformed -= transformHandler;
            }
            gestures.Clear();
        }

        #endregion

        #region Event handlers

        private void transformHandler(object sender, EventArgs e)
        {
            var gesture = sender as ITransformGesture;
            gesture.ApplyTransform(cachedTransform);
        }

        #endregion

    }
}
