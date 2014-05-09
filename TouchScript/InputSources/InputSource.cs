/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using System.Collections.Generic;
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

        /// <summary>
        /// Reference to global touch manager.
        /// </summary>
        protected ITouchManager manager;

        #endregion

        #region Unity methods

        /// <summary>
        /// Unity OnEnable callback.
        /// </summary>
        protected virtual void OnEnable()
        {
            manager = TouchManager.Instance;
            if (manager == null) throw new InvalidOperationException("TouchManager instance is required!");
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

        protected virtual int beginTouch(Vector2 position)
        {
            return beginTouch(position, null, null);
        }

        protected virtual int beginTouch(Vector2 position, Tags tags)
        {
            return beginTouch(position, tags, null);
        }

        /// <summary>
        /// OnEnable touch in given screen position.
        /// </summary>
        /// <param name="position">Screen position.</param>
        /// <returns>Internal touch id.</returns>
        protected virtual int beginTouch(Vector2 position, Tags tags, IDictionary<string, System.Object> properties)
        {
            if (CoordinatesRemapper != null)
            {
                position = CoordinatesRemapper.Remap(position);
            }
            return manager.BeginTouch(position, tags, properties);
        }

        /// <summary>
        /// End touch with id.
        /// </summary>
        /// <param name="id">Touch point id.</param>
        protected virtual void endTouch(int id)
        {
            manager.EndTouch(id);
        }

        /// <summary>
        /// Move touch with id.
        /// </summary>
        /// <param name="id">Touch id.</param>
        /// <param name="position">New screen position.</param>
        protected virtual void moveTouch(int id, Vector2 position)
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
        protected virtual void cancelTouch(int id)
        {
            manager.CancelTouch(id);
        }

        #endregion
    }
}