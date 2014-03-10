using TouchScript.Gestures;
using UnityEngine;

public class Pan_Object : MonoBehaviour
{

    private Quaternion targetRotation;

	private void Awake()
	{
	    targetRotation = transform.localRotation;
	}

    private void OnEnable()
    {
        transform.FindChild("Button").GetComponent<TapGesture>().StateChanged += tapStateChangeHandler;
    }

    private void OnDisable()
    {
        transform.FindChild("Button").GetComponent<TapGesture>().StateChanged -= tapStateChangeHandler;
    }
	
	void Update ()
	{
	    transform.localRotation = Quaternion.Lerp(transform.localRotation, targetRotation, .1f);
	}

    private void tapStateChangeHandler(object sender, GestureStateChangeEventArgs e)
    {
        if (e.State == Gesture.GestureState.Recognized)
        {
            targetRotation = Quaternion.Euler(0, 90, 0)*targetRotation;
        }
    }

}
