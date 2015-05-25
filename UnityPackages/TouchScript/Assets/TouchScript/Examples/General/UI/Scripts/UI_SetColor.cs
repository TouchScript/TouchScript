using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class UI_SetColor : MonoBehaviour 
{

	public List<Color> Colors;

	public void SetColor(int id) 
	{
		GetComponent<Image>().color = Colors[id];
	}

}
