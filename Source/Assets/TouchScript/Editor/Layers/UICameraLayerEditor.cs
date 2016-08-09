/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using TouchScript.Layers;
using UnityEditor;

namespace TouchScript.Editor.Layers
{
    [CustomEditor(typeof (UICameraLayer))]
    internal sealed class UICameraLayerEditor : UnityEditor.Editor
    {
        private void OnEnable() {}

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }
    }
}