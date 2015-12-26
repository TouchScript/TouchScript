/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using UnityEngine;

namespace TouchScript.InputSources
{
    /// <summary>
    /// Base class for all touch input sources.
    /// </summary>
    public abstract class InputSource : MonoBehaviour, IInputSource
    {
        #region Public properties

        /// <summary>
        /// Gets or sets current remapper.
        /// </summary>
        /// <value>Optional remapper to use to change screen coordinates which go into the TouchManager.</value>
        public ICoordinatesRemapper CoordinatesRemapper { get; set; }

        #endregion

        #region Private variables

        [SerializeField]
        [HideInInspector]
        private bool advancedProps; // is used to save whether advanced properties are opened or closed

        private TouchManagerInstance manager;

        #endregion

        #region Public methods

        /// <inheritdoc />
        public virtual void UpdateInput() {}

        /// <inheritdoc />
        public virtual void CancelTouch(TouchPoint touch, bool @return) {}

        #endregion

        #region Unity methods

        /// <summary>
        /// Unity OnEnable callback.
        /// </summary>
        protected virtual void OnEnable()
        {
            manager = TouchManagerInstance.Instance;
            if (manager == null) throw new InvalidOperationException("TouchManager instance is required!");
            manager.AddInput(this);
        }

        /// <summary>
        /// Unity OnDestroy callback.
        /// </summary>
        protected virtual void OnDisable()
        {
            if (manager != null)
            {
                manager.RemoveInput(this);
                manager = null;
            }
        }

        #endregion

        #region Protected methods

        /// <summary>
        /// Begin touch in given screen position.
        /// </summary>
        /// <param name="position">Screen position.</param>
        /// <param name="tags">Initial tags.</param>
        /// <param name="canRemap">if set to <c>true</c> a <see cref="CoordinatesRemapper"/> can be used on provided coordinates.</param>
        /// <returns> Internal touch id. </returns>
        protected virtual TouchPoint beginTouch(Vector2 position, Tags tags, bool canRemap = true)
        {
            if (CoordinatesRemapper != null && canRemap) position = CoordinatesRemapper.Remap(position);
            return manager.INTERNAL_BeginTouch(position, this, tags);
        }

        /// <summary>
        /// Mark touch as updated.
        /// </summary>
        /// <param name="id">Touch id.</param>
        protected virtual void updateTouch(int id)
        {
            manager.INTERNAL_UpdateTouch(id);
        }

        /// <summary>
        /// Mark touch as moved.
        /// </summary>
        /// <param name="id">Touch id.</param>
        /// <param name="position">Screen position.</param>
        protected virtual void moveTouch(int id, Vector2 position)
        {
            if (CoordinatesRemapper != null)
            {
                position = CoordinatesRemapper.Remap(position);
            }
            manager.INTERNAL_MoveTouch(id, position);
        }

        /// <summary>
        /// End touch with id.
        /// </summary>
        /// <param name="id">Touch point id.</param>
        protected virtual void endTouch(int id)
        {
            manager.INTERNAL_EndTouch(id);
        }

        /// <summary>
        /// Cancel touch with id.
        /// </summary>
        /// <param name="id">Touch id.</param>
        protected virtual void cancelTouch(int id)
        {
            manager.INTERNAL_CancelTouch(id);
        }

        #endregion
    }
}