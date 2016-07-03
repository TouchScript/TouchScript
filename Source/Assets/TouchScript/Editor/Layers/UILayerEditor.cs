/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using TouchScript.Layers;
using UnityEditor;

namespace TouchScript.Editor.Layers
{
    [CustomEditor(typeof (UILayer))]
    internal sealed class UILayerEditor : UnityEditor.Editor
    {
        private void OnEnable() {}

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }
    }
}