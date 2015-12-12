/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace TouchScript.Utils
{
    /// <summary>
    /// Utils to manipulate clusters of points.
    /// </summary>
    public static class ClusterUtils
    {
        private static StringBuilder hashString = new StringBuilder();

        /// <summary>
        /// Calculates the centroid of touch positions.
        /// </summary>
        /// <param name="touches">List of touch points.</param>
        /// <returns>Centroid of touch points' positions or <see cref="TouchManager.INVALID_POSITION"/> if cluster contains no points.</returns>
        public static Vector2 Get2DCenterPosition(IList<TouchPoint> touches)
        {
            var count = touches.Count;
            if (count == 0) return TouchManager.INVALID_POSITION;
            if (count == 1) return touches[0].Position;

            var position = new Vector2();
            for (var i = 0; i < count; i++) position += touches[i].Position;
            return position / count;
        }

        /// <summary>
        /// Calculates the centroid of previous touch positions.
        /// </summary>
        /// <param name="touches">List of touch points.</param>
        /// <returns>Centroid of previous touch point's positions or <see cref="TouchManager.INVALID_POSITION"/> if cluster contains no points.</returns>
        public static Vector2 GetPrevious2DCenterPosition(IList<TouchPoint> touches)
        {
            var count = touches.Count;
            if (count == 0) return TouchManager.INVALID_POSITION;
            if (count == 1) return touches[0].PreviousPosition;

            var position = new Vector2();
            for (var i = 0; i < count; i++) position += touches[i].PreviousPosition;
            return position / count;
        }

        /// <summary>
        /// Computes a unique hash for a list of touches.
        /// </summary>
        /// <param name="touches">List of touch points.</param>
        /// <returns>A unique string for a list of touches.</returns>
        public static string GetPointsHash(IList<TouchPoint> touches)
        {
            hashString.Remove(0, hashString.Length);
            for (var i = 0; i < touches.Count; i++)
            {
                hashString.Append("#");
                hashString.Append(touches[i].Id);
            }
            return hashString.ToString();
        }
    }
}