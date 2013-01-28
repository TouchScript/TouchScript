/*
 * Copyright (C) 2012 Interactive Lab
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation 
 * files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, 
 * modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the 
 * Software is furnished to do so, subject to the following conditions:
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the 
 * Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE 
 * WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR 
 * COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR 
 * OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using System.Collections.Generic;
using TouchScript.Clusters;
using UnityEngine;

namespace TouchScript.Gestures {
	public class TwoClusterTransform2DGestureBase : Transform2DGestureBase {
		protected Cluster2 clusters = new Cluster2();
		protected Vector2 screenPosition;
		protected Vector3 previousScreenPosition;

		/// <summary>
		/// Minimum distance between clusters in cm for gesture to be recognized.
		/// </summary>
		[SerializeField]
		public float MinClusterDistance { get; set; }

		public override Vector2 ScreenPosition {
			get { return screenPosition; }
		}

		public override Vector2 PreviousScreenPosition {
			get { return previousScreenPosition; }
		}

		public TwoClusterTransform2DGestureBase() : base() {
			MinClusterDistance = .5f;
		}

		protected override void touchesBegan(IList<TouchPoint> touches) {
			base.touchesBegan(touches);
			clusters.AddPoints(touches);
		}

		protected override void touchesMoved(IList<TouchPoint> touches) {
			base.touchesMoved(touches);

			clusters.Invalidate();
			clusters.MinPointsDistance = MinClusterDistance*TouchManager.Instance.DotsPerCentimeter;
		}

		protected override void touchesEnded(IList<TouchPoint> touches) {
			clusters.RemovePoints(touches);
			if ((State == GestureState.Began || State == GestureState.Changed) && !clusters.HasClusters) {
				setState(GestureState.Ended);
			}
		}

		protected override void reset() {
			base.reset();
			clusters.RemoveAllPoints();
		}
	}
}