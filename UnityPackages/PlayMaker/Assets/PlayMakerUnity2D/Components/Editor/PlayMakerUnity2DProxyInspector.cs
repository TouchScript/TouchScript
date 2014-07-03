using System;
using UnityEditor;
using UnityEngine;


//[CustomEditor(typeof(PlayMakerUnity2DProxy))]
public class PlayMakerUnity2DProxyInspector : Editor
{

	public override void OnInspectorGUI()
	{

		EditorGUI.indentLevel = 0;

		//OnGUI_DrawEventImplementation();

	}
	/*
	public void OnGUI_DrawEventImplementation()
	{
		PlayMakerUnity2DProxy _target = (PlayMakerUnity2DProxy)this.target;

		if (_target.han)
		{
			GUI.color = Color.green;
			GUILayout.BeginHorizontal("","box",GUILayout.ExpandWidth(true));
				GUI.color = Color.white;
			
				EditorGUILayout.LabelField(PlayMakerUnity2d.OnCollisionEnter2DEvent,"Implemented");
			
			GUILayout.EndHorizontal();
		}else{
			GUILayout.BeginHorizontal("","box",GUILayout.ExpandWidth(true));
			
				EditorGUILayout.LabelField(PlayMakerUnity2d.OnCollisionEnter2DEvent,"not found");
			
			GUILayout.EndHorizontal();
		}
	}
	*/

}