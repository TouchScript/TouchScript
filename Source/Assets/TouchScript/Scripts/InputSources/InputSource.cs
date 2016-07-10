/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
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

        [SerializeField]
        [HideInInspector]
        private bool advancedProps; // is used to save whether advanced properties are opened or closed

        private ICoordinatesRemapper coordinatesRemapper;
        private TouchManagerInstance manager;

        #endregion

        #region Public methods

        /// <inheritdoc />
        public virtual void UpdateInput() {}

        /// <inheritdoc />
        public virtual bool CancelPointer(Pointer pointer, bool shouldReturn)
        {
            return false;
        }

        #endregion

        #region Internal methods

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

        protected virtual void addPointer(Pointer pointer)
        {
            manager.INTERNAL_AddPointer(pointer);
        }

        /// <summary>
        /// Mark pointer as moved.
        /// </summary>
        /// <param name="id">Pointer id.</param>
        /// <param name="position">Screen position.</param>
        protected virtual void updatePointer(Pointer pointer)
        {
            if (pointer == null) return;
            manager.INTERNAL_UpdatePointer(pointer.Id);
        }

        /// <summary>
        /// Begin pointer in given screen position.
        /// </summary>
        /// <param name="position">Screen position.</param>
        /// <returns> New pointer. </returns>
        protected virtual void pressPointer(Pointer pointer)
        {
            if (pointer == null) return;
            manager.INTERNAL_PressPointer(pointer.Id);
        }

        /// <summary>
        /// End pointer with id.
        /// </summary>
        /// <param name="id">Pointer id.</param>
        protected virtual void releasePointer(Pointer pointer)
        {
            if (pointer == null) return;
            pointer.Flags = pointer.Flags & ~Pointer.FLAG_INCONTACT;
            manager.INTERNAL_ReleasePointer(pointer.Id);
        }

        protected virtual void removePointer(Pointer pointer)
        {
            if (pointer == null) return;
            manager.INTERNAL_RemovePointer(pointer.Id);
        }

        /// <summary>
        /// Cancel pointer with id.
        /// </summary>
        /// <param name="id">Pointer id.</param>
        protected virtual void cancelPointer(Pointer pointer)
        {
            if (pointer == null) return;
            manager.INTERNAL_CancelPointer(pointer.Id);
        }

        protected virtual void updateCoordinatesRemapper(ICoordinatesRemapper remapper)
        {
        }

        protected virtual Vector2 remapCoordinates(Vector2 position)
        {
            if (coordinatesRemapper != null) return coordinatesRemapper.Remap(position);
            return position;
        }

        #endregion
    }
}