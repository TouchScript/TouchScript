/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using TouchScript.Hit;
using TouchScript.Layers;
using UnityEngine;

namespace TouchScript
{
    /// <summary>
    /// Touch point.
    /// </summary>
    internal sealed class TouchPoint : ITouchPoint
    {
        #region Public properties

        public int Id { get; private set; }

        public Transform Target { get; internal set; }

        public Vector2 Position
        {
            get { return position; }
            internal set
            {
                PreviousPosition = position;
                position = value;
            }
        }

        public Vector2 PreviousPosition { get; private set; }

        public ITouchHit Hit { get; internal set; }

        public TouchLayer Layer { get; internal set; }

        #endregion

        #region Private variables

        private Vector2 position = Vector2.zero;

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="TouchPoint"/> class.
        /// </summary>
        /// <param name="id">Touch point id.</param>
        /// <param name="position">Screen position.</param>
        internal TouchPoint(int id, Vector2 position)
        {
            Id = id;
            Position = position;
            PreviousPosition = position;
        }

        #region Internal methods

        internal void ResetPosition()
        {
            PreviousPosition = Position;
        }

        #endregion
    }
}