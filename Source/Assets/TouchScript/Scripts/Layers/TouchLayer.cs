/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using TouchScript.Hit;
using TouchScript.Utils;
using UnityEngine;
using System.Collections;

namespace TouchScript.Layers
{
    /// <summary>
    /// Base class for all touch layers. Used to check if some object is hit by a touch point.
    /// <seealso cref="ITouchManager"/>
    /// <seealso cref="TouchHit"/>
    /// <seealso cref="TouchPoint"/>
    /// </summary>
    /// <remarks>
    /// <para>In <b>TouchScript</b> it's a layer's job to determine if a touch on the screen hits anything in Unity's 3d/2d world.</para>
    /// <para><see cref="ITouchManager"/> keeps a sorted list of all layers in <see cref="ITouchManager.Layers"/> which it queries when a new touch appears. It's a layer's job to return <see cref="LayerHitResult.Hit"/> if this touch hits an object. Layers can even be used to "hit" objects outside of Unity's 3d world, for example <b>Scaleform</b> integration is implemented this way.</para>
    /// <para>Layers can be configured in a scene using <see cref="TouchManager"/> or from code using <see cref="ITouchManager"/> API.</para>
    /// <para>If you want to route touches and manually control which objects they should "touch" it's better to create a new layer extending <see cref="TouchLayer"/>.</para>
    /// </remarks>
    [ExecuteInEditMode]
    public abstract class TouchLayer : MonoBehaviour
    {
        #region Constants

        /// <summary>
        /// Result of a touch's hit test with a layer.
        /// </summary>
        public enum LayerHitResult
        {
            /// <summary>
            /// Something wrong happened.
            /// </summary>
            Error = 0,

            /// <summary>
            /// Touch hit an object.
            /// </summary>
            Hit = 1,

            /// <summary>
            /// Touch didn't hit any object.
            /// </summary>
            Miss = 2
        }

        #endregion

        #region Events

        /// <summary>
        /// Occurs when layer determines that a touch has hit something.
        /// </summary>
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
        public string Name;

        /// <summary>
        /// Layers screen to world projection normal.
        /// </summary>
        public virtual Vector3 WorldProjectionNormal
        {
            get { return transform.forward; }
        }

        /// <summary>
        /// Gets or sets an object implementing <see cref="ILayerDelegate"/> to be asked for layer specific actions.
        /// </summary>
        /// <value> The delegate. </value>
        public ILayerDelegate Delegate { get; set; }

        #endregion

        #region Public methods

        /// <summary>
        /// Gets the projection parameters of this layer which might depend on a specific touch data.
        /// </summary>
        /// <param name="touch"> Touch to retrieve projection parameters for. </param>
        /// <returns></returns>
        public virtual ProjectionParams GetProjectionParams(TouchPoint touch)
        {
            return layerProjectionParams;
        }

        /// <summary>
        /// Checks if a point in screen coordinates hits something in this layer.
        /// </summary>
        /// <param name="position">Position in screen coordinates.</param>
        /// <param name="hit">Hit result.</param>
        /// <returns><see cref="LayerHitResult.Hit"/>, if an object is hit, <see cref="LayerHitResult.Miss"/> or <see cref="LayerHitResult.Error"/> otherwise.</returns>
        public virtual LayerHitResult Hit(Vector2 position, out TouchHit hit)
        {
            hit = default(TouchHit);
            if (enabled == false || gameObject.activeInHierarchy == false) return LayerHitResult.Miss;
            return LayerHitResult.Error;
        }

        #endregion

        #region Private variables

        /// <summary>
        /// The layer projection parameters.
        /// </summary>
        protected ProjectionParams layerProjectionParams;

        #endregion

        #region Unity methods

        /// <summary>
        /// Unity Awake callback.
        /// </summary>
        protected virtual void Awake()
        {
            setName();
            if (!Application.isPlaying) return;

            layerProjectionParams = createProjectionParams();
            StartCoroutine(lateAwake());
        }

        private IEnumerator lateAwake()
        {
            yield return null;

            // Add ourselves after TouchManager finished adding layers in order
            TouchManager.Instance.AddLayer(this, -1, false);
        }

        // To be able to turn layers off
        private void Start() {}

        /// <summary>
        /// Unity OnDestroy callback.
        /// </summary>
        protected virtual void OnDestroy()
        {
            if (!Application.isPlaying || TouchManager.Instance == null) return;

            StopAllCoroutines();
            TouchManager.Instance.RemoveLayer(this);
        }

        #endregion

        #region Internal methods

        internal bool INTERNAL_BeginTouch(TouchPoint touch)
        {
            TouchHit hit;
            if (Delegate != null && Delegate.ShouldReceiveTouch(this, touch) == false) return false;
            var result = beginTouch(touch, out hit);
            if (result == LayerHitResult.Hit)
            {
                touch.Layer = this;
                touch.Hit = hit;
                if (hit.Transform != null) touch.Target = hit.Transform;
                if (touchBeganInvoker != null)
                    touchBeganInvoker.InvokeHandleExceptions(this, new TouchLayerEventArgs(touch));
                return true;
            }
            return false;
        }

        internal void INTERNAL_UpdateTouch(TouchPoint touch)
        {
            updateTouch(touch);
        }

        internal void INTERNAL_EndTouch(TouchPoint touch)
        {
            endTouch(touch);
        }

        internal void INTERNAL_CancelTouch(TouchPoint touch)
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
            if (string.IsNullOrEmpty(Name)) Name = "Layer";
        }

        /// <summary>
        /// Called when a layer is touched to query the layer if this touch hits something.
        /// </summary>
        /// <param name="touch">Touch.</param>
        /// <param name="hit">Hit result.</param>
        /// <returns><see cref="LayerHitResult.Hit"/>, if an object is hit, <see cref="LayerHitResult.Miss"/> or <see cref="LayerHitResult.Error"/> otherwise.</returns>
        /// <remarks>This method may also be used to update some internal state or resend this event somewhere.</remarks>
        protected virtual LayerHitResult beginTouch(TouchPoint touch, out TouchHit hit)
        {
            var result = Hit(touch.Position, out hit);
            return result;
        }

        /// <summary>
        /// Called when a touch is moved.
        /// </summary>
        /// <param name="touch">Touch.</param>
        /// <remarks>This method may also be used to update some internal state or resend this event somewhere.</remarks>
        protected virtual void updateTouch(TouchPoint touch) {}

        /// <summary>
        /// Called when a touch ends.
        /// </summary>
        /// <param name="touch">Touch.</param>
        /// <remarks>This method may also be used to update some internal state or resend this event somewhere.</remarks>
        protected virtual void endTouch(TouchPoint touch) {}

        /// <summary>
        /// Called when a touch is cancelled.
        /// </summary>
        /// <param name="touch">Touch.</param>
        /// <remarks>This method may also be used to update some internal state or resend this event somewhere.</remarks>
        protected virtual void cancelTouch(TouchPoint touch) {}

        /// <summary>
        /// Creates projection parameters.
        /// </summary>
        /// <returns> Created <see cref="ProjectionParams"/> instance.</returns>
        protected virtual ProjectionParams createProjectionParams()
        {
            return new ProjectionParams();
        }

        #endregion
    }

    /// <summary>
    /// Arguments used with <see cref="TouchLayer"/> events.
    /// </summary>
    public class TouchLayerEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the touch associated with the event.
        /// </summary>
        public TouchPoint Touch { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TouchLayerEventArgs"/> class.
        /// </summary>
        /// <param name="touch">The touch associated with the event.</param>
        public TouchLayerEventArgs(TouchPoint touch)
            : base()
        {
            Touch = touch;
        }
    }
}