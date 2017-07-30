/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using TouchScript.Core;
using TouchScript.Pointers;
using UnityEngine;

namespace TouchScript.InputSources
{

    #region Consts

    public delegate void PointerDelegate(Pointer pointer);

    #endregion

    /// <summary>
    /// Base class for all pointer input sources.
    /// </summary>
    public abstract class InputSource : MonoBehaviour, IInputSource
    {
        #region Public properties

        /// <summary>
        /// Gets or sets current remapper.
        /// </summary>
        /// <value>Optional remapper to use to change screen coordinates which go into the TouchManager.</value>
        public ICoordinatesRemapper CoordinatesRemapper
        {
            get { return coordinatesRemapper; }
            set
            {
                if (coordinatesRemapper == value) return;
                coordinatesRemapper = value;
                updateCoordinatesRemapper(value);
            }
        }

        #endregion

        #region Private variables

        /// <exclude/>
        [SerializeField]
        [HideInInspector]
		protected bool basicEditor = true;

		private ICoordinatesRemapper coordinatesRemapper;
        private TouchManagerInstance manager;

        #endregion

        #region Public methods

        /// <inheritdoc />
        public virtual bool UpdateInput()
        {
            return false;
        }

        /// <inheritdoc />
        public virtual void UpdateResolution() {}

        /// <inheritdoc />
        public virtual bool CancelPointer(Pointer pointer, bool shouldReturn)
        {
            return false;
        }

        #endregion

        #region Internal methods

        /// <inheritdoc />
        public virtual void INTERNAL_DiscardPointer(Pointer pointer) {}

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
        /// Adds the pointer to the system.
        /// </summary>
        /// <param name="pointer">The pointer to add.</param>
        protected virtual void addPointer(Pointer pointer)
        {
            manager.INTERNAL_AddPointer(pointer);
        }

        /// <summary>
        /// Mark pointer as updated.
        /// </summary>
        /// <param name="pointer">The pointer to update.</param>
        protected virtual void updatePointer(Pointer pointer)
        {
            if (pointer == null) return;
            manager.INTERNAL_UpdatePointer(pointer.Id);
        }

        /// <summary>
        /// Mark the pointer as touching the surface.
        /// </summary>
        /// <param name="pointer">The pointer.</param>
        protected virtual void pressPointer(Pointer pointer)
        {
            if (pointer == null) return;
            manager.INTERNAL_PressPointer(pointer.Id);
        }

        /// <summary>
        /// Mark the pointer as no longer touching the surface.
        /// </summary>
        /// <param name="pointer">The pointer.</param>
        protected virtual void releasePointer(Pointer pointer)
        {
            if (pointer == null) return;
            pointer.Buttons &= ~Pointer.PointerButtonState.AnyButtonPressed;
            manager.INTERNAL_ReleasePointer(pointer.Id);
        }

        /// <summary>
        /// Removes the pointer.
        /// </summary>
        /// <param name="pointer">The pointer.</param>
        protected virtual void removePointer(Pointer pointer)
        {
            if (pointer == null) return;
            manager.INTERNAL_RemovePointer(pointer.Id);
        }

        /// <summary>
        /// Cancels the pointer.
        /// </summary>
        /// <param name="pointer">The pointer.</param>
        protected virtual void cancelPointer(Pointer pointer)
        {
            if (pointer == null) return;
            manager.INTERNAL_CancelPointer(pointer.Id);
        }

        /// <summary>
        /// Called from <see cref="CoordinatesRemapper"/> setter to update touch handlers with the new value.
        /// </summary>
        /// <param name="remapper">The new remapper.</param>
        protected virtual void updateCoordinatesRemapper(ICoordinatesRemapper remapper) {}

        /// <summary>
        /// Remaps the coordinates using the <see cref="CoordinatesRemapper"/> if it is set.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <returns>Remapped position if <see cref="CoordinatesRemapper"/> is set; the value of position argument otherwise.</returns>
        protected virtual Vector2 remapCoordinates(Vector2 position)
        {
            if (coordinatesRemapper != null) return coordinatesRemapper.Remap(position);
            return position;
        }

        #endregion
    }
}