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
                isDirty = true;
            }
        }

        /// <inheritdoc />
        public Vector2 PreviousPosition { get; private set; }

        /// <inheritdoc />
        public ITouchHit Hit
        {
            get
            {
                if (isDirty)
                {
                    TouchManager.Instance.GetHitTarget(position, out hit);
                    isDirty = false;
                }
                return hit;
            }
            internal set
            {
                hit = value;
                isDirty = false;
            }
        }

        /// <inheritdoc />
        public TouchLayer Layer { get; internal set; }

        public Tags Tags { get; private set; }

        public IDictionary<string, System.Object> Properties { get { return properties; } } 

        #endregion

        #region Private variables

        private Vector2 position = Vector2.zero;
        private ITouchHit hit;
        private bool isDirty = false;
        private Dictionary<string, System.Object> properties; 

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="TouchPoint"/> class.
        /// </summary>
        /// <param name="id">Unique id of the touch.</param>
        /// <param name="position">Screen position of the touch.</param>
        /// <param name="tags">Initial tags.</param>
        /// <param name="properties">Initial properties.</param>
        internal TouchPoint(int id, Vector2 position, Tags tags, IDictionary<string, object> properties)
        {
            Id = id;
            Position = position;
            PreviousPosition = position;

            Tags = tags ?? new Tags();
            this.properties = (properties == null) ? new Dictionary<string, object>() : new Dictionary<string, object>(properties);
        }

        #region Internal methods

        /// <summary>
        /// Resets touch's position. Used internally to update <see cref="TouchPoint.PreviousPosition"/> between frames.
        /// </summary>
        internal void ResetPosition()
        {
            PreviousPosition = Position;
            isDirty = true;
        }

        #endregion
    }
}