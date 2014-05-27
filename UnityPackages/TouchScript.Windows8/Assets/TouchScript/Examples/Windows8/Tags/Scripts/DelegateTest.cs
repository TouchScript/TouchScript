using TouchScript;
using TouchScript.Gestures;
using UnityEngine;

public class DelegateTest : MonoBehaviour
{
    private void Start()
    {
        GetComponent<TapGesture>().Delegate = new TapDelegate();
    }

    private class TapDelegate : IGestureDelegate
    {
        public bool ShouldReceiveTouch(Gesture gesture, ITouch touch)
        {
            if (touch.Tags.HasTag("Mouse")) return false;
            return true;
        }

        public bool ShouldBegin(Gesture gesture)
        {
            return true;
        }

        public bool ShouldRecognizeSimultaneously(Gesture first, Gesture second)
        {
            return first.IsFriendly(second);
        }
    }
}