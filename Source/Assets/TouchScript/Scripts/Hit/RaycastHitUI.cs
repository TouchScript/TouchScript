/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TouchScript.Hit
{
	public struct RaycastHitUI
	{

		public GameObject GameObject;
		public BaseRaycaster Raycaster;
		public float GraphicIndex;
		public int Depth;
		public int SortingLayer;
		public int SortingOrder;
		public Graphic Graphic;
		public Vector3 WorldPosition;
		public Vector3 WorldNormal;

	}
}

