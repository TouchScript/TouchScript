/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System.Collections.Generic;
using TouchScript.Layers.Base;
using TouchScript.Pointers;
using UnityEngine;

namespace TouchScript.Layers
{
    /// <summary>
    /// Pointer layer which handles Unity UI and interface objects in a Canvas.
    /// </summary>
    [AddComponentMenu("TouchScript/Layers/UI Layer")]
    [HelpURL("http://touchscript.github.io/docs/html/T_TouchScript_Layers_UILayer.htm")]
    public class UICameraLayer : UILayerBase
    {
        #region Private variables

        private Dictionary<int, ProjectionParams> projectionParamsCache = new Dictionary<int, ProjectionParams>();

        #endregion

        #region Public methods

        /// <inheritdoc />
        public override ProjectionParams GetProjectionParams(Pointer pointer)
        {
            var graphic = pointer.GetPressData().RaycastHitUI.Graphic;
            if (graphic == null) return layerProjectionParams;
            var canvas = graphic.canvas;
            if (canvas == null) return layerProjectionParams;

            ProjectionParams pp;
            if (!projectionParamsCache.TryGetValue(canvas.GetInstanceID(), out pp))
            {
                // TODO: memory leak
                pp = new WorldSpaceCanvasProjectionParams(canvas);
                projectionParamsCache.Add(canvas.GetInstanceID(), pp);
            }
            return pp;
        }

        #endregion

        #region Protected functions

        /// <inheritdoc />
        protected override void setName()
        {
            Name = "UI Camera Layer";
        }

        protected override bool filterCanvas(Canvas canvas)
        {
            return canvas == null || canvas.renderMode == RenderMode.ScreenSpaceOverlay;
        }

        #endregion
    }
}