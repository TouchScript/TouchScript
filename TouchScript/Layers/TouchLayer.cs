/**
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using UnityEngine;

namespace TouchScript.Layers
{
    [ExecuteInEditMode()]
    public abstract class TouchLayer : MonoBehaviour
    {
        public String Name;

        public virtual Camera Camera
        {
            get { return null; }
        }

        public virtual HitResult Hit(Vector2 position, out RaycastHit hit)
        {
            hit = new RaycastHit();
            return HitResult.Miss;
        }

        internal void BeginTouch(TouchPoint touch)
        {
            beginTouch(touch);
        }

        internal void MoveTouch(TouchPoint touch)
        {
            moveTouch(touch);
        }

        internal void EndTouch(TouchPoint touch)
        {
            endTouch(touch);
        }

        internal void CancelTouch(TouchPoint touch)
        {
            cancelTouch(touch);
        }

        protected virtual void Awake()
        {
            if (GetComponents<TouchLayer>().Length > 1)
            {
                DestroyImmediate(this);
                return;
            }

            setName();
            if (Application.isPlaying) TouchManager.AddLayer(this);
        }

        protected virtual void OnDestroy()
        {
            if (Application.isPlaying) TouchManager.RemoveLayer(this);
        }

        protected virtual void setName()
        {
            if (String.IsNullOrEmpty(Name) && camera != null) Name = camera.name;
        }

        protected virtual void beginTouch(TouchPoint touch)
        {}

        protected virtual void moveTouch(TouchPoint touch)
        {}

        protected virtual void endTouch(TouchPoint touch)
        {}

        protected virtual void cancelTouch(TouchPoint touch)
        {}
    }

    public enum HitResult
    {
        Hit,
        Miss,
        Error
    }
}