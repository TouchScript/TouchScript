using UnityEditor;
using UnityEngine;
using System.Collections;

public class PlayMakerUnity2DEditorTools : MonoBehaviour {


	
	public static bool isSceneValid()
	{
		return GameObject.Find(PlayMakerUnity2d.PlayMakerUnity2dProxyName) != null;
	}
	
	public static void SetUpScene()
	{
		PrefabUtility.InstantiatePrefab(Resources.Load(PlayMakerUnity2d.PlayMakerUnity2dProxyName, typeof(GameObject)));
	}
	
	[ContextMenu("Help")]
	public void help ()
	{
		Application.OpenURL ("https://hutonggames.fogbugz.com/default.asp?W1150");
	}
	
	[MenuItem ("PlayMaker/Addons/Unity 2D/Components/Add PlayMakerUnity2D to Scene")]
	static void AddProxyToScene () {
		SetUpScene();
	}
	
	[MenuItem ("PlayMaker/Addons/Unity 2D/Components/Add PlayMakerUnity2D to Scene", true)]
	static bool ValidateAddProxyToScene() {
		return !isSceneValid();
	}
	
	
	[MenuItem ("PlayMaker/Addons/Unity 2D/Components/Add PlayMakerUnity2DProxy to Selection")]
	static void AddProxyToSelection () {
		
		if (Selection.activeTransform.gameObject.GetComponent<PlayMakerUnity2DProxy>()==null)
		{
			Selection.activeTransform.gameObject.AddComponent<PlayMakerUnity2DProxy>();
		}else{
			Debug.LogWarning("There is already a PlayMakerUnity2DProxy Component on GameObject '"+Selection.activeTransform.gameObject.name+"'");
		}
		
		if (!isSceneValid())
		{
			SetUpScene();
		}
	}
	
	[MenuItem ("PlayMaker/Addons/Unity 2D/Components/Add PlayMakerUnity2DProxy to Selection", true)]
	static bool ValidateAddProxyToSelection() {
		return Selection.activeObject != null && Selection.activeObject.GetType() == typeof(GameObject);
	}

}
