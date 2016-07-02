using UnityEngine;
using TouchScript.Layers;

namespace TouchScript.Examples.Cube
{
    public class LayerDelegate : MonoBehaviour, ILayerDelegate
    {

        public RedirectInput Source;
        public TouchLayer RenderTextureLayer;

        public bool ShouldReceivePointer(TouchLayer layer, Pointer pointer)
        {
            if (layer == RenderTextureLayer)
                return pointer.InputSource == Source;
            return pointer.InputSource != Source;
        }
    }
}
