/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using TouchScript.Gestures;
using UnityEngine;

namespace TouchScript.Examples.Checkers
{
    public class Exclusive : MonoBehaviour, IGestureDelegate
    {
        public TransformGesture Target;
        public Color Color = Color.red;

        private bool exclusive = false;
        private Renderer cachedRenderer;
        private float shininess;

        private void Awake()
        {
            GestureManager.Instance.GlobalGestureDelegate = this;
            cachedRenderer = Target.GetComponent<Renderer>();
            shininess = cachedRenderer.sharedMaterial.GetFloat("_Shininess");
        }

        private void Update()
        {
            if (UnityEngine.Input.GetKey(KeyCode.Space))
            {
                exclusive = true;
                cachedRenderer.material.SetColor("_SpecColor", Color);
                cachedRenderer.material.SetFloat("_Shininess", 0);
            }
            else
            {
                exclusive = false;
                cachedRenderer.material.SetColor("_SpecColor", Color.white);
                cachedRenderer.material.SetFloat("_Shininess", shininess);
            }
        }

        public bool ShouldBegin(Gesture gesture)
        {
            if (exclusive) return gesture == Target;
            return true;
        }

        public bool ShouldReceiveTouch(Gesture gesture, TouchPoint touch)
        {
            if (exclusive) return gesture == Target;
            return true;
        }

        public bool ShouldRecognizeSimultaneously(Gesture first, Gesture second)
        {
            return false;
        }
    }
}