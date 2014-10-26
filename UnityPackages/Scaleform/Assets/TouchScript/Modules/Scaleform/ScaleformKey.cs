using System.IO;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class ScaleformKey : ScriptableObject
{

#if UNITY_EDITOR
    [MenuItem("Assets/TouchScript/Create Scaleform Key Asset")]
    public static void CreateAsset()
    {
        ScaleformKey asset = CreateInstance<ScaleformKey>();

        string path = AssetDatabase.GetAssetPath(Selection.activeObject);
        if (path == "")
        {
            path = "Assets";
        }
        else if (Path.GetExtension(path) != "")
        {
            path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
        }

        string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + "/New Scaleform Key.asset");

        AssetDatabase.CreateAsset(asset, assetPathAndName);

        AssetDatabase.SaveAssets();
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = asset;
    }
#endif

    public string Key = "";

}
