using UnityEngine;
using System.Collections;

public class TSOptions : MonoBehaviour {

    public bool HideMouse = false;

	void Update () {
        Screen.showCursor = !HideMouse;
	}
}
