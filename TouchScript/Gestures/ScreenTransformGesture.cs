/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using TouchScript.Behaviors;
using TouchScript.Gestures.Abstract;
using UnityEngine;

namespace TouchScript.Gestures
{
    [AddComponentMenu("TouchScript/Gestures/Screen Transform Gesture")]
    public class ScreenTransformGesture : TransformGestureBase, ITransformer
    {

        #region Public methods

        public void ApplyTransform(Transform target)
        {
            if (DeltaPosition != Vector3.zero) target.position += DeltaPosition;
            if (!Mathf.Approximately(DeltaRotation, 0f))
                target.rotation *= Quaternion.Euler(0, 0, DeltaRotation);
            if (!Mathf.Approximately(DeltaScale, 1f)) target.localScale *= DeltaScale;
        }

        #endregion

        #region Protected methods

        protected override float doRotation(Vector2 oldScreenPos1, Vector2 oldScreenPos2, Vector2 newScreenPos1,
            Vector2 newScreenPos2)
        {
            var oldScreenDelta = oldScreenPos2 - oldScreenPos1;
            var newScreenDelta = newScreenPos2 - newScreenPos1;
            return (Mathf.Atan2(newScreenDelta.y, newScreenDelta.x) -
                    Mathf.Atan2(oldScreenDelta.y, oldScreenDelta.x))*Mathf.Rad2Deg;
        }

        protected override float doScaling(Vector2 oldScreenPos1, Vector2 oldScreenPos2, Vector2 newScreenPos1,
            Vector2 newScreenPos2)
        {
            return (newScreenPos2 - newScreenPos1).magnitude/(oldScreenPos2 - oldScreenPos1).magnitude;
        }

        protected override Vector3 doTranslation(Vector2 oldScreenCenter, Vector2 newScreenCenter)
        {
            if (isTransforming)
            {
                return new Vector3(newScreenCenter.x - oldScreenCenter.x, newScreenCenter.y - oldScreenCenter.y, 0);
            }

            screenPixelTranslationBuffer += newScreenCenter - oldScreenCenter;
            if (screenPixelTranslationBuffer.sqrMagnitude > screenTransformPixelThresholdSquared)
            {
                isTransforming = true;
                return screenPixelTranslationBuffer;
            }

            return Vector3.zero;
        }

        #endregion
    }
}