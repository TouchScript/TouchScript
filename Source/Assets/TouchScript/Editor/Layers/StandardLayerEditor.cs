/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using TouchScript.Editor.Utils;
using TouchScript.Layers;
using UnityEditor;
using UnityEngine;

namespace TouchScript.Editor.Layers
{
    [CustomEditor(typeof(StandardLayer), true)]
    internal class StandardLayerEditor : UnityEditor.Editor
    {
        private static readonly GUIContent TEXT_ADVANCED_HEADER = new GUIContent("Advanced", "Advanced properties.");
        private static readonly GUIContent TEXT_TOP = new GUIContent("Objects to look for:");

        private static readonly GUIContent TEXT_3D_OBJECTS = new GUIContent("Hit 3D Objects", "Layer should raycast 3D objects.");
        private static readonly GUIContent TEXT_2D_OBJECTS = new GUIContent("Hit 2D Objects", "Layer should raycast 2D objects.");
        private static readonly GUIContent TEXT_WORLD_UI = new GUIContent("Hit World UI", "Layer should raycast World Space UI.");
        private static readonly GUIContent TEXT_SS_UI = new GUIContent("Hit Screen UI", "Layer should raycast Screen Space UI.");
        private static readonly GUIContent TEXT_LAYER_MASK = new GUIContent("Layer Mask", "Layer mask.");
        private static readonly GUIContent TEXT_HIT_FILTERS = new GUIContent("Use Hit FIlters", "Layer should test for individual HitTest objects.");

        private SerializedProperty advanced;
        private SerializedProperty hit3DObjects;
        private SerializedProperty hit2DObjects;
        private SerializedProperty hitWorldSpaceUI;
        private SerializedProperty hitScreenSpaceUI;
        private SerializedProperty layerMask;
        private SerializedProperty useHitFilters;

        protected virtual void OnEnable()
        {
            hideFlags = HideFlags.HideAndDontSave;

            advanced = serializedObject.FindProperty("advancedProps");
            hit3DObjects = serializedObject.FindProperty("hit3DObjects");
            hit2DObjects = serializedObject.FindProperty("hit2DObjects");
            hitWorldSpaceUI = serializedObject.FindProperty("hitWorldSpaceUI");
            hitScreenSpaceUI = serializedObject.FindProperty("hitScreenSpaceUI");
            layerMask = serializedObject.FindProperty("layerMask");
            useHitFilters = serializedObject.FindProperty("useHitFilters");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfDirtyOrScript();

            EditorGUILayout.LabelField(TEXT_TOP, EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(hitScreenSpaceUI, TEXT_SS_UI);
            EditorGUILayout.PropertyField(hit3DObjects, TEXT_3D_OBJECTS);
            EditorGUILayout.PropertyField(hit2DObjects, TEXT_2D_OBJECTS);
            EditorGUILayout.PropertyField(hitWorldSpaceUI, TEXT_WORLD_UI);
            EditorGUILayout.PropertyField(layerMask, TEXT_LAYER_MASK);

            EditorGUI.BeginChangeCheck();
            var expanded = GUIElements.BeginFoldout(advanced.isExpanded, TEXT_ADVANCED_HEADER);
            if (EditorGUI.EndChangeCheck())
            {
                advanced.isExpanded = expanded;
            }
            if (expanded)
            {
                GUILayout.BeginVertical(GUIElements.FoldoutStyle);
                drawAdvanced();
                GUILayout.EndVertical();
            }
            GUIElements.EndFoldout();

            serializedObject.ApplyModifiedProperties();
        }

        protected virtual void drawAdvanced()
        {
            EditorGUILayout.PropertyField(useHitFilters, TEXT_HIT_FILTERS);
        }

    }
}
