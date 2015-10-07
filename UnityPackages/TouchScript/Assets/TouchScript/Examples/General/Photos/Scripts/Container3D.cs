using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TouchScript.Gestures;

namespace TouchScript.Examples.Photos 
{
	public class Container3D : MonoBehaviour 
	{
		private List<Transform> children;

		private void Start() 
		{
			var count = transform.childCount;
			children = new List<Transform>(count);
			for (var i = 0; i < count; i++)
			{
				var child = transform.GetChild(i);
				children.Add(child);
				var pressGesture = child.GetComponent<PressGesture>();
				if (pressGesture != null) pressGesture.Pressed += pressedHandler;
			}
			sortChildren();
		}

		private void sortChildren()
		{
			var count = children.Count;
			for (var i = 0; i < count; i++)
			{
				var child = children[i];
				child.localPosition = new Vector3(child.localPosition.x, child.localPosition.y, -i * 0.003f);
			}
		}

		private void pressedHandler(object sender, System.EventArgs e)
		{
			var child = (sender as Gesture).transform;
			children.Remove(child);
			children.Add(child);
			sortChildren();
		}
	}
}