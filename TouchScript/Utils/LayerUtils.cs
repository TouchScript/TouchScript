/*
 * @author Olli Niskanen
 * Code inspired by Ivan Murashko
 * http://answers.unity3d.com/questions/585108/how-do-you-access-sorting-layers-via-scripting.html
 */

using System;
using System.Reflection;
using UnityEditorInternal;

namespace TouchScript.Utils
{
    /// <summary>
    /// Utils to access Unity layers
    /// </summary>
    public static class LayerUtils
    {
		// Get the sorting layer names
		public static string[] GetSortingLayerNames() {
			Type internalEditorUtilityType = typeof(InternalEditorUtility);
			PropertyInfo sortingLayersProperty = internalEditorUtilityType.GetProperty("sortingLayerNames", BindingFlags.Static | BindingFlags.NonPublic);
			return (string[])sortingLayersProperty.GetValue(null, new object[0]) as string[];
		}

		/// <summary>
		/// Get the unique sorting layer IDs
		/// </summary>
		public static int[] GetSortingLayerUniqueIDs() 
		{
			Type internalEditorUtilityType = typeof(InternalEditorUtility);
			PropertyInfo sortingLayerUniqueIDsProperty = internalEditorUtilityType.GetProperty("sortingLayerUniqueIDs", BindingFlags.Static | BindingFlags.NonPublic);
			return sortingLayerUniqueIDsProperty.GetValue(null, new System.Object[0]) as int[];
		}
    }
}
