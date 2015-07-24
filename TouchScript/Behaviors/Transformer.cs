/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using TouchScript.Gestures;
using UnityEngine;

namespace TouchScript.Behaviors
{

    [AddComponentMenu("TouchScript/Behaviors/Transformer")]
    public class Transformer : MonoBehaviour
    {

        private Transform cachedTransform;

        private void Awake()
        {
            cachedTransform = transform;
        }

        private void OnEnable()
        {
            if (GetComponent<TransformGesture>() != null) GetComponent<TransformGesture>().Transformed += transformHandler;
        }

        private void OnDisable()
        {
            if (GetComponent<TransformGesture>() != null) GetComponent<TransformGesture>().Transformed -= transformHandler;
        }

        private void transformHandler(object sender, EventArgs e)
        {
            var gesture = (TransformGesture) sender;
            gesture.ApplyTransform(cachedTransform);
            //cachedTransform.position += gesture.DeltaPosition;
        }

    }
}
