/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using TouchScript.Editor.Gestures.Abstract;
using TouchScript.Gestures;
using UnityEditor;

namespace TouchScript.Editor.Gestures
{
    [CustomEditor(typeof(ScreenTransformGesture), true)]
    internal class ScreenTransformGestureEditor : AbstractTransformGestureEditor
    {
    }
}
