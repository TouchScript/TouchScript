/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using TouchScript.Gestures.Simple;
using UnityEditor;
using UnityEngine;

namespace TouchScript.Editor.Gestures.Simple
{
    [CustomEditor(typeof(TwoPointTransform2DGestureBase), true)]
    internal class TwoPointTransform2DGestureBaseEditor : MultiPointTransform2DGestureBaseEditor
    {
        protected override void OnEnable()
        {
            base.OnEnable();

            showMinPointsCount = false;
        }
        
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }
    }
}