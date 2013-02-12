/**
 * @author Valentin Simonov / http://va.lent.in/
 */

using TouchScript.Layers;
using UnityEngine;

namespace TouchScript
{
    /// <summary>
    /// Touch point.
    /// </summary>
    public class TouchPoint
    {
        public TouchPoint(int id, Vector2 position)
        {
            Id = id;
            Position = position;
            PreviousPosition = position;
        }

        /// <summary>
        /// Internal unique touch point id.
        /// </summary>
        public int Id { get; private set; }

        private Vector2 position = Vector2.zero;

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
        public RaycastHit Hit { get; internal set; }

        /// <summary>
        /// Original camera through which the target was seen.
        /// </summary>
        public TouchLayer Layer { get; internal set; }
    }
}