using TouchScript.Gestures;
using TouchScript.Gestures.Simple;
using UnityEngine;

[RequireComponent(typeof(SimpleTapGesture))]
public class Tapper : MonoBehaviour
{
    private Vector3 startScale;

    private void Start()
    {
        startScale = transform.localScale;

        GetComponent<SimpleTapGesture>().StateChanged += (sender, args) =>
                                                         {
                                                             switch (args.State)
                                                             {
                                                                 case Gesture.GestureState.Recognized:
                                                                     transform.localScale = startScale*Random.Range(.5f, 2f);
                                                                     transform.localEulerAngles = new Vector3(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360));
                                                                     break;
                                                             }
                                                         };
    }
}