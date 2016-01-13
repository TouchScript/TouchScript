/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using TouchScript.Gestures.Base;
using TouchScript.Layers;
using TouchScript.Utils.Geom;
using UnityEngine;

namespace TouchScript.Gestures
{
    /// <summary>
    /// Recognizes a transform gesture in screen space, i.e. translation, rotation, scaling or a combination of these.
    /// </summary>
    [AddComponentMenu("TouchScript/Gestures/Screen Transform Gesture")]
    [HelpURL("http://touchscript.github.io/docs/html/T_TouchScript_Gestures_ScreenTransformGesture.htm")]
    public class ScreenTransformGesture : TransformGestureBase, ITransformGesture
    {
        #region Public methods

        /// <inheritdoc />
        public void ApplyTransform(Transform target)
        {
            if (DeltaPosition != Vector3.zero) target.position += DeltaPosition;
            if (!Mathf.Approximately(DeltaRotation, 0f))
                target.rotation = Quaternion.Euler(0, 0, DeltaRotation) * target.rotation;
            if (!Mathf.Approximately(DeltaScale, 1f)) target.localScale *= DeltaScale;
        }

        #endregion

        #region Protected methods

        /// <inheritdoc />
        protected override float doRotation(Vector2 oldScreenPos1, Vector2 oldScreenPos2, Vector2 newScreenPos1,
                                            Vector2 newScreenPos2, ProjectionParams projectionParams)
        {
            var oldScreenDelta = oldScreenPos2 - oldScreenPos1;
            var newScreenDelta = newScreenPos2 - newScreenPos1;
            return (Mathf.Atan2(newScreenDelta.y, newScreenDelta.x) -
                    Mathf.Atan2(oldScreenDelta.y, oldScreenDelta.x)) * Mathf.Rad2Deg;
        }

        /// <inheritdoc />
        protected override float doScaling(Vector2 oldScreenPos1, Vector2 oldScreenPos2, Vector2 newScreenPos1,
                                           Vector2 newScreenPos2, ProjectionParams projectionParams)
        {
            return (newScreenPos2 - newScreenPos1).magnitude / (oldScreenPos2 - oldScreenPos1).magnitude;
        }

        /// <inheritdoc />
        protected override Vector3 doOnePointTranslation(Vector2 oldScreenPos, Vector2 newScreenPos,
                                                         ProjectionParams projectionParams)
        {
            if (isTransforming)
            {
                return new Vector3(newScreenPos.x - oldScreenPos.x, newScreenPos.y - oldScreenPos.y, 0);
            }

            screenPixelTranslationBuffer += newScreenPos - oldScreenPos;
            if (screenPixelTranslationBuffer.sqrMagnitude > screenTransformPixelThresholdSquared)
            {
                isTransforming = true;
                return screenPixelTranslationBuffer;
            }

            return Vector3.zero;
        }

        /// <inheritdoc />
        protected override Vector3 doTwoPointTranslation(Vector2 oldScreenPos1, Vector2 oldScreenPos2,
                                                         Vector2 newScreenPos1, Vector2 newScreenPos2, float dR, float dS, ProjectionParams projectionParams)
        {
            if (isTransforming)
            {
                var transformedPoint = scaleAndRotate(oldScreenPos1, (oldScreenPos1 + oldScreenPos2) * .5f, dR, dS);
                return new Vector3(newScreenPos1.x - transformedPoint.x, newScreenPos1.y - transformedPoint.y, 0);
            }

            screenPixelTranslationBuffer += newScreenPos1 - oldScreenPos1;
            if (screenPixelTranslationBuffer.sqrMagnitude > screenTransformPixelThresholdSquared)
            {
                isTransforming = true;
                oldScreenPos1 = newScreenPos1 - screenPixelTranslationBuffer;
                var transformedPoint = scaleAndRotate(oldScreenPos1, (oldScreenPos1 + oldScreenPos2) * .5f, dR, dS);
                return new Vector3(newScreenPos1.x - transformedPoint.x, newScreenPos1.y - transformedPoint.y, 0);
            }

            return Vector3.zero;
        }

        #endregion

        #region Private functions

        private Vector2 scaleAndRotate(Vector2 point, Vector2 center, float dR, float dS)
        {
            var delta = point - center;
            if (dR != 0) delta = TwoD.Rotate(delta, dR);
            if (dS != 0) delta = delta * dS;
            return center + delta;
        }

        #endregion
    }
}