/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TouchScript.Hit
{
    /// <exclude />
    public struct RaycastHitUI
    {
        public Transform Target;
        public BaseRaycaster Raycaster;
        public int GraphicIndex;
        public int Depth;
        public int SortingLayer;
        public int SortingOrder;
        public Graphic Graphic;
        public Vector3 WorldPosition;
        public Vector3 WorldNormal;
        public float Distance;
    }
}