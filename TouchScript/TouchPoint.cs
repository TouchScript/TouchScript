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

        public IDictionary<string, System.Object> Properties
        {
            get { return properties; }
        }

        #endregion

        #region Private variables

        private Vector2 position = Vector2.zero;
        private Vector2 newPosition = Vector2.zero;
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
        internal TouchPoint(int id, Vector2 position, Tags tags)
        {
            Id = id;
            this.position = PreviousPosition = newPosition = position;

            Tags = tags ?? new Tags();
            properties = new Dictionary<string, object>();
        }

        #region Internal methods

        /// <summary>
        /// Resets touch's position. Used internally to update <see cref="TouchPoint.PreviousPosition"/> between frames.
        /// </summary>
        internal void ResetPosition()
        {
            PreviousPosition = position;
            position = newPosition;
            newPosition = position;
            if (PreviousPosition != position) isDirty = true;
        }

        internal void SetPosition(Vector2 value)
        {
            newPosition = value;
        }

        #endregion
    }
}
