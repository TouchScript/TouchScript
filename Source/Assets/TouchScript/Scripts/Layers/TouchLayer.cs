/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using TouchScript.Hit;
using TouchScript.Utils;
using UnityEngine;
using System.Collections;
using TouchScript.Pointers;

namespace TouchScript.Layers
{
    /// <summary>
    /// Base class for all pointer layers. Used to check if some object is hit by a pointer.
    /// <seealso cref="ITouchManager"/>
    /// <seealso cref="TouchHit"/>
    /// <seealso cref="Pointer"/>
    /// </summary>
    /// <remarks>
    /// <para>In <b>TouchScript</b> it's a layer's job to determine if a pointer on the screen hits anything in Unity's 3d/2d world.</para>
    /// <para><see cref="ITouchManager"/> keeps a sorted list of all layers in <see cref="ITouchManager.Layers"/> which it queries when a new pointer appears. It's a layer's job to return <see cref="LayerHitResult.Hit"/> if this pointer hits an object. Layers can even be used to "hit" objects outside of Unity's 3d world, for example <b>Scaleform</b> integration is implemented this way.</para>
    /// <para>Layers can be configured in a scene using <see cref="TouchManager"/> or from code using <see cref="ITouchManager"/> API.</para>
    /// <para>If you want to route pointers and manually control which objects they should "pointer" it's better to create a new layer extending <see cref="TouchLayer"/>.</para>
    /// </remarks>
    [ExecuteInEditMode]
    public abstract class TouchLayer : MonoBehaviour
    {
        #region Constants

        /// <summary>
        /// Result of a pointer's hit test with a layer.
        /// </summary>
        public enum LayerHitResult
        {
            /// <summary>
            /// Something wrong happened.
            /// </summary>
            Error = 0,

            /// <summary>
            /// Pointer hit an object.
            /// </summary>
            Hit = 1,

            /// <summary>
            /// Pointer didn't hit any object.
            /// </summary>
            Miss = 2
        }

        #endregion

        #region Events

        /// <summary>
        /// Occurs when layer determines that a pointer has hit something.
        /// </summary>
        public event EventHandler<TouchLayerEventArgs> PointerBegan
        {
            add { pointerBeganInvoker += value; }
            remove { pointerBeganInvoker -= value; }
        }

        // Needed to overcome iOS AOT limitations
        private EventHandler<TouchLayerEventArgs> pointerBeganInvoker;

        #endregion

        #region Public properties

        /// <summary>
        /// Pointer layer's name.
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
        /// Gets the projection parameters of this layer which might depend on a specific pointer data.
        /// </summary>
        /// <param name="pointer"> Pointer to retrieve projection parameters for. </param>
        /// <returns></returns>
        public virtual ProjectionParams GetProjectionParams(Pointer pointer)
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

        internal void INTERNAL_UpdatePointer(Pointer pointer)
        {
            updatePointer(pointer);
        }

        internal bool INTERNAL_PressPointer(Pointer pointer)
        {
            TouchHit hit;
            if (Delegate != null && Delegate.ShouldReceivePointer(this, pointer) == false) return false;
            var result = beginPointer(pointer, out hit);
            if (result == LayerHitResult.Hit)
            {
                pointer.INTERNAL_SetTargetData(hit);
                if (pointerBeganInvoker != null)
                    pointerBeganInvoker.InvokeHandleExceptions(this, new TouchLayerEventArgs(pointer));
                return true;
            }
            return false;
        }

        internal void INTERNAL_ReleasePointer(Pointer pointer)
        {
            endPointer(pointer);
        }

        internal void INTERNAL_CancelPointer(Pointer pointer)
        {
            cancelPointer(pointer);
        }

        #endregion

        #region Protected functions

        /// <summary>
        /// Updates pointer layers's name.
        /// </summary>
        protected virtual void setName()
        {
            if (string.IsNullOrEmpty(Name)) Name = "Layer";
        }

        /// <summary>
        /// Called when a layer is touched to query the layer if this pointer hits something.
        /// </summary>
        /// <param name="pointer">Pointer.</param>
        /// <param name="hit">Hit result.</param>
        /// <returns><see cref="LayerHitResult.Hit"/>, if an object is hit, <see cref="LayerHitResult.Miss"/> or <see cref="LayerHitResult.Error"/> otherwise.</returns>
        /// <remarks>This method may also be used to update some internal state or resend this event somewhere.</remarks>
        protected virtual LayerHitResult beginPointer(Pointer pointer, out TouchHit hit)
        {
            var result = Hit(pointer.Position, out hit);
            return result;
        }

        /// <summary>
        /// Called when a pointer is moved.
        /// </summary>
        /// <param name="pointer">Pointer.</param>
        /// <remarks>This method may also be used to update some internal state or resend this event somewhere.</remarks>
        protected virtual void updatePointer(Pointer pointer) {}

        /// <summary>
        /// Called when a pointer ends.
        /// </summary>
        /// <param name="pointer">Pointer.</param>
        /// <remarks>This method may also be used to update some internal state or resend this event somewhere.</remarks>
        protected virtual void endPointer(Pointer pointer) {}

        /// <summary>
        /// Called when a pointer is cancelled.
        /// </summary>
        /// <param name="pointer">Pointer.</param>
        /// <remarks>This method may also be used to update some internal state or resend this event somewhere.</remarks>
        protected virtual void cancelPointer(Pointer pointer) {}

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
        /// Gets the pointer associated with the event.
        /// </summary>
        public Pointer Pointer { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TouchLayerEventArgs"/> class.
        /// </summary>
        /// <param name="pointer">The pointer associated with the event.</param>
        public TouchLayerEventArgs(Pointer pointer)
            : base()
        {
            Pointer = pointer;
        }
    }
}