/*
 * Copyright (C) 2012 Interactive Lab
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation  * files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy,  * modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the  * Software is furnished to do so, subject to the following conditions:
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the  * Software.
 *  * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE  * WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR  * COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR  * OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using UnityEngine;

namespace TouchScript.InputSources {
	/// <summary>
	/// Input source to grab mouse presses as touch points.
	/// </summary>
	[AddComponentMenu("TouchScript/Input Sources/Mouse Input")]
	public class MouseInput : InputSource {
		#region Private variables

		private int mousePointId = -1;
		private Vector3 mousePointPos = Vector3.zero;

		#endregion

		#region Unity

		protected override void Update() {
			base.Update();

			var upHandled = false;
			if (Input.GetMouseButtonUp(0)) {
				if (mousePointId != -1) {
					endTouch(mousePointId);
					mousePointId = -1;
					upHandled = true;
				}
			}

			if (Input.GetMouseButtonDown(0)) {
				var pos = Input.mousePosition;
				mousePointId = beginTouch(new Vector2(pos.x, pos.y));
			} else if (Input.GetMouseButton(0)) {
				var pos = Input.mousePosition;
				if (mousePointPos != pos) {
					mousePointPos = pos;
					moveTouch(mousePointId, new Vector2(pos.x, pos.y));
				}
			}

			if (Input.GetMouseButtonUp(0) && !upHandled) {
				endTouch(mousePointId);
				mousePointId = -1;
			}
		}

		#endregion
	}
}