/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using TouchScript.Hit;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace TouchScript.Layers
{
	[AddComponentMenu("TouchScript/Layers/UI Overlay Layer")]
	public class UIOverlayLayer : TouchLayer
	{

		private static UIOverlayLayer instance;
		private static readonly Comparison<RaycastResult> raycastComparer = raycastComparerFunc;

		private PropertyInfo canvasProp;
		private List<BaseRaycaster> raycasters;
		private List<HitTest> tmpHitTestList = new List<HitTest>(10);
		private List<RaycastResult> graphicList = new List<RaycastResult>(20);

		#region Public methods

		/// <inheritdoc />
		public override LayerHitResult Hit(Vector2 position, out TouchHit hit)
		{
			if (base.Hit(position, out hit) == LayerHitResult.Miss) return LayerHitResult.Miss;

			graphicList.Clear();
			var count = raycasters.Count;
			for (var i = 0; i < count; i++)
			{
				var raycaster = raycasters[i] as GraphicRaycaster;
				if (raycaster == null) continue;
				var canvas = canvasProp.GetValue(raycaster, null) as Canvas;
				if (canvas == null) continue;
				if (canvas.renderMode != RenderMode.ScreenSpaceOverlay) continue;

				var foundGraphics = GraphicRegistry.GetGraphicsForCanvas(canvas);
				var count2 = foundGraphics.Count;
				for (int j = 0; j < count2; j++)
				{
					Graphic graphic = foundGraphics[j];
					
					// -1 means it hasn't been processed by the canvas, which means it isn't actually drawn
					if (graphic.depth == -1 || !graphic.raycastTarget)
						continue;
					
					if (!RectTransformUtility.RectangleContainsScreenPoint(graphic.rectTransform, position))
						continue;
					
					if (graphic.Raycast(position, null)) graphicList.Add(
						new RaycastResult() { Graphic = graphic, Raycaster = raycaster, SortingLayer = canvas.sortingLayerID, 
						SortingOrder = canvas.sortingOrder, Depth = graphic.depth, Index = graphicList.Count });
				}
			}

			count = graphicList.Count;
			if (count == 0) return LayerHitResult.Miss;
			if (count > 1)
			{
				graphicList.Sort(raycastComparerFunc);
				for (var i = 0; i < count; ++i)
				{
					var raycastHit = graphicList[i];
					switch (doHit(raycastHit, out hit))
					{
					case HitTest.ObjectHitResult.Hit:
						return LayerHitResult.Hit;
					case HitTest.ObjectHitResult.Discard:
						return LayerHitResult.Miss;
					}
				}
			}
			else
			{
				switch (doHit(graphicList[0], out hit))
				{
				case HitTest.ObjectHitResult.Hit:
					return LayerHitResult.Hit;
				case HitTest.ObjectHitResult.Error:
					return LayerHitResult.Error;
				default:
					return LayerHitResult.Miss;
				}
			}
			
			return LayerHitResult.Miss;
		}
		
		#endregion

		#region Unity methods

		/// <inheritdoc />
		protected override void Awake()
		{
			if (Application.isPlaying)
			{
				if (instance == null) instance = this;
				if (instance != this)
				{
					Debug.LogWarning("[TouchScript] Only one instance of ScreenSpaceUILayer should exist in a scene. Destroying.");
					Destroy(this);
					return;
				}
			}
			
			base.Awake();
			if (!Application.isPlaying) return;

			raycasters = Type.GetType(System.Reflection.Assembly.CreateQualifiedName("UnityEngine.UI", "UnityEngine.EventSystems.RaycasterManager")).
				GetField("s_Raycasters", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null) as List<BaseRaycaster>;
			canvasProp = typeof(GraphicRaycaster).GetProperty("canvas", BindingFlags.NonPublic | BindingFlags.Instance);
		}

		#endregion

		#region Protected functions
		
		/// <inheritdoc />
		protected override void setName()
		{
			Name = "UI Overlay Layer";
		}
		
		#endregion
		
		#region Private functions

		private static int raycastComparerFunc(RaycastResult lhs, RaycastResult rhs)
		{
			if (lhs.Raycaster != rhs.Raycaster)
			{
				if (lhs.Raycaster.sortOrderPriority != rhs.Raycaster.sortOrderPriority)
					return rhs.Raycaster.sortOrderPriority.CompareTo(lhs.Raycaster.sortOrderPriority);
				
				if (lhs.Raycaster.renderOrderPriority != rhs.Raycaster.renderOrderPriority)
					return rhs.Raycaster.renderOrderPriority.CompareTo(lhs.Raycaster.renderOrderPriority);
			}
			
			if (lhs.SortingLayer != rhs.SortingLayer)
			{
				// Uses the layer value to properly compare the relative order of the layers.
				var rid = SortingLayer.GetLayerValueFromID(rhs.SortingLayer);
				var lid = SortingLayer.GetLayerValueFromID(lhs.SortingLayer);
				return rid.CompareTo(lid);
			}
			
			if (lhs.SortingOrder != rhs.SortingOrder)
				return rhs.SortingOrder.CompareTo(lhs.SortingOrder);

			if (lhs.Depth != rhs.Depth)
				return rhs.Depth.CompareTo(lhs.Depth);
			
			return lhs.Index.CompareTo(rhs.Index);
		}

		private HitTest.ObjectHitResult doHit(RaycastResult raycastHit, out TouchHit hit)
		{
			hit = new TouchHit(raycastHit.Graphic.transform);
			
			var go = raycastHit.Graphic.gameObject;
			if (go == null) return HitTest.ObjectHitResult.Miss;
			go.GetComponents(tmpHitTestList);
			var count = tmpHitTestList.Count;
			if (count == 0) return HitTest.ObjectHitResult.Hit;
			
			var hitResult = HitTest.ObjectHitResult.Hit;
			for (var i = 0; i < count; i++)
			{
				var test = tmpHitTestList[i];
				if (!test.enabled) continue;
				hitResult = test.IsHit(hit);
				if (hitResult == HitTest.ObjectHitResult.Miss || hitResult == HitTest.ObjectHitResult.Discard) break;
			}
			return hitResult;
		}

		#endregion

		private struct RaycastResult
		{
			public Graphic Graphic;
			public GraphicRaycaster Raycaster;
			public int SortingLayer;
			public int SortingOrder;
			public int Depth;
			public int Index;
		}

	}
}

