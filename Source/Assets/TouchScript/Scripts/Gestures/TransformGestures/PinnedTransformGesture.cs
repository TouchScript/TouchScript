/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System.Collections.Generic;
using TouchScript.Gestures.TransformGestures.Base;
using TouchScript.Layers;
using TouchScript.Utils.Geom;
using TouchScript.Pointers;
using UnityEngine.Profiling;

#if TOUCHSCRIPT_DEBUG
using TouchScript.Debugging.GL;
#endif
using UnityEngine;

namespace TouchScript.Gestures.TransformGestures
{
    /// <summary>
    /// Recognizes a transform gesture around center of the object, i.e. one finger rotation, scaling or a combination of these.
    /// </summary>
    [AddComponentMenu("TouchScript/Gestures/Pinned Transform Gesture")]
    [HelpURL("http://touchscript.github.io/docs/html/T_TouchScript_Gestures_TransformGestures_PinnedTransformGesture.htm")]
    public class PinnedTransformGesture : OnePointTrasformGestureBase
    {
        #region Constants

        #endregion

        #region Public properties

        /// <summary>
        /// Gets or sets transform's projection type.
        /// </summary>
        /// <value> Projection type. </value>
        public TransformGesture.ProjectionType Projection
        {
            get { return projection; }
            set
            {
                if (projection == value) return;
                projection = value;
                if (Application.isPlaying) updateProjectionPlane();
            }
        }

        /// <summary>
        /// Gets or sets transform's projection plane normal.
        /// </summary>
        /// <value> Projection plane normal. </value>
        public Vector3 ProjectionPlaneNormal
        {
            get
            {
                if (projection == TransformGesture.ProjectionType.Layer) return projectionLayer.WorldProjectionNormal;
                return projectionPlaneNormal;
            }
            set
            {
                if (projection == TransformGesture.ProjectionType.Layer) projection = TransformGesture.ProjectionType.Object;
                value.Normalize();
                if (projectionPlaneNormal == value) return;
                projectionPlaneNormal = value;
                if (Application.isPlaying) updateProjectionPlane();
            }
        }

        /// <summary>
        /// Plane where transformation occured.
        /// </summary>
        public Plane TransformPlane
        {
            get { return transformPlane; }
        }

        #endregion

        #region Private variables

		[SerializeField]
		private bool projectionProps; // Used in the custom inspector

        [SerializeField]
        private TransformGesture.ProjectionType projection = TransformGesture.ProjectionType.Layer;

        [SerializeField]
        private Vector3 projectionPlaneNormal = Vector3.forward;

        private TouchLayer projectionLayer;
        private Plane transformPlane;

		private CustomSampler gestureSampler;

        #endregion

        #region Public methods

        #endregion

        #region Unity methods

        /// <inheritdoc />
        protected override void Awake()
        {
            base.Awake();

            transformPlane = new Plane();
			gestureSampler = CustomSampler.Create("[TouchScript] Pinned Transform Gesture");
        }

        /// <inheritdoc />
        protected override void OnEnable()
        {
            base.OnEnable();

            updateProjectionPlane();
        }

		[ContextMenu("Basic Editor")]
		private void switchToBasicEditor()
		{
			basicEditor = true;
		}

        #endregion

        #region Gesture callbacks

        /// <inheritdoc />
        protected override void pointersPressed(IList<Pointer> pointers)
        {
			gestureSampler.Begin();

            base.pointersPressed(pointers);

            if (NumPointers == pointers.Count)
            {
                projectionLayer = activePointers[0].GetPressData().Layer;
                updateProjectionPlane();

#if TOUCHSCRIPT_DEBUG
                drawDebug(activePointers[0].ProjectionParams.ProjectFrom(cachedTransform.position), activePointers[0].Position);
#endif
            }

			gestureSampler.End();
        }

		/// <inheritdoc />
		protected override void pointersUpdated(IList<Pointer> pointers)
		{
			gestureSampler.Begin();

			base.pointersUpdated(pointers);

			gestureSampler.End();
		}

        /// <inheritdoc />
        protected override void pointersReleased(IList<Pointer> pointers)
        {
			gestureSampler.Begin();

            base.pointersReleased(pointers);

#if TOUCHSCRIPT_DEBUG
            if (NumPointers == 0) clearDebug();
            else drawDebug(activePointers[0].ProjectionParams.ProjectFrom(cachedTransform.position), activePointers[0].Position);
#endif

			gestureSampler.End();
        }

        #endregion

        #region Protected methods

        /// <inheritdoc />
        protected override float doRotation(Vector3 center, Vector2 oldScreenPos, Vector2 newScreenPos,
                                 ProjectionParams projectionParams)
        {
            var newVector = projectionParams.ProjectTo(newScreenPos, TransformPlane) - center;
            var oldVector = projectionParams.ProjectTo(oldScreenPos, TransformPlane) - center;
            var angle = Vector3.Angle(oldVector, newVector);
            if (Vector3.Dot(Vector3.Cross(oldVector, newVector), TransformPlane.normal) < 0)
                angle = -angle;
            return angle;
        }

        /// <inheritdoc />
        protected override float doScaling(Vector3 center, Vector2 oldScreenPos, Vector2 newScreenPos,
                                ProjectionParams projectionParams)
        {
            var newVector = projectionParams.ProjectTo(newScreenPos, TransformPlane) - center;
            var oldVector = projectionParams.ProjectTo(oldScreenPos, TransformPlane) - center;
            return newVector.magnitude / oldVector.magnitude;
        }

#if TOUCHSCRIPT_DEBUG
        protected override void clearDebug()
        {
            base.clearDebug();

            GLDebug.RemoveFigure(debugID + 3);
        }

        protected override void drawDebug(Vector2 point1, Vector2 point2)
        {
            base.drawDebug(point1, point2);

            if (!DebugMode) return;
            GLDebug.DrawPlaneWithNormal(debugID + 3, cachedTransform.position, RotationAxis, 1f, GLDebug.MULTIPLY, float.PositiveInfinity);
        }
#endif

        #endregion

        #region Private functions

        private void updateProjectionPlane()
        {
            if (!Application.isPlaying) return;

            switch (projection)
            {
                case TransformGesture.ProjectionType.Layer:
                    if (projectionLayer == null)
                        transformPlane = new Plane(cachedTransform.TransformDirection(Vector3.forward),
                            cachedTransform.position);
                    else transformPlane = new Plane(projectionLayer.WorldProjectionNormal, cachedTransform.position);
                    break;
                case TransformGesture.ProjectionType.Object:
                    transformPlane = new Plane(cachedTransform.TransformDirection(projectionPlaneNormal),
                        cachedTransform.position);
                    break;
                case TransformGesture.ProjectionType.Global:
                    transformPlane = new Plane(projectionPlaneNormal, cachedTransform.position);
                    break;
            }

            rotationAxis = transformPlane.normal;
        }

        #endregion
    }
}