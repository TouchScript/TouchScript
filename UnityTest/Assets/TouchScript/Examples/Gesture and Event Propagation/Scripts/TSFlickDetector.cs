using TouchScript.Events;
using UnityEngine;
using System.Collections;
using TouchScript.Gestures;

[RequireComponent(typeof(FlickGesture))]
public class TSFlickDetector : MonoBehaviour {

	void Start () {
	    GetComponent<FlickGesture>().StateChanged += delegate(object sender, GestureStateChangeEventArgs args) { if (args.State == Gesture.GestureState.Recognized) print("FLICK"); };
	}
	
}
