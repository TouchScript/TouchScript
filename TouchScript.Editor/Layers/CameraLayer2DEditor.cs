/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using TouchScript.Layers;
using UnityEditor;
using UnityEngine;

namespace TouchScript.Editor.Layers
{
    [CustomEditor(typeof(CameraLayer2D))]
    internal sealed class CameraLayer2DEditor : UnityEditor.Editor
    {
        public const string TEXT_REBUILD = "Unity doesn't expose actual 2d layers sorting, so if you change 2d layers you must manually rebuild layers by pressing this button.";

        private SerializedProperty sortedLayerIds;
        private SerializedProperty index;

        private void OnEnable()
        {
            sortedLayerIds = serializedObject.FindProperty("sortedLayerIds");
            index = serializedObject.FindProperty("Index");
            if (sortedLayerIds.arraySize == 0) rebuildSortingLayers();
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            GUILayout.Space(10);
            if (GUILayout.Button(new GUIContent("Update Sorting Layers", TEXT_REBUILD)))
            {
                rebuildSortingLayers();
            }
            EditorGUILayout.PropertyField(index);

            serializedObject.ApplyModifiedProperties();
        }

        private void rebuildSortingLayers()
        {
            var data = getSortingLayerIdsToSortOrder();
            sortedLayerIds.arraySize = data.Length;
            for (var i = 0; i < data.Length; i++)
            {
                sortedLayerIds.GetArrayElementAtIndex(i).intValue = data[i];
            }

            Debug.Log("CameraLayer2D: sorting layer order was rebuilt.");
        }

        // https://github.com/InteractiveLab/TouchScript/issues/60
        // Based on https://gist.github.com/stuartcarnie/8511903
        private static int[] getSortingLayerIdsToSortOrder()
        {
            var type = typeof(UnityEditorInternal.InternalEditorUtility);

            var getSortingLayerCount = type.GetMethod("GetSortingLayerCount", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            var getSortingLayerUserID = type.GetMethod("GetSortingLayerUserID", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);

            int count = (int)getSortingLayerCount.Invoke(null, null);
            var layerIdsToSortOrder = new int[count];
            int maxUserId = 0;
            for (int i = 0; i < count; i++)
            {
                var userId = (int)getSortingLayerUserID.Invoke(null, new object[] {i});
                maxUserId = Math.Max(maxUserId, userId);
                if (maxUserId >= layerIdsToSortOrder.Length)
                {
                    Array.Resize(ref layerIdsToSortOrder, maxUserId + 1);
                }
                layerIdsToSortOrder[userId] = i;
            }

            return layerIdsToSortOrder;
        }
    }
}
