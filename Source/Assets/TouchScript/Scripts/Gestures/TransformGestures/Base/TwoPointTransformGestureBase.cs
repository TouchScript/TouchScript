/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System.Collections.Generic;
using TouchScript.Layers;
using TouchScript.Utils.Geom;
using TouchScript.Pointers;
using UnityEngine;

#if TOUCHSCRIPT_DEBUG
using System.Collections;
using TouchScript.Debugging.GL;
#endif

namespace TouchScript.Gestures.TransformGestures.Base
{
    /// <summary>
    /// Abstract base classfor two-point transform gestures.
    /// </summary>
    public abstract class TwoPointTransformGestureBase : TransformGestureBase
    {
        #region Constants

        #endregion

        #region Events

        #endregion

        #region Public properties

        /// <summary>
        /// Gets or sets minimum distance between 2 points in cm for gesture to begin.
        /// </summary>
        /// <value> Minimum distance. </value>
        public virtual float MinScreenPointsDistance
        {
            get { return minScreenPointsDistance; }
            set
            {
                minScreenPointsDistance = value;
                updateMinScreenPointsDistance();
            }
        }

        #endregion

        #region Private variables

        /// <summary>
        /// <see cref="MinScreenPointsDistance"/> in pixels for internal use.
        /// </summary>
        protected float minScreenPointsPixelDistance;

        /// <summary>
        /// <see cref="MinScreenPointsDistance"/> squared in pixels for internal use.
        /// </summary>
        protected float minScreenPointsPixelDistanceSquared;

        /// <summary>
        /// Translation buffer.
        /// </summary>
        protected Vector2 screenPixelTranslationBuffer;

        /// <summary>
        /// Rotation buffer.
        /// </summary>
        protected float screenPixelRotationBuffer;

        /// <summary>
        /// Angle buffer.
        /// </summary>
        protected float angleBuffer;

        /// <summary>
        /// Screen space scaling buffer.
        /// </summary>
        protected float screenPixelScalingBuffer;

        /// <summary>
        /// Scaling buffer.
        /// </summary>
        protected float scaleBuffer;

        [SerializeField]
        private float minScreenPointsDistance = 0.5f;

        #endregion

        #region Unity methods

        /// <inheritdoc />
        protected override void OnEnable()
        {
            base.OnEnable();
            updateMinScreenPointsDistance();
        }

        #endregion

        #region Gesture callbacks

#if TOUCHSCRIPT_DEBUG
        /// <inheritdoc />
        protected override void pointersPressed(IList<Pointer> pointers)
        {
            base.pointersPressed(pointers);

            if (!(pointersNumState == PointersNumState.PassedMaxThreshold ||
                pointersNumState == PointersNumState.PassedMinMaxThreshold))
                drawDebugDelayed(getNumPoints());
        }
#endif

        /// <inheritdoc />
        protected override void pointersUpdated(IList<Pointer> pointers)
        {
            base.pointersUpdated(pointers);

            var projectionParams = activePointers[0].ProjectionParams;
            var dP = deltaPosition = Vector3.zero;
            var dR = deltaRotation = 0;
            var dS = deltaScale = 1f;

#if TOUCHSCRIPT_DEBUG
            drawDebugDelayed(getNumPoints());
#endif

            if (pointersNumState != PointersNumState.InRange) return;

            var translationEnabled = (Type & TransformGesture.TransformType.Translation) == TransformGesture.TransformType.Translation;
            var rotationEnabled = (Type & TransformGesture.TransformType.Rotation) == TransformGesture.TransformType.Rotation;
            var scalingEnabled = (Type & TransformGesture.TransformType.Scaling) == TransformGesture.TransformType.Scaling;

            // one pointer or one cluster (points might be too close to each other for 2 clusters)
            if (getNumPoints() == 1 || (!rotationEnabled && !scalingEnabled))
            {
                if (!translationEnabled) return; // don't look for translates
                if (!relevantPointers1(pointers)) return;

                // translate using one point
                dP = doOnePointTranslation(getPointPreviousScreenPosition(0), getPointScreenPosition(0), projectionParams);
            }
            else
            {
                // Make sure that we actually care about the pointers moved.
                if (!relevantPointers2(pointers)) return;

                var newScreenPos1 = getPointScreenPosition(0);
                var newScreenPos2 = getPointScreenPosition(1);

                // Here we can't reuse last frame screen positions because points 0 and 1 can change.
                // For example if the first of 3 fingers is lifted off.
                var oldScreenPos1 = getPointPreviousScreenPosition(0);
                var oldScreenPos2 = getPointPreviousScreenPosition(1);

                var newScreenDelta = newScreenPos2 - newScreenPos1;
                if (newScreenDelta.sqrMagnitude > minScreenPointsPixelDistanceSquared)
                {
                    if (rotationEnabled)
                    {
                        if (isTransforming)
                        {
                            dR = doRotation(oldScreenPos1, oldScreenPos2, newScreenPos1, newScreenPos2, projectionParams);
                        }
                        else
                        {
                            float d1, d2;
                            // Find how much we moved perpendicular to the line (oldScreenPos1, oldScreenPos2)
                            TwoD.PointToLineDistance2(oldScreenPos1, oldScreenPos2, newScreenPos1, newScreenPos2,
                                out d1, out d2);
                            screenPixelRotationBuffer += (d1 - d2);
                            angleBuffer += doRotation(oldScreenPos1, oldScreenPos2, newScreenPos1, newScreenPos2, projectionParams);

                            if (screenPixelRotationBuffer * screenPixelRotationBuffer >=
                                screenTransformPixelThresholdSquared)
                            {
                                isTransforming = true;
                                dR = angleBuffer;
                            }
                        }
                    }

                    if (scalingEnabled)
                    {
                        if (isTransforming)
                        {
                            dS *= doScaling(oldScreenPos1, oldScreenPos2, newScreenPos1, newScreenPos2, projectionParams);
                        }
                        else
                        {
                            var oldScreenDelta = oldScreenPos2 - oldScreenPos1;
                            var newDistance = newScreenDelta.magnitude;
                            var oldDistance = oldScreenDelta.magnitude;
                            screenPixelScalingBuffer += newDistance - oldDistance;
                            scaleBuffer *= doScaling(oldScreenPos1, oldScreenPos2, newScreenPos1, newScreenPos2, projectionParams);

                            if (screenPixelScalingBuffer * screenPixelScalingBuffer >=
                                screenTransformPixelThresholdSquared)
                            {
                                isTransforming = true;
                                dS = scaleBuffer;
                            }
                        }
                    }

                    if (translationEnabled)
                    {
                        if (dR == 0 && dS == 1) dP = doOnePointTranslation(oldScreenPos1, newScreenPos1, projectionParams);
                        else
                            dP = doTwoPointTranslation(oldScreenPos1, oldScreenPos2, newScreenPos1, newScreenPos2, dR, dS, projectionParams);
                    }
                }
                else if (translationEnabled)
                {
                    // points are too close, translate using one point
                    dP = doOnePointTranslation(oldScreenPos1, newScreenPos1, projectionParams);
                }
            }

            if (dP != Vector3.zero) transformMask |= TransformGesture.TransformType.Translation;
            if (dR != 0) transformMask |= TransformGesture.TransformType.Rotation;
            if (dS != 1) transformMask |= TransformGesture.TransformType.Scaling;

            if (transformMask != 0)
            {
                if (State == GestureState.Possible) setState(GestureState.Began);
                switch (State)
                {
                    case GestureState.Began:
                    case GestureState.Changed:
                        deltaPosition = dP;
                        deltaRotation = dR;
                        deltaScale = dS;
                        setState(GestureState.Changed);
                        resetValues();
                        break;
                }
            }
        }

        /// <inheritdoc />
        protected override void reset()
        {
            base.reset();

            screenPixelTranslationBuffer = Vector2.zero;
            screenPixelRotationBuffer = 0f;
            angleBuffer = 0;
            screenPixelScalingBuffer = 0f;
            scaleBuffer = 1f;

#if TOUCHSCRIPT_DEBUG
            clearDebug();
#endif
        }

        #endregion

        #region Protected methods

        /// <summary>
        /// Calculates rotation.
        /// </summary>
        /// <param name="oldScreenPos1"> Finger one old screen position. </param>
        /// <param name="oldScreenPos2"> Finger two old screen position. </param>
        /// <param name="newScreenPos1"> Finger one new screen position. </param>
        /// <param name="newScreenPos2"> Finger two new screen position. </param>
        /// <param name="projectionParams"> Layer projection parameters. </param>
        /// <returns> Angle in degrees. </returns>
        protected virtual float doRotation(Vector2 oldScreenPos1, Vector2 oldScreenPos2, Vector2 newScreenPos1,
                                           Vector2 newScreenPos2, ProjectionParams projectionParams)
        {
            return 0;
        }

        /// <summary>
        /// Calculates scaling.
        /// </summary>
        /// <param name="oldScreenPos1"> Finger one old screen position. </param>
        /// <param name="oldScreenPos2"> Finger two old screen position. </param>
        /// <param name="newScreenPos1"> Finger one new screen position. </param>
        /// <param name="newScreenPos2"> Finger two new screen position. </param>
        /// <param name="projectionParams"> Layer projection parameters. </param>
        /// <returns> Multiplicative delta scaling. </returns>
        protected virtual float doScaling(Vector2 oldScreenPos1, Vector2 oldScreenPos2, Vector2 newScreenPos1,
                                          Vector2 newScreenPos2, ProjectionParams projectionParams)
        {
            return 1;
        }

        /// <summary>
        /// Calculates single finger translation.
        /// </summary>
        /// <param name="oldScreenPos"> Finger old screen position. </param>
        /// <param name="newScreenPos"> Finger new screen position. </param>
        /// <param name="projectionParams"> Layer projection parameters. </param>
        /// <returns> Delta translation vector. </returns>
        protected virtual Vector3 doOnePointTranslation(Vector2 oldScreenPos, Vector2 newScreenPos,
                                                        ProjectionParams projectionParams)
        {
            return Vector3.zero;
        }

        /// <summary>
        /// Calculated two finger translation with respect to rotation and scaling.
        /// </summary>
        /// <param name="oldScreenPos1"> Finger one old screen position. </param>
        /// <param name="oldScreenPos2"> Finger two old screen position. </param>
        /// <param name="newScreenPos1"> Finger one new screen position. </param>
        /// <param name="newScreenPos2"> Finger two new screen position. </param>
        /// <param name="dR"> Calculated delta rotation. </param>
        /// <param name="dS"> Calculated delta scaling. </param>
        /// <param name="projectionParams"> Layer projection parameters. </param>
        /// <returns> Delta translation vector. </returns>
        protected virtual Vector3 doTwoPointTranslation(Vector2 oldScreenPos1, Vector2 oldScreenPos2,
                                                        Vector2 newScreenPos1, Vector2 newScreenPos2, float dR, float dS, ProjectionParams projectionParams)
        {
            return Vector3.zero;
        }

        /// <summary>
        /// Gets the number of points.
        /// </summary>
        /// <returns> Number of points. </returns>
        protected virtual int getNumPoints()
        {
            return NumPointers;
        }

        /// <summary>
        /// Checks if there are pointers in the list which matter for the gesture.
        /// </summary>
        /// <param name="pointers"> List of pointers. </param>
        /// <returns> <c>true</c> if there are relevant pointers; <c>false</c> otherwise.</returns>
        protected virtual bool relevantPointers1(IList<Pointer> pointers)
        {
            // We care only about the first pointer
            var count = pointers.Count;
            for (var i = 0; i < count; i++)
            {
                if (pointers[i] == activePointers[0]) return true;
            }
            return false;
        }

        /// <summary>
        /// Checks if there are pointers in the list which matter for the gesture.
        /// </summary>
        /// <param name="pointers"> List of pointers. </param>
        /// <returns> <c>true</c> if there are relevant pointers; <c>false</c> otherwise.</returns>
        protected virtual bool relevantPointers2(IList<Pointer> pointers)
        {
            // We care only about the first and the second pointers
            var count = pointers.Count;
            for (var i = 0; i < count; i++)
            {
                var pointer = pointers[i];
                if (pointer == activePointers[0] || pointer == activePointers[1]) return true;
            }
            return false;
        }

        /// <summary>
        /// Returns screen position of a point with index 0 or 1
        /// </summary>
        /// <param name="index"> The index. </param>
        protected virtual Vector2 getPointScreenPosition(int index)
        {
            return activePointers[index].Position;
        }

        /// <summary>
        /// Returns previous screen position of a point with index 0 or 1
        /// </summary>
        /// <param name="index"> The index. </param>
        protected virtual Vector2 getPointPreviousScreenPosition(int index)
        {
            return activePointers[index].PreviousPosition;
        }

#if TOUCHSCRIPT_DEBUG
        protected virtual void clearDebug()
        {
            GLDebug.RemoveFigure(debugID);
            GLDebug.RemoveFigure(debugID + 1);
            GLDebug.RemoveFigure(debugID + 2);

            if (debugCoroutine != null) StopCoroutine(debugCoroutine);
            debugCoroutine = null;
        }

        protected void drawDebugDelayed(int touchPoints)
        {
            if (debugCoroutine != null) StopCoroutine(debugCoroutine);
            debugCoroutine = StartCoroutine(doDrawDebug(touchPoints));
        }

        protected virtual void drawDebug(int touchPoints)
        {
            if (!DebugMode) return;

            var color = State == GestureState.Possible ? Color.red : Color.green;
            switch (touchPoints)
            {
                case 1:
                    GLDebug.DrawSquareScreenSpace(debugID, getPointScreenPosition(0), 0f, debugPointerSize, color,
                        float.PositiveInfinity);
                    GLDebug.RemoveFigure(debugID + 1);
                    GLDebug.RemoveFigure(debugID + 2);
                    break;
                default:
                    var newScreenPos1 = getPointScreenPosition(0);
                    var newScreenPos2 = getPointScreenPosition(1);
                    GLDebug.DrawSquareScreenSpace(debugID, newScreenPos1, 0f, debugPointerSize, color,
                        float.PositiveInfinity);
                    GLDebug.DrawSquareScreenSpace(debugID + 1, newScreenPos2, 0f, debugPointerSize, color,
                        float.PositiveInfinity);
                    GLDebug.DrawLineWithCrossScreenSpace(debugID + 2, newScreenPos1, newScreenPos2, .5f,
                        debugPointerSize * .3f, color, float.PositiveInfinity);
                    break;
            }
        }

        private IEnumerator doDrawDebug(int touchPoints)
        {
            yield return new WaitForEndOfFrame();

            drawDebug(touchPoints);
        }
#endif

        #endregion

        #region Private functions

        private void updateMinScreenPointsDistance()
        {
            minScreenPointsPixelDistance = minScreenPointsDistance * touchManager.DotsPerCentimeter;
            minScreenPointsPixelDistanceSquared = minScreenPointsPixelDistance * minScreenPointsPixelDistance;
        }

        #endregion
    }
}