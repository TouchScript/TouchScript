/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using TouchScript.InputSources.Devices;
using UnityEngine;

namespace TouchScript.InputSources
{
    /// <summary>
    /// Base class for all touch input sources
    /// </summary>
    public abstract class InputSource : MonoBehaviour, IInputSource
    {
        #region Public properties

        /// <summary>
        /// Optional remapper to use to change screen coordinates which go into the TouchManager.
        /// </summary>
        public ICoordinatesRemapper CoordinatesRemapper { get; set; }

        public InputDevice InputDevice
        {
            get { return inputDevice; }
            set { inputDevice = value; }
        }

        #endregion

        #region Private variables

        /// <summary>
        /// Reference to global touch manager.
        /// </summary>
        protected ITouchManager manager;

        [SerializeField]
        private InputDevice inputDevice;

        #endregion

        #region Unity methods

        /// <summary>
        /// Unity OnEnable callback.
        /// </summary>
        protected virtual void OnEnable()
        {
            manager = TouchManager.Instance;
            if (manager == null) throw new InvalidOperationException("TouchManager instance is required!");

            if (inputDevice == null) inputDevice = ScriptableObject.CreateInstance<GenericInputDevice>();
        }

        /// <summary>
        /// Unity OnDestroy callback.
        /// </summary>
        protected virtual void OnDisable()
        {
            manager = null;
        }

        /// <summary>
        /// Unity Update callback.
        /// </summary>
        protected virtual void Update()
        {}

        #endregion

        #region Callbacks

        /// <summary>
        /// OnEnable touch in given screen position.
        /// </summary>
        /// <param name="position">Screen position.</param>
        /// <returns>Internal touch id.</returns>
        protected int beginTouch(Vector2 position)
        {
            if (CoordinatesRemapper != null)
            {
                position = CoordinatesRemapper.Remap(position);
            }
            return manager.BeginTouch(position);
        }

        /// <summary>
        /// End touch with id.
        /// </summary>
        /// <param name="id">Touch point id.</param>
        protected void endTouch(int id)
        {
            manager.EndTouch(id);
        }

        /// <summary>
        /// Move touch with id.
        /// </summary>
        /// <param name="id">Touch id.</param>
        /// <param name="position">New screen position.</param>
        protected void moveTouch(int id, Vector2 position)
        {
            if (CoordinatesRemapper != null)
            {
                position = CoordinatesRemapper.Remap(position);
            }
            manager.MoveTouch(id, position);
        }

        /// <summary>
        /// Cancel touch with id.
        /// </summary>
        /// <param name="id">Touch id.</param>
        protected void cancelTouch(int id)
        {
            manager.CancelTouch(id);
        }

        #endregion
    }
}