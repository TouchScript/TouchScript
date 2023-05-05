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
            get { return _coordinatesRemapper; }
            set
            {
                if (_coordinatesRemapper == value) return;
                _coordinatesRemapper = value;
                UpdateCoordinatesRemapper(value);
            }
        }

        #endregion

        #region Private variables

        /// <exclude/>
        [SerializeField]
        [HideInInspector]
		protected bool basicEditor = true;

		private ICoordinatesRemapper _coordinatesRemapper;
        private TouchManagerInstance _touchManager;

        protected int ScreenWidth;
        protected int ScreenHeight;

        #endregion

        #region Public methods

        /// <inheritdoc />
        public abstract bool UpdateInput();


        /// <inheritdoc />
        public abstract bool CancelPointer(Pointer pointer, bool shouldReturn);

        #endregion

        #region Internal methods

        /// <inheritdoc />
        public abstract void INTERNAL_DiscardPointer(Pointer pointer);

        /// <inheritdoc />
        public virtual void INTERNAL_UpdateResolution()
        {
            ScreenWidth = Screen.width;
            ScreenHeight = Screen.height;
        }

        #endregion

        #region Unity methods

        /// <summary>
        /// Unity OnEnable callback.
        /// </summary>
        protected virtual void OnEnable()
        {
            _touchManager = TouchManagerInstance.Instance;
            if (_touchManager == null) throw new InvalidOperationException("TouchManager instance is required!");
            _touchManager.AddInput(this);

            Init();

            INTERNAL_UpdateResolution();
        }

        /// <summary>
        /// Unity OnDestroy callback.
        /// </summary>
        protected virtual void OnDisable()
        {
            if (_touchManager != null)
            {
                _touchManager.RemoveInput(this);
                _touchManager = null;
            }
        }

        #endregion

        #region Protected methods

        /// <summary>
        /// Initializes the input source.
        /// </summary>
        protected abstract void Init();

        /// <summary>
        /// Adds the pointer to the system.
        /// </summary>
        /// <param name="pointer">The pointer to add.</param>
        protected virtual void AddPointer(Pointer pointer)
        {
            _touchManager.INTERNAL_AddPointer(pointer);
        }

        /// <summary>
        /// Mark pointer as updated.
        /// </summary>
        /// <param name="pointer">The pointer to update.</param>
        protected virtual void UpdatePointer(Pointer pointer)
        {
            if (pointer == null) return;
            _touchManager.INTERNAL_UpdatePointer(pointer.Id);
        }

        /// <summary>
        /// Mark the pointer as touching the surface.
        /// </summary>
        /// <param name="pointer">The pointer.</param>
        protected virtual void PressPointer(Pointer pointer)
        {
            if (pointer == null) return;
            _touchManager.INTERNAL_PressPointer(pointer.Id);
        }

        /// <summary>
        /// Mark the pointer as no longer touching the surface.
        /// </summary>
        /// <param name="pointer">The pointer.</param>
        protected virtual void ReleasePointer(Pointer pointer)
        {
            if (pointer == null) return;
            pointer.Buttons &= ~Pointer.PointerButtonState.AnyButtonPressed;
            _touchManager.INTERNAL_ReleasePointer(pointer.Id);
        }

        /// <summary>
        /// Removes the pointer.
        /// </summary>
        /// <param name="pointer">The pointer.</param>
        protected virtual void RemovePointer(Pointer pointer)
        {
            if (pointer == null) return;
            _touchManager.INTERNAL_RemovePointer(pointer.Id);
        }

        /// <summary>
        /// Cancels the pointer.
        /// </summary>
        /// <param name="pointer">The pointer.</param>
        protected virtual void CancelPointer(Pointer pointer)
        {
            if (pointer == null) return;
            _touchManager.INTERNAL_CancelPointer(pointer.Id);
        }

        /// <summary>
        /// Called from <see cref="CoordinatesRemapper"/> setter to update touch handlers with the new value.
        /// </summary>
        /// <param name="remapper">The new remapper.</param>
        protected abstract void UpdateCoordinatesRemapper(ICoordinatesRemapper remapper);

        /// <summary>
        /// Remaps the coordinates using the <see cref="CoordinatesRemapper"/> if it is set.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <returns>Remapped position if <see cref="CoordinatesRemapper"/> is set; the value of position argument otherwise.</returns>
        protected virtual Vector2 RemapCoordinates(Vector2 position)
        {
            if (_coordinatesRemapper != null) return _coordinatesRemapper.Remap(position);
            return position;
        }

        #endregion
    }
}