using TouchScript.Hit;
using TouchScript.Layers;
using UnityEngine;

namespace TouchScript
{
    public interface ITouch
    {
        /// <summary>
        /// Internal unique touch point id.
        /// </summary>
        int Id { get; }

        /// <summary>
        /// Original hit target.
        /// </summary>
        Transform Target { get; }

        /// <summary>
        /// Current touch position.
        /// </summary>
        Vector2 Position { get; }

        /// <summary>
        ///Previous position.
        /// </summary>
        Vector2 PreviousPosition { get; }

        /// <summary>
        /// Original hit information.
        /// </summary>
        ITouchHit Hit { get; }

        /// <summary>
        /// Original camera through which the target was seen.
        /// </summary>
        TouchLayer Layer { get; }
    }
}