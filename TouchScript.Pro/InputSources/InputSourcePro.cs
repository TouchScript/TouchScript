/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using TouchScript.Devices.Display;
using UnityEngine;

namespace TouchScript.InputSources
{
    /// <summary>
    /// Base class for all PRO touch input sources. A duplicate of <see cref="TouchScript.InputSources.InputSource"/> since Unity doesn't see classes extending a class from another DLL.
    /// </summary>
    public abstract class InputSourcePro : MonoBehaviour, IInputSource
    {
        #region Public properties

        /// <summary>
        /// Optional remapper to use to change screen coordinates which go into the TouchManager.
        /// </summary>
        public ICoordinatesRemapper CoordinatesRemapper { get; set; }

        #endregion

        #region Private variables

        /// <summary>
        /// Reference to global touch manager.
        /// </summary>
        protected ITouchManager manager;

        #endregion

        #region Unity

        /// <summary>
        /// Unity3d OnEnable callback.
        /// </summary>
        protected virtual void OnEnable()
        {
            manager = TouchManager.Instance;
            if (manager == null) throw new InvalidOperationException("TouchManager instance is required!");
        }

        /// <summary>
        /// Unity3d OnDisable callback.
        /// </summary>
        protected virtual void OnDisable()
        {
            manager = null;
        }

        /// <summary>
        /// Unity3d Update callback.
        /// </summary>
        protected virtual void Update()
        { }

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