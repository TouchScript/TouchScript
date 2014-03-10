using TouchScript.Gestures;
using TouchScript.Hit;
using UnityEngine;

public class Tap_Spawner : MonoBehaviour
{
    public Transform CubePrefab;
    public Transform Container;
    public float Scale = .5f;

    private void OnEnable()
    {
        GetComponent<TapGesture>().StateChanged += tapStateChangedHandler;
    }

    private void OnDisable()
    {
        GetComponent<TapGesture>().StateChanged -= tapStateChangedHandler;
    }

    private void tapStateChangedHandler(object sender, GestureStateChangeEventArgs e)
    {
        if (e.State == Gesture.GestureState.Recognized)
        {
            var gesture = sender as TapGesture;
            ITouchHit hit;
            gesture.GetTargetHitResult(out hit);
            var hit3d = hit as ITouchHit3D;
            if (hit3d == null) return;

            Color color = new Color(Random.value, Random.value, Random.value);
            var cube = Instantiate(CubePrefab) as Transform;
            cube.parent = Container;
            cube.name = "Cube";
            cube.localScale = Vector3.one*Scale*cube.localScale.x;
            cube.position = hit3d.Point + hit3d.Normal*2;
            cube.renderer.material.color = color;
        }
    }
}