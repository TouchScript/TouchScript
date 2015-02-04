/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using System.Collections;
using TouchScript.Hit;
using TouchScript.Utils;
using UnityEngine;

namespace TouchScript.Layers
{
    /// <summary>
    /// Base class for all touch layers. Used to check if some object is hit by a touch point.
    /// <seealso cref="ITouchManager"/>
    /// <seealso cref="ITouchHit"/>
    /// <seealso cref="ITouch"/>
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
        public String Name;

        /// <summary>
        /// Touch layer's index.
        /// </summary>
        public int Index;

        /// <summary>
        /// Layers screen to world projection normal.
        /// </summary>
        public virtual Vector3 WorldProjectionNormal
        {
            get { return transform.forward; }
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Checks if a point in screen coordinates hits something in this layer.
        /// </summary>
        /// <param name="position">Position in screen coordinates.</param>
        /// <param name="hit">Hit result.</param>
        /// <returns><see cref="LayerHitResult.Hit"/>, if an object is hit, <see cref="LayerHitResult.Miss"/> or <see cref="LayerHitResult.Error"/> otherwise.</returns>
        public virtual LayerHitResult Hit(Vector2 position, out ITouchHit hit)
        {
            hit = null;
            if (enabled == false || gameObject.activeInHierarchy == false) return LayerHitResult.Miss;
            return LayerHitResult.Error;
        }

        /// <summary>
        /// Projects a screen point on a plane using this layer's parameters.
        /// </summary>
        /// <param name="screenPosition">Screen point to project.</param>
        /// <param name="projectionPlane">3D plane to project to.</param>
        /// <returns>Projected point in world coordinates.</returns>
        public virtual Vector3 ProjectTo(Vector2 screenPosition, Plane projectionPlane)
        {
            return ProjectionUtils.ScreenToPlaneProjection(screenPosition, projectionPlane);
        }

        #endregion

        #region Unity methods

        /// <summary>
        /// Unity Awake callback.
        /// </summary>
        protected virtual void Awake()
        {
            setName();
            if (Application.isPlaying) StartCoroutine(lateAwake());
        }

        /// <summary>
        /// Unity OnDestroy callback.
        /// </summary>
        protected virtual void OnDestroy()
        {
            if (Application.isPlaying && TouchManager.Instance != null) TouchManager.Instance.RemoveLayer(this);
        }

        /// <summary>
        /// Unity OnEnable callback.
        /// </summary>
        protected virtual void OnEnable()
        {
            if (Application.isPlaying && TouchManager.Instance != null) TouchManager.Instance.AddLayer(this);
        }

        /// <summary>
        /// Unity OnDisable callback.
        /// </summary>
        protected virtual void OnDisable()
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
                if (hit != null) touch.Target = hit.Transform;
                if (touchBeganInvoker != null) touchBeganInvoker(this, new TouchLayerEventArgs(touch));
                return true;
            }
            return false;
        }

        internal void UpdateTouch(ITouch touch)
        {
            updateTouch(touch);
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
        /// Called when a layer is touched to query the layer if this touch hits something.
        /// </summary>
        /// <param name="touch">Touch.</param>
        /// <param name="hit">Hit result.</param>
        /// <returns><see cref="LayerHitResult.Hit"/>, if an object is hit, <see cref="LayerHitResult.Miss"/> or <see cref="LayerHitResult.Error"/> otherwise.</returns>
        /// <remarks>This method may also be used to update some internal state or resend this event somewhere.</remarks>
        protected virtual LayerHitResult beginTouch(ITouch touch, out ITouchHit hit)
        {
            var result = Hit(touch.Position, out hit);
            return result;
        }

        /// <summary>
        /// Called when a touch is moved.
        /// </summary>
        /// <param name="touch">Touch.</param>
        /// <remarks>This method may also be used to update some internal state or resend this event somewhere.</remarks>
        protected virtual void updateTouch(ITouch touch) {}

        /// <summary>
        /// Called when a touch ends.
        /// </summary>
        /// <param name="touch">Touch.</param>
        /// <remarks>This method may also be used to update some internal state or resend this event somewhere.</remarks>
        protected virtual void endTouch(ITouch touch) {}

        /// <summary>
        /// Called when a touch is cancelled.
        /// </summary>
        /// <param name="touch">Touch.</param>
        /// <remarks>This method may also be used to update some internal state or resend this event somewhere.</remarks>
        protected virtual void cancelTouch(ITouch touch) {}

        #endregion

        #region Private functions

        private IEnumerator lateAwake()
        {
            yield return new WaitForEndOfFrame();
            TouchManager.Instance.AddLayer(this);
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
        public ITouch Touch { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TouchLayerEventArgs"/> class.
        /// </summary>
        /// <param name="touch">The touch associated with the event.</param>
        public TouchLayerEventArgs(ITouch touch)
            : base()
        {
            Touch = touch;
        }
    }
}
