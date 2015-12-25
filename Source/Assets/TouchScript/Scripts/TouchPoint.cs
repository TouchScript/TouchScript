/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System.Collections.Generic;
using TouchScript.Hit;
using TouchScript.InputSources;
using TouchScript.Layers;
using UnityEngine;

namespace TouchScript
{
    /// <summary>
    /// <para>Representation of a finger within TouchScript.</para>
    /// <para>An object implementing this interface is created when user touches the screen. A unique id is assigned to it which doesn't change throughout its life.</para>
    /// <para><b>Attention!</b> Do not store references to these objects beyond touch's lifetime (i.e. when target finger is lifted off). These objects may be reused internally. Store unique ids instead.</para>
    /// </summary>
    public class TouchPoint
    {
        #region Public properties

        /// <summary>
        /// Internal unique touch point id.
        /// </summary>
        public int Id { get; private set; }

        /// <summary>
        /// Original hit target.
        /// </summary>
        public Transform Target { get; internal set; }

        /// <summary>
        /// Current position in screen coordinates.
        /// </summary>
        public Vector2 Position
        {
            get { return position; }
        }

        /// <summary>
        /// Previous position (during last frame) in screen coordinates.
        /// </summary>
        public Vector2 PreviousPosition { get; private set; }

        /// <summary>
        /// Original hit information.
        /// </summary>
        public TouchHit Hit { get; internal set; }

        /// <summary>
        /// Original layer which registered this touch.
        /// <seealso cref="TouchLayer"/>
        /// <seealso cref="CameraLayer"/>
        /// <seealso cref="CameraLayer2D"/>
        /// </summary>
        public TouchLayer Layer { get; internal set; }

        /// <summary>
        /// Original input source which created this touch.
        /// <seealso cref="IInputSource"/>
        /// </summary>
        public IInputSource InputSource { get; internal set; }

        /// <summary>
        /// Projection parameters for the layer which created this touch.
        /// </summary>
        public ProjectionParams ProjectionParams
        {
            get { return Layer.GetProjectionParams(this); }
        }

        /// <summary>
        /// Tags collection for this touch object.
        /// </summary>
        public Tags Tags { get; private set; }

        /// <summary>
        /// List of custom properties (key-value pairs) for this touch object.
        /// </summary>
        public Dictionary<string, object> Properties
        {
            get { return properties; }
            set { properties = value; }
        }

        #endregion

        #region Private variables

        private int refCount = 0;
        private Vector2 position = Vector2.zero;
        private Vector2 newPosition = Vector2.zero;
        private Dictionary<string, object> properties;

        #endregion

        #region Public methods

        /// <inheritdoc />
        public override bool Equals(object other)
        {
            return Equals(other as TouchPoint);
        }

        /// <inheritdoc />
        public bool Equals(TouchPoint other)
        {
            if (other == null)
                return false;

            return Id == other.Id;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Id;
        }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="TouchPoint"/> class.
        /// </summary>
        public TouchPoint()
        {
            properties = new Dictionary<string, object>();
        }

        #region Internal methods

        /// <summary>
        /// Initializes a new instance of the <see cref="TouchPoint" /> class.
        /// </summary>
        /// <param name="id">Unique id of the touch.</param>
        /// <param name="position">Screen position of the touch.</param>
        /// <param name="input">Input source which created this touch.</param>
        /// <param name="tags">Initial tags.</param>
        internal void INTERNAL_Init(int id, Vector2 position, IInputSource input, Tags tags)
        {
            Id = id;
            InputSource = input;
            this.position = PreviousPosition = newPosition = position;
            Tags = tags ?? Tags.EMPTY;
        }

        internal void INTERNAL_Reset()
        {
            refCount = 0;
            Hit = default(TouchHit);
            Target = null;
            Layer = null;
            Tags = null;
            properties.Clear();
        }

        internal void INTERNAL_ResetPosition()
        {
            PreviousPosition = position;
            position = newPosition;
            newPosition = position;
        }

        internal void INTERNAL_SetPosition(Vector2 value)
        {
            newPosition = value;
        }

        internal void INTERNAL_Retain()
        {
            refCount++;
        }

        internal int INTERNAL_Release()
        {
            return --refCount;
        }

        #endregion
    }
}