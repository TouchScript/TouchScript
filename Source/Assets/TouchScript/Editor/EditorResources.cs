/*
 * @author Valentin Simonov / http://va.lent.in/
 * Adapted from https://github.com/Unity-Technologies/PostProcessing
 */

using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TouchScript.Editor
{
    static class EditorResources
    {
        static string editorResourcesPath = string.Empty;

        internal static string EditorResourcesPath
        {
            get
            {
                if (string.IsNullOrEmpty(editorResourcesPath))
                {
                    string path;

                    if (searchForEditorResourcesPath(out path))
                        editorResourcesPath = path;
                    else
                        Debug.LogError("Unable to locate editor resources. Make sure the TouchScript package has been installed correctly.");
                }

                return editorResourcesPath;
            }
        }

        internal static T Load<T>(string name)
            where T : Object
        {
            return AssetDatabase.LoadAssetAtPath<T>(EditorResourcesPath + name);
        }

        static bool searchForEditorResourcesPath(out string path)
        {
            path = string.Empty;

            string searchStr = "/TouchScript/Editor/EditorResources/";
            string str = null;

            foreach (var assetPath in AssetDatabase.GetAllAssetPaths())
            {
                if (assetPath.Contains(searchStr))
                {
                    str = assetPath;
                    break;
                }
            }

            if (str == null)
                return false;

            path = str.Substring(0, str.LastIndexOf(searchStr, StringComparison.Ordinal) + searchStr.Length);
            return true;
        }
    }
}
