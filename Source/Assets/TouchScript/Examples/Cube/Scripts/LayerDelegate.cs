/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using UnityEngine;
using TouchScript.Layers;
using TouchScript.Pointers;

namespace TouchScript.Examples.Cube
{
    public class LayerDelegate : MonoBehaviour, ILayerDelegate
    {

        public RedirectInput Source;
        public TouchLayer RenderTextureLayer;

        public bool ShouldReceivePointer(TouchLayer layer, IPointer pointer)
        {
            if (layer == RenderTextureLayer)
                return pointer.InputSource == Source;
            return pointer.InputSource != Source;
        }
    }
}
