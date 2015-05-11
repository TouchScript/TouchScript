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
        public TouchHit Hit { get; internal set; }

        /// <inheritdoc />
        public TouchLayer Layer { get; internal set; }

        public ProjectionParams ProjectionParams
        {
            get
            {
                if (!projection.IsValid)
                {
                    if (Layer == null) projection = TouchLayer.INVALID_PROJECTION_PARAMS;
                    projection = Layer.GetProjectionParams(this);
                }
                return projection;
            }
        }

        public Tags Tags { get; private set; }

        public IDictionary<string, System.Object> Properties
        {
            get { return properties; }
        }

        #endregion

        #region Private variables

        private Vector2 position = Vector2.zero;
        private Vector2 newPosition = Vector2.zero;
        private ProjectionParams projection;
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
        /// </summary>
        internal void NewFrame()
        {
            PreviousPosition = position;
            position = newPosition;
            newPosition = position;
        }

        internal void SetPosition(Vector2 value)
        {
            newPosition = value;
        }

        #endregion
    }
}
