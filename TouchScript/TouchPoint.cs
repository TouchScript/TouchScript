/*
 * Copyright (C) 2012 Interactive Lab
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation  * files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy,  * modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the  * Software is furnished to do so, subject to the following conditions:
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the  * Software.
 *  * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE  * WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR  * COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR  * OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using UnityEngine;

namespace TouchScript {
	/// <summary>
	/// Touch point.
	/// </summary>
	public class TouchPoint {
		public TouchPoint(int id, Vector2 position) {
			Id = id;
			Position = position;
			PreviousPosition = position;
		}

		/// <summary>
		/// Internal unique touch point id.
		/// </summary>
		public int Id { get; private set; }

		private Vector2 position = Vector2.zero;

		/// <summary>
		/// Current touch position.
		/// </summary>
		public Vector2 Position {
			get { return position; }
			set {
				PreviousPosition = position;
				position = value;
			}
		}

		/// <summary>
		///Previous position.
		/// </summary>
		public Vector2 PreviousPosition { get; private set; }

		/// <summary>
		/// Original hit target.
		/// </summary>
		public Transform Target { get; internal set; }

		/// <summary>
		/// Original hit information.
		/// </summary>
		public RaycastHit Hit { get; internal set; }

		/// <summary>
		/// Original camera through which the target was seen.
		/// </summary>
		public Camera HitCamera { get; internal set; }
	}
}