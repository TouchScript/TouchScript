using TouchScript.Gestures.Simple;
using UnityEngine;

[RequireComponent(typeof(SimpleRotateGesture))]
[RequireComponent(typeof(SimpleScaleGesture))]
public class FullscreenZoom : MonoBehaviour
{

    public float ZoomSpeed = 1;

    private void Start()
    {
        GetComponent<SimpleRotateGesture>().StateChanged += (sender, args) =>
                                                            {
                                                                transform.RotateAroundLocal(Vector3.back, (sender as SimpleRotateGesture).LocalDeltaRotation * Mathf.Deg2Rad);
                                                            };
        GetComponent<SimpleScaleGesture>().StateChanged += (sender, args) =>
                                                           {
                                                               transform.localPosition += Vector3.forward * ((sender as SimpleScaleGesture).LocalDeltaScale - 1) * ZoomSpeed;
                                                           };
    }
}