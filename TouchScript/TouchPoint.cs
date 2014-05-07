/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System.Collections.Generic;
using TouchScript.Hit;
using TouchScript.Layers;
using UnityEngine;

namespace TouchScript
{
    /// <summary>
    /// Class which internally represents a touch.
    /// </summary>
    internal sealed class TouchPoint : ITouch
    {
        #region Public properties

        /// <inheritdoc />
        public int Id { get; private set; }

        /// <inheritdoc />
        public Transform Target { get; internal set; }

        /// <inheritdoc />
        public Vector2 Position
        {
            get { return position; }
            internal set
            {
                PreviousPosition = position;
                position = value;
            }
        }

        /// <inheritdoc />
        public Vector2 PreviousPosition { get; private set; }

        /// <inheritdoc />
        public ITouchHit Hit { get; internal set; }

        /// <inheritdoc />
        public TouchLayer Layer { get; internal set; }

        public ICollection<string> Tags { get { return tags; } }

        #endregion

        #region Private variables

        private Vector2 position = Vector2.zero;
        private List<string> tags;

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="TouchPoint"/> class.
        /// </summary>
        /// <param name="id">Unique id of the touch.</param>
        /// <param name="position">Screen position of the touch.</param>
        internal TouchPoint(int id, Vector2 position, ICollection<string> tags)
        {
            Id = id;
            Position = position;
            PreviousPosition = position;
            if (tags == null) this.tags = new List<string>();
            else this.tags = new List<string>(tags);
        }

        #region Internal methods

        /// <summary>
        /// Resets touch's position. Used internally to update <see cref="TouchPoint.PreviousPosition"/> between frames.
        /// </summary>
        internal void ResetPosition()
        {
            PreviousPosition = Position;
        }

        #endregion
    }
}