/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System.Collections.Generic;
using System.Text;
using TouchScript.Pointers;
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
        /// Calculates the centroid of pointers' positions.
        /// </summary>
        /// <param name="pointers">List of pointers.</param>
        /// <returns>Centroid of pointers' positions or <see cref="TouchManager.INVALID_POSITION"/> if cluster contains no points.</returns>
        public static Vector2 Get2DCenterPosition(IList<Pointer> pointers)
        {
            var count = pointers.Count;
            if (count == 0) return TouchManager.INVALID_POSITION;
            if (count == 1) return pointers[0].Position;

            var position = new Vector2();
            for (var i = 0; i < count; i++) position += pointers[i].Position;
            return position / count;
        }

        /// <summary>
        /// Calculates the centroid of pointers' previous positions.
        /// </summary>
        /// <param name="pointers">List of pointers.</param>
        /// <returns>Centroid of pointers' previous positions or <see cref="TouchManager.INVALID_POSITION"/> if cluster contains no points.</returns>
        public static Vector2 GetPrevious2DCenterPosition(IList<Pointer> pointers)
        {
            var count = pointers.Count;
            if (count == 0) return TouchManager.INVALID_POSITION;
            if (count == 1) return pointers[0].PreviousPosition;

            var position = new Vector2();
            for (var i = 0; i < count; i++) position += pointers[i].PreviousPosition;
            return position / count;
        }

        /// <summary>
        /// Computes a unique hash for a list of pointers.
        /// </summary>
        /// <param name="pointers">List of pointers.</param>
        /// <returns>A unique string for a list of pointers.</returns>
        public static string GetPointsHash(IList<Pointer> pointers)
        {
            hashString.Remove(0, hashString.Length);
            for (var i = 0; i < pointers.Count; i++)
            {
                hashString.Append("#");
                hashString.Append(pointers[i].Id);
            }
            return hashString.ToString();
        }
    }
}