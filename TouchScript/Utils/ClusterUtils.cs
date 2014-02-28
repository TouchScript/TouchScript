using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace TouchScript.Utils
{
    public static class ClusterUtils
    {
        private static StringBuilder hashString = new StringBuilder();

        /// <summary>
        /// Returns a camera which was used to capture cluster's points.
        /// </summary>
        /// <param name="touches">List of touch points.</param>
        /// <returns>Camera instance.</returns>
        public static Camera GetClusterCamera(IList<ITouchPoint> touches)
        {
            if (touches.Count == 0) return Camera.main;
            var cam = touches[0].Layer.Camera;
            if (cam == null) return Camera.main;
            return cam;
        }

        /// <summary>
        /// Calculates the centroid of touch points' positions.
        /// </summary>
        /// <param name="touches">List of touch points.</param>
        /// <returns>Centroid of touch points' positions or <see cref="TouchManager.INVALID_POSITION"/> if cluster contains no points.</returns>
        public static Vector2 Get2DCenterPosition(IList<ITouchPoint> touches)
        {
            var length = touches.Count;
            if (length == 0) return TouchManager.INVALID_POSITION;
            if (length == 1) return touches[0].Position;

            var position = new Vector2();
            foreach (var point in touches) position += point.Position;
            return position/(float)length;
        }

        /// <summary>
        /// Calculates the centroid of previous touch points' positions.
        /// </summary>
        /// <param name="touches">List of touch points.</param>
        /// <returns>Centroid of previous touch point's positions or <see cref="TouchManager.INVALID_POSITION"/> if cluster contains no points.</returns>
        public static Vector2 GetPrevious2DCenterPosition(IList<ITouchPoint> touches)
        {
            var length = touches.Count;
            if (length == 0) return TouchManager.INVALID_POSITION;
            if (length == 1) return touches[0].PreviousPosition;

            var position = new Vector2();
            foreach (var point in touches) position += point.PreviousPosition;
            return position/(float)length;
        }

        /// <summary>
        /// Computes a unique hash for a list of touch points.
        /// </summary>
        /// <param name="touches">List of touch points.</param>
        /// <returns>A unique string for a list of touch points.</returns>
        public static String GetPointsHash(IList<ITouchPoint> touches)
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