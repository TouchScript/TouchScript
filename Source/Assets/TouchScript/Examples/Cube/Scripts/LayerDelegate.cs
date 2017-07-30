/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using UnityEngine;
using TouchScript.Layers;
using TouchScript.Pointers;
using TouchScript.InputSources;

namespace TouchScript.Examples.Cube
{
    /// <exclude />
    public class LayerDelegate : MonoBehaviour, ILayerDelegate
    {

        public RedirectInput Source;
        public TouchLayer RenderTextureLayer;

        public bool ShouldReceivePointer(TouchLayer layer, IPointer pointer)
        {
            if (layer == RenderTextureLayer)
				return pointer.InputSource == (IInputSource)Source;
			return pointer.InputSource != (IInputSource)Source;
        }
    }
}
