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
        #region Constants

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

        #endregion

        #region Events

        public event EventHandler<TouchLayerEventArgs> TouchBegan
        {
            add { touchBeganInvoker += value; }
            remove { touchBeganInvoker -= value; }
        }

        // Needed to overcome iOS AOT limitations
        private EventHandler<TouchLayerEventArgs> touchBeganInvoker;

        #endregion

        #region Public properties

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

        #endregion

        #region Public methods

        /// <summary>
        /// Checks if a point hits something in this layer.
        /// </summary>
        /// <param name="position">Position in screen coordinates.</param>
        /// <param name="hit">Raycast result.</param>
        /// <returns>Hit, if an object is hit, Miss or Error otherwise.</returns>
        public virtual LayerHitResult Hit(Vector2 position, out ITouchHit hit)
        {
            hit = null;
            return LayerHitResult.Miss;
        }

        #endregion

        #region Unity methods

        /// <summary>
        /// Unity Awake callback.
        /// </summary>
        protected virtual void Awake()
        {
            setName();
            if (Application.isPlaying && TouchManager.Instance != null) TouchManager.Instance.AddLayer(this);
        }

        /// <summary>
        /// Unity OnDestroy callback.
        /// </summary>
        protected virtual void OnDestroy()
        {
            if (Application.isPlaying && TouchManager.Instance != null) TouchManager.Instance.RemoveLayer(this);
        }

        #endregion

        #region Internal methods

        internal bool BeginTouch(TouchPoint touch)
        {
            ITouchHit hit;
            var result = beginTouch(touch, out hit);
            if (result == LayerHitResult.Hit)
            {
                touch.Layer = this;
                touch.Hit = hit;
                touch.Target = hit.Transform;
                if (touchBeganInvoker != null) touchBeganInvoker(this, new TouchLayerEventArgs(touch));
                return true;
            }
            return false;
        }

        internal void MoveTouch(ITouch touch)
        {
            moveTouch(touch);
        }

        internal void EndTouch(ITouch touch)
        {
            endTouch(touch);
        }

        internal void CancelTouch(ITouch touch)
        {
            cancelTouch(touch);
        }

        #endregion

        #region Protected functions

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
        protected virtual LayerHitResult beginTouch(ITouch touch, out ITouchHit hit)
        {
            hit = null;
            return LayerHitResult.Error;
        }

        /// <summary>
        /// Called when a touch is moved.
        /// </summary>
        /// <param name="touch">Touch point.</param>
        protected virtual void moveTouch(ITouch touch)
        {}

        /// <summary>
        /// Called when a touch is moved.
        /// </summary>
        /// <param name="touch">Touch point.</param>
        protected virtual void endTouch(ITouch touch)
        {}

        /// <summary>
        /// Called when a touch is cancelled.
        /// </summary>
        /// <param name="touch">Touch point.</param>
        protected virtual void cancelTouch(ITouch touch)
        {}

        #endregion
    }

    public class TouchLayerEventArgs : EventArgs
    {
        public ITouch TouchPoint { get; private set; }

        public TouchLayerEventArgs(ITouch touchPoint)
            : base()
        {
            TouchPoint = touchPoint;
        }
    }
}