/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using TouchScript.Gestures;
using UnityEditor;

namespace TouchScript.Editor.Gestures
{
    public class Transform2DGestureBaseEditor : GestureEditor
    {
        public override void OnInspectorGUI()
        {
            var instance = target as Transform2DGestureBase;

            serializedObject.Update();
            EditorGUIUtility.LookLikeControls();
            instance.Projection = (Transform2DGestureBase.ProjectionType)EditorGUILayout.EnumPopup("Projection Type", instance.Projection);
            switch (instance.Projection)
            {
                case Transform2DGestureBase.ProjectionType.Camera:
                    EditorGUILayout.HelpBox("Object is moving on a plane through its pivot point parallel to camera screen.", MessageType.Info, true);
                    break;
                case Transform2DGestureBase.ProjectionType.Global:
                    EditorGUILayout.HelpBox("Object is moving on a plane through its pivot point with given normal vector in global space.", MessageType.Info, true);
                    break;
                case Transform2DGestureBase.ProjectionType.Local:
                    EditorGUILayout.HelpBox("Object is moving on a plane through its pivot point with given normal vector in local space.", MessageType.Info, true);
                    break;
            }
            if (instance.Projection != Transform2DGestureBase.ProjectionType.Camera)
            {
                instance.ProjectionNormal = EditorGUILayout.Vector3Field("Projection Normal", instance.ProjectionNormal);
            }
            serializedObject.ApplyModifiedProperties();
            base.OnInspectorGUI();
        }
    }
}