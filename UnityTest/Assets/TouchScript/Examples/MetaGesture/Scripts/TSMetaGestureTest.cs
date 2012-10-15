using TouchScript.Gestures;
using UnityEngine;

[RequireComponent(typeof(MetaGesture))]
public class TSMetaGestureTest : MonoBehaviour {

	void Start () {
	    var gesture = GetComponent<MetaGesture>();
	    gesture.TouchPointAdded += (sender, args) => Debug.Log(string.Format("Touch added with id {0} at {1}.", args.TouchPoint.Id, args.TouchPoint.Position));
        gesture.TouchPointRemoved += (sender, args) => Debug.Log(string.Format("Touch removed with id {0}.", args.TouchPoint.Id));
        gesture.TouchPointUpdated += (sender, args) => Debug.Log(string.Format("Touch updated with id {0} at {1}.", args.TouchPoint.Id, args.TouchPoint.Position));
        gesture.TouchPointCancelled += (sender, args) => Debug.Log(string.Format("Touch cancelled with id {0}.", args.TouchPoint.Id));
	}
	
}
