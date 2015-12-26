/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using UnityEngine;
using TouchScript.Gestures;

namespace TouchScript.Examples.Colors
{
    public class Circle : MonoBehaviour
    {
        private bool isDestroyed = false;

        public Color Kill()
        {
            isDestroyed = true;

            GetComponent<TransformGesture>().Cancel(true, true);
            GetComponent<TransformGesture>().Cancel(true, true);
            var color = GetComponent<Renderer>().sharedMaterial.color;
            Destroy(gameObject);
            return color;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (isDestroyed) return;

            var gesture = GetComponent<TransformGesture>();
            if (gesture.State != Gesture.GestureState.Changed && gesture.State != Gesture.GestureState.Began) return;

            var otherCircle = other.GetComponent<Circle>();
            if (!otherCircle) return;

            var otherColor = otherCircle.Kill();
            var scale =
                Mathf.Sqrt(otherCircle.transform.localScale.x*otherCircle.transform.localScale.x +
                           transform.localScale.x*transform.localScale.x);
            var color = Color.Lerp(GetComponent<Renderer>().sharedMaterial.color, otherColor, .5f);

            var obj = Instantiate(gameObject) as GameObject;
            obj.transform.SetParent(transform.parent);
            obj.transform.localPosition = transform.localPosition;
            obj.transform.localRotation = transform.localRotation;
            obj.transform.localScale = new Vector3(scale, 1, scale);
            obj.GetComponent<Renderer>().sharedMaterial.color = color;

            Kill();
        }
    }
}