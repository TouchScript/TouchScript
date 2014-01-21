/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using TouchScript.Hit;
using UnityEngine;

namespace TouchScript.Layers
{
    /// <summary>
    /// Base class for all touch layers. Used to check if some object is hit by a touch point.
    /// </summary>
    [ExecuteInEditMode]
    public abstract class TouchLayer : MonoBehaviour
    {
        /// <summary>
        /// Result of a touch point's hit test with a layer.
        /// </summary>
        public enum LayerHitResult
        {
            /// <summary>
            /// Something wrong happened.
            /// </summary>
            Error = 0,

            /// <summary>
            /// Touch point hit an object.
            /// </summary>
            Hit = 1,

            /// <summary>
            /// Touch point didn't hit an object.
            /// </summary>
            Miss = 2
        }

        public event EventHandler<TouchLayerEventArgs> TouchBegan
        {
            add { touchBeganInvoker += value; }
            remove { touchBeganInvoker -= value; }
        }
        
        // Needed to overcome iOS AOT limitations
        private EventHandler<TouchLayerEventArgs> touchBeganInvoker;

        /// <summary>
        /// Touch layer's name.
        /// </summary>
        public String Name;

        /// <summary>
        /// Camera a touch layer is using.
        /// Null if layer doesn't support cameras.
        /// </summary>
        public virtual Camera Camera
        {
            get { return null; }
        }

        /// <summary>
        /// Checks if a point hits something in this layer.
        /// </summary>
        /// <param name="position">Position in screen coordinates.</param>
        /// <param name="hit">Raycast result.</param>
        /// <returns>Hit, if an object is hit, Miss or Error otherwise.</returns>
        public virtual LayerHitResult Hit(Vector2 position, out TouchHit hit)
        {
            hit = new TouchHit();
            return LayerHitResult.Miss;
        }

        internal bool BeginTouch(TouchPoint touch)
        {
            var result = beginTouch(touch);
            if (result == LayerHitResult.Hit)
            {
                touch.Layer = this;
                if (touchBeganInvoker != null) touchBeganInvoker(this, new TouchLayerEventArgs(touch));
                return true;
            }
            return false;
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

        /// <summary>
        /// Unity Awake callback.
        /// </summary>
        protected virtual void Awake()
        {
            setName();
            if (Application.isPlaying) TouchManager.AddLayer(this);
        }

        /// <summary>
        /// Unity OnDestroy callback.
        /// </summary>
        protected virtual void OnDestroy()
        {
            if (Application.isPlaying) TouchManager.RemoveLayer(this);
        }

        /// <summary>
        /// Updates touch layers's name.
        /// </summary>
        protected virtual void setName()
        {
            Name = "undefined";
        }

        /// <summary>
        /// Called when a layer is touched.
        /// </summary>
        /// <param name="touch">Touch point.</param>
        /// <returns>If this touch hit anything in the layer.</returns>
        protected virtual LayerHitResult beginTouch(TouchPoint touch)
        {
            return LayerHitResult.Error;
        }

        /// <summary>
        /// Called when a touch is moved.
        /// </summary>
        /// <param name="touch">Touch point.</param>
        protected virtual void moveTouch(TouchPoint touch)
        {}

        /// <summary>
        /// Called when a touch is moved.
        /// </summary>
        /// <param name="touch">Touch point.</param>
        protected virtual void endTouch(TouchPoint touch)
        {}

        /// <summary>
        /// Called when a touch is cancelled.
        /// </summary>
        /// <param name="touch">Touch point.</param>
        protected virtual void cancelTouch(TouchPoint touch)
        {}
    }

    public class TouchLayerEventArgs : EventArgs
    {

        public TouchPoint TouchPoint;

        public TouchLayerEventArgs(TouchPoint touchPoint) : base()
        {
            TouchPoint = touchPoint;
        }
    }

}