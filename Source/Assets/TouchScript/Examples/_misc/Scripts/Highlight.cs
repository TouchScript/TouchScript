/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using UnityEngine;
using TouchScript.Behaviors.UI;

/// <exclude />
public class Highlight : MonoBehaviour 
{

	public Color OverColor = Color.red;

	private OverHelper over;
	private MeshRenderer r;
	private Material oldMaterial;

	private void OnEnable()
	{
		over = GetComponent<OverHelper>();
		r = GetComponent<MeshRenderer>();
		oldMaterial = r.sharedMaterial;

		over.Over += overHandler;
		over.Out += outHandler;
	}

	private void OnDisable()
	{
		over.Over -= overHandler;
		over.Out -= outHandler;
	}

	void overHandler (object sender, System.EventArgs e)
	{
		r.material.color = OverColor;
	}

	void outHandler (object sender, System.EventArgs e)
	{
		r.material = oldMaterial;
	}

}
