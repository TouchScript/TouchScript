/*
 * Copyright (C) 2012 Interactive Lab
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation  * files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy,  * modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the  * Software is furnished to do so, subject to the following conditions:
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the  * Software.
 *  * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE  * WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR  * COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR  * OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using System;
using System.Collections.Generic;
using TouchScript.Events;
using UnityEngine;

namespace TouchScript.Debugging {
	/// <summary>
	/// Visual debugger to show touches as GUI elements.
	/// </summary>
	[AddComponentMenu("TouchScript/Touch Debugger")]
	public class TouchDebugger : MonoBehaviour {
		#region Unity fields

		/// <summary>
		/// Texture to use
		/// </summary>
		public Texture2D TouchTexture;

		/// <summary>
		/// Font color for touch ids
		/// </summary>
		public Color FontColor;

		#endregion

		#region Private variables

		private Dictionary<int, TouchPoint> dummies = new Dictionary<int, TouchPoint>();

		#endregion

		#region Unity

		private void Start() {
			if (camera == null) throw new Exception("A camera is required.");

			if (TouchManager.Instance != null) {
				TouchManager.Instance.TouchesBegan += OnTouchesBegan;
				TouchManager.Instance.TouchesEnded += OnTouchesEnded;
				TouchManager.Instance.TouchesMoved += OnTouchesMoved;
				TouchManager.Instance.TouchesCancelled += OnTouchesCancelled;
			}
		}

		private void Update() {
			camera.orthographicSize = Screen.height*.5f;
		}

		private void OnGUI() {
			if (TouchTexture == null) return;

			GUI.color = FontColor;

			foreach (KeyValuePair<int, TouchPoint> dummy in dummies) {
				var x = dummy.Value.Position.x;
				var y = Screen.height - dummy.Value.Position.y;
				GUI.DrawTexture(new Rect(x - TouchTexture.width/2, y - TouchTexture.height/2, TouchTexture.width, TouchTexture.height), TouchTexture, ScaleMode.ScaleToFit);
				GUI.Label(new Rect(x + TouchTexture.width, y - 9, 60, 25), dummy.Value.Id.ToString());
			}
		}

		private void OnDestroy() {
			if (TouchManager.Instance != null) {
				TouchManager.Instance.TouchesBegan -= OnTouchesBegan;
				TouchManager.Instance.TouchesEnded -= OnTouchesEnded;
				TouchManager.Instance.TouchesMoved -= OnTouchesMoved;
				TouchManager.Instance.TouchesCancelled -= OnTouchesCancelled;
			}
		}

		#endregion

		#region Private functions

		private void updateDummy(TouchPoint dummy) {
			dummies[dummy.Id] = dummy;
		}

		#endregion

		#region Event handlers

		private void OnTouchesBegan(object sender, TouchEventArgs e) {
			if (!enabled) return;

			foreach (var touchPoint in e.TouchPoints) {
				dummies.Add(touchPoint.Id, touchPoint);
			}
		}

		private void OnTouchesMoved(object sender, TouchEventArgs e) {
			if (!enabled) return;

			foreach (var touchPoint in e.TouchPoints) {
				TouchPoint dummy;
				if (!dummies.TryGetValue(touchPoint.Id, out dummy)) return;
				updateDummy(touchPoint);
			}
		}

		private void OnTouchesEnded(object sender, TouchEventArgs e) {
			if (!enabled) return;

			foreach (var touchPoint in e.TouchPoints) {
				TouchPoint dummy;
				if (!dummies.TryGetValue(touchPoint.Id, out dummy)) return;
				dummies.Remove(touchPoint.Id);
			}
		}

		private void OnTouchesCancelled(object sender, TouchEventArgs e) {
			OnTouchesEnded(sender, e);
		}

		#endregion
	}
}