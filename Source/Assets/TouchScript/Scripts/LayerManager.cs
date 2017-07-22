/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using UnityEngine;

namespace TouchScript
{
    public sealed class LayerManager : MonoBehaviour
    {
        public static ILayerManager Instance
        {
            get { return LayerManagerInstance.Instance; }
        }
    }
}
