using TouchScript.Gestures;
using UnityEditor;

namespace TouchScript.Editor.Gestures
{

    [CustomEditor(typeof(TapGesture))]
    public class TapGestureEditor : GestureEditor
    {

        public override void OnInspectorGUI()
        {
            EditorGUIUtility.LookLikeInspector();

            base.OnInspectorGUI();
        }

    }
}
