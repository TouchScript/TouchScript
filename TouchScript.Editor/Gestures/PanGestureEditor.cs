/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using TouchScript.Editor.Gestures.Simple;
using TouchScript.Gestures;
using UnityEditor;

namespace TouchScript.Editor.Gestures
{
    [CustomEditor(typeof(PanGesture))]
    public class PanGestureEditor : SimplePanGestureEditor
    {}
}