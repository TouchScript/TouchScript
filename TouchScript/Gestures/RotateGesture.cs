/*
 * Copyright (C) 2012 Interactive Lab
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation  * files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy,  * modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the  * Software is furnished to do so, subject to the following conditions:
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the  * Software.
 *  * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE  * WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR  * COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR  * OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using System;
using System.Collections.Generic;
using TouchScript.Clusters;
using TouchScript.Utils;
using UnityEngine;

namespace TouchScript.Gestures {
	/// <summary>
	/// Recognizes rotation gesture.
	/// </summary>
	[AddComponentMenu("TouchScript/Gestures/Rotate Gesture")]
	public class RotateGesture : TwoClusterTransform2DGestureBase {
		#region Private variables

		private float rotationBuffer;
		private bool isRotating = false;

		#endregion

		#region Public properties

		/// <summary>
		/// Minimum rotation in degrees to be considered as a possible gesture.
		/// </summary>
		[SerializeField]
		public float RotationThreshold { get; set; }

		/// <summary>
		/// Contains local rotation when gesture is recognized.
		/// </summary>
		public float LocalDeltaRotation { get; private set; }

		#endregion

		public RotateGesture() : base() {
			RotationThreshold = 3;
			MinClusterDistance = .5f;
		}

		#region Gesture callbacks

		protected override void touchesMoved(IList<TouchPoint> touches) {
			base.touchesMoved(touches);

			if (!clusters.HasClusters) return;

			Vector3 oldGlobalCenter3DPos, oldLocalCenter3DPos, newGlobalCenter3DPos, newLocalCenter3DPos;
			var deltaRotation = 0f;

			updateProjectionCamera(Cluster.GetClusterCamera(activeTouches));

			var old2DPos1 = clusters.GetPreviousCenterPosition(Cluster2.CLUSTER1);
			var old2DPos2 = clusters.GetPreviousCenterPosition(Cluster2.CLUSTER2);
			var new2DPos1 = clusters.GetCenterPosition(Cluster2.CLUSTER1);
			var new2DPos2 = clusters.GetCenterPosition(Cluster2.CLUSTER2);
			var old3DPos1 = ProjectionUtils.CameraToPlaneProjection(old2DPos1, projectionCamera, WorldTransformPlane);
			var old3DPos2 = ProjectionUtils.CameraToPlaneProjection(old2DPos2, projectionCamera, WorldTransformPlane);
			var new3DPos1 = ProjectionUtils.CameraToPlaneProjection(new2DPos1, projectionCamera, WorldTransformPlane);
			var new3DPos2 = ProjectionUtils.CameraToPlaneProjection(new2DPos2, projectionCamera, WorldTransformPlane);
			var newVector = new3DPos2 - new3DPos1;
			var oldVector = old3DPos2 - old3DPos1;

			Vector2 oldCenter2DPos = (old2DPos1 + old2DPos2)*.5f;
			Vector2 newCenter2DPos = (new2DPos1 + new2DPos2)*.5f;

			var angle = Vector3.Angle(oldVector, newVector);
			if (Vector3.Dot(Vector3.Cross(oldVector, newVector), WorldTransformPlane.normal) < 0) angle = -angle;
			if (isRotating) {
				deltaRotation = angle;
			} else {
				rotationBuffer += angle;
				if (rotationBuffer*rotationBuffer >= RotationThreshold*RotationThreshold) {
					isRotating = true;
					deltaRotation = rotationBuffer;
				}
			}

			oldGlobalCenter3DPos = ProjectionUtils.CameraToPlaneProjection(oldCenter2DPos, projectionCamera, WorldTransformPlane);
			newGlobalCenter3DPos = ProjectionUtils.CameraToPlaneProjection(newCenter2DPos, projectionCamera, WorldTransformPlane);
			oldLocalCenter3DPos = globalToLocalPosition(oldGlobalCenter3DPos);
			newLocalCenter3DPos = globalToLocalPosition(newGlobalCenter3DPos);

			if (Math.Abs(deltaRotation) > 0.00001) {
				switch (State) {
					case GestureState.Possible:
					case GestureState.Began:
					case GestureState.Changed:
						screenPosition = newCenter2DPos;
						previousScreenPosition = oldCenter2DPos;
						PreviousWorldTransformCenter = oldGlobalCenter3DPos;
						WorldTransformCenter = newGlobalCenter3DPos;
						PreviousWorldTransformCenter = oldGlobalCenter3DPos;
						LocalTransformCenter = newLocalCenter3DPos;
						PreviousLocalTransformCenter = oldLocalCenter3DPos;

						LocalDeltaRotation = deltaRotation;

						if (State == GestureState.Possible) {
							setState(GestureState.Began);
						} else {
							setState(GestureState.Changed);
						}
						break;
				}
			}
		}

		protected override void reset() {
			base.reset();
			LocalDeltaRotation = 0f;
			rotationBuffer = 0f;
			isRotating = false;
		}

		#endregion
	}
}