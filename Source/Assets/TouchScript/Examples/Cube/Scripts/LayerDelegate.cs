using UnityEngine;
using TouchScript.Layers;

namespace TouchScript.Examples.Cube
{
    public class LayerDelegate : MonoBehaviour, ILayerDelegate
    {

        public RedirectInput Source;
        public TouchLayer RenderTextureLayer;

        public bool ShouldReceiveTouch(TouchLayer layer, TouchPoint touch)
        {
            if (layer == RenderTextureLayer)
                return touch.InputSource == Source;
            return touch.InputSource != Source;
        }
    }
}
