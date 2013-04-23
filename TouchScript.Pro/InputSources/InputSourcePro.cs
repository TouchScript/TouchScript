/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using UnityEngine;

namespace TouchScript.InputSources
{
    /// <summary>
    /// Base class for all touch input sources
    /// </summary>
    public abstract class InputSourcePro : MonoBehaviour, IInputSource
    {
        #region Private variables

        protected TouchManager Manager;

        #endregion

        #region Public properties

        /// <summary>
        /// Optional remapper to use to change screen coordinates which go into the TouchManager.
        /// </summary>
        public ICoordinatesRemapper CoordinatesRemapper { get; set; }

        #endregion

        #region Unity

        protected virtual void Start()
        {
            Manager = TouchManager.Instance;
            if (Manager == null) throw new InvalidOperationException("TouchManager instance is required!");
        }

        protected virtual void OnDestroy()
        {
            Manager = null;
        }

        protected virtual void Update()
        { }

        #endregion

        #region Callbacks

        protected int beginTouch(Vector2 position)
        {
            if (CoordinatesRemapper != null)
            {
                position = CoordinatesRemapper.Remap(position);
            }
            return Manager.BeginTouch(position);
        }

        protected void endTouch(int id)
        {
            Manager.EndTouch(id);
        }

        protected void moveTouch(int id, Vector2 position)
        {
            if (CoordinatesRemapper != null)
            {
                position = CoordinatesRemapper.Remap(position);
            }
            Manager.MoveTouch(id, position);
        }

        protected void cancelTouch(int id)
        {
            Manager.CancelTouch(id);
        }

        #endregion
    }
}