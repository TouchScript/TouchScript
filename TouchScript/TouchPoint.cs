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
    public class TouchPoint
    {
        #region Constants

        /// <summary>
        /// The value of TouchPoint.Position in an unkown state.
        /// </summary>
        public static readonly Vector2 InvalidPosition = new Vector2(float.NaN, float.NaN);

        #endregion

        #region Public properties

        /// <summary>
        /// Internal unique touch point id.
        /// </summary>
        public int Id { get; private set; }

        /// <summary>
        /// Current touch position.
        /// </summary>
        public Vector2 Position
        {
            get { return position; }
            set
            {
                PreviousPosition = position;
                position = value;
            }
        }

        /// <summary>
        ///Previous position.
        /// </summary>
        public Vector2 PreviousPosition { get; private set; }

        /// <summary>
        /// Original hit target.
        /// </summary>
        public Transform Target { get; internal set; }

        /// <summary>
        /// Original hit information.
        /// </summary>
        public TouchHit Hit { get; internal set; }

        /// <summary>
        /// Original camera through which the target was seen.
        /// </summary>
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
        public TouchPoint(int id, Vector2 position)
        {
            Id = id;
            Position = position;
            PreviousPosition = position;
        }

        #region Public methods

        /// <summary>
        /// Determines whether position vector is invalid.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <returns>
        ///   <c>true</c> position is invalid; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsInvalidPosition(Vector2 position)
        {
            return position.Equals(InvalidPosition);
        }

        #endregion

        #region Internal methods

        internal void ResetPosition()
        {
            PreviousPosition = Position;
        }

        #endregion
    }
}