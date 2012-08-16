using UnityEngine;
using System.Collections;

public class Options : MonoBehaviour {

    public bool HideMouse = false;

	void Update () {
        Screen.showCursor = !HideMouse;
	}
}
