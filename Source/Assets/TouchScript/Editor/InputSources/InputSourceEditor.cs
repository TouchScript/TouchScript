/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using TouchScript.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace TouchScript.Editor.InputSources
{
    public class InputSourceEditor : UnityEditor.Editor
    {
        protected virtual void OnEnable()
        {
        }

        public override void OnInspectorGUI()
        {
        }

        protected virtual void drawAdvanced() {}
    }
}
