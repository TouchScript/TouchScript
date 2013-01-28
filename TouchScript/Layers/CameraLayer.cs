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
using TouchScript.Hit;
using UnityEngine;

namespace TouchScript.Layers {
	[AddComponentMenu("TouchScript/Layers/Camera Layer")]
	public class CameraLayer : LayerBase {
		public override HitResult Hit(Vector2 position, out RaycastHit hit, out Camera hitCamera) {
			hit = new RaycastHit();
			hitCamera = null;

			if (camera == null) return HitResult.Error;

			var ray = camera.ScreenPointToRay(new Vector3(position.x, position.y, camera.nearClipPlane));
			var hits = Physics.RaycastAll(ray);

			if (hits.Length == 0) return HitResult.Miss;
			if (hits.Length > 1) {
				var cameraPos = camera.transform.position;
				var sortedHits = new List<RaycastHit>(hits);
				sortedHits.Sort((a, b) => {
									var distA = (a.point - cameraPos).sqrMagnitude;
									var distB = (b.point - cameraPos).sqrMagnitude;
									return distA < distB ? -1 : 1;
								});
				hits = sortedHits.ToArray();
			}

			var success = false;
			foreach (var raycastHit in hits) {
				var hitTests = raycastHit.transform.GetComponents<HitTest>();
				if (hitTests.Length == 0) {
					hit = raycastHit;
					success = true;
					break;
				}

				var allowed = true;
				foreach (var test in hitTests) {
					allowed = test.IsHit(raycastHit);
					if (!allowed) break;
				}
				if (allowed) {
					hit = raycastHit;
					success = true;
					break;
				}
			}

			if (success) {
				hitCamera = camera;
				return HitResult.Hit;
			}

			return HitResult.Miss;
		}
	}
}
