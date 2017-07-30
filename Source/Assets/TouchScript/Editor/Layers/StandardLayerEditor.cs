/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using TouchScript.Editor.EditorUI;
using TouchScript.Layers;
using UnityEditor;
using UnityEngine;

namespace TouchScript.Editor.Layers
{
    [CustomEditor(typeof(StandardLayer), true)]
    internal class StandardLayerEditor : UnityEditor.Editor
    {
		public static readonly GUIContent TEXT_ADVANCED_HEADER = new GUIContent("Advanced", "Advanced properties.");
		public static readonly GUIContent TEXT_HIT_HEADER = new GUIContent("Hit test options", "Options which control what types of objects this layer should search under pointers.");

		public static readonly GUIContent TEXT_3D_OBJECTS = new GUIContent("Hit 3D Objects", "Layer should raycast 3D objects.");
		public static readonly GUIContent TEXT_2D_OBJECTS = new GUIContent("Hit 2D Objects", "Layer should raycast 2D objects.");
		public static readonly GUIContent TEXT_WORLD_UI = new GUIContent("Hit World UI", "Layer should raycast World Space UI.");
		public static readonly GUIContent TEXT_SS_UI = new GUIContent("Hit Screen Space UI", "Layer should raycast Screen Space UI.");
		public static readonly GUIContent TEXT_LAYER_MASK = new GUIContent("Layer Mask", "Layer mask.");
		public static readonly GUIContent TEXT_HIT_FILTERS = new GUIContent("Use Hit FIlters", "Layer should test for individual HitTest objects.");

		public static readonly GUIContent TEXT_HELP = new GUIContent("This component assigns target GameObjects in the scene for pressed pointers.");

		private SerializedProperty advancedProps, hitProps;
        private SerializedProperty basicEditor;
        private SerializedProperty hit3DObjects;
        private SerializedProperty hit2DObjects;
        private SerializedProperty hitWorldSpaceUI;
        private SerializedProperty hitScreenSpaceUI;
        private SerializedProperty layerMask;
        private SerializedProperty useHitFilters;

        protected virtual void OnEnable()
        {
            hideFlags = HideFlags.HideAndDontSave;

            advancedProps = serializedObject.FindProperty("advancedProps");
            hitProps = serializedObject.FindProperty("hitProps");
            basicEditor = serializedObject.FindProperty("basicEditor");
            hit3DObjects = serializedObject.FindProperty("hit3DObjects");
            hit2DObjects = serializedObject.FindProperty("hit2DObjects");
            hitWorldSpaceUI = serializedObject.FindProperty("hitWorldSpaceUI");
            hitScreenSpaceUI = serializedObject.FindProperty("hitScreenSpaceUI");
            layerMask = serializedObject.FindProperty("layerMask");
            useHitFilters = serializedObject.FindProperty("useHitFilters");
        }

        public override void OnInspectorGUI()
        {
#if UNITY_5_6_OR_NEWER
			serializedObject.UpdateIfRequiredOrScript();
#else
			serializedObject.UpdateIfDirtyOrScript();
#endif

            GUILayout.Space(5);

			if (basicEditor.boolValue)
			{
				drawHit();

				if (GUIElements.BasicHelpBox(TEXT_HELP))
				{
					basicEditor.boolValue = false;
					Repaint();
				}
			}
			else
			{
                drawHit();
                drawAdvanced();
			}

            serializedObject.ApplyModifiedProperties();
        }

        private void drawHit()
        {
			var display = GUIElements.Header(TEXT_HIT_HEADER, hitProps);
			if (display)
			{
				EditorGUI.indentLevel++;
				doDrawHit();
				EditorGUI.indentLevel--;
			}
        }

        protected virtual void doDrawHit()
		{
			EditorGUILayout.PropertyField(hitScreenSpaceUI, TEXT_SS_UI);
			EditorGUILayout.PropertyField(hit3DObjects, TEXT_3D_OBJECTS);
			EditorGUILayout.PropertyField(hit2DObjects, TEXT_2D_OBJECTS);
			EditorGUILayout.PropertyField(hitWorldSpaceUI, TEXT_WORLD_UI);
			EditorGUILayout.PropertyField(layerMask, TEXT_LAYER_MASK);
		}

        private void drawAdvanced()
        {
			var display = GUIElements.Header(TEXT_ADVANCED_HEADER, advancedProps);
			if (display)
			{
				EditorGUI.indentLevel++;
				doDrawAdvanced();
				EditorGUI.indentLevel--;
			}
        }

        protected virtual void doDrawAdvanced()
        {
            EditorGUILayout.PropertyField(useHitFilters, TEXT_HIT_FILTERS);
        }

    }
}
