using TouchScript.Gestures;
using UnityEngine;

public class Basic2dExample_Logo : MonoBehaviour {

    private enum State
    {
        Free,
        Manual
    }

    private State state;
    private Rigidbody2D rb;

	void Start ()
	{
	    GetComponent<PressGesture>().StateChanged += (sender, args) =>
	    {
	        switch (args.State)
	        {
	            case Gesture.GestureState.Recognized:
	                if (state == State.Free) stateManual();
	                break;
	        }
	    };
        GetComponent<ReleaseGesture>().StateChanged += (sender, args) =>
        {
            switch (args.State)
            {
                case Gesture.GestureState.Recognized:
                    if (state == State.Manual) stateFree();
                    break;
            }
        };

	    rb = GetComponent<Rigidbody2D>();

	    stateFree();
	}
	
    private void stateFree()
    {
        setState(State.Free);

        rb.gravityScale = 1;
    }

    private void stateManual()
    {
        setState(State.Manual);

        rb.gravityScale = 0;
    }

    private void setState(State value)
    {
        state = value;
        Debug.Log("State " + value);
    }

}
