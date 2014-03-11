using System.IO;
using UnityEditor;
using UnityEngine;

public class ScaleformKey : ScriptableObject
{

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

    public string Key = "";

}
