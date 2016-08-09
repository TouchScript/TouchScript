/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using TouchScript.Layers.Base;
using UnityEngine;

namespace TouchScript.Layers
{
    [AddComponentMenu("TouchScript/Layers/UI Overlay Layer")]
    public class UIOverlayLayer : UILayerBase
    {
        #region Protected functions

        /// <inheritdoc />
        protected override void setName()
        {
            Name = "UI Overlay Layer";
        }

        protected override bool filterCanvas(Canvas canvas)
        {
            return canvas == null || canvas.renderMode != RenderMode.ScreenSpaceOverlay;
        }

        #endregion
    }
}