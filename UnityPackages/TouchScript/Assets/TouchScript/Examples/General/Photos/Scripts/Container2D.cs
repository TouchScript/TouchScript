using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TouchScript.Gestures;

namespace TouchScript.Examples.Photos 
{
	public class Container2D : MonoBehaviour 
	{
		private List<SpriteRenderer> children;
		
		private void Start() 
		{
			var count = transform.childCount;
			children = new List<SpriteRenderer>(count);
			for (var i = 0; i < count; i++)
			{
				var child = transform.GetChild(i).GetComponent<SpriteRenderer>();
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
				child.sortingOrder = i;
			}
		}
		
		private void pressedHandler(object sender, System.EventArgs e)
		{
			var child = (sender as Gesture).GetComponent<SpriteRenderer>();
			children.Remove(child);
			children.Add(child);
			sortChildren();
		}
	}
}