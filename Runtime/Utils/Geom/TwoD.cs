/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using UnityEngine;

namespace TouchScript.Utils.Geom
{
    /// <summary>
    /// A class with 2D helper functions.
    /// </summary>
    public static class TwoD
    {
        /// <summary>
        /// Calculates distance from line to point.
        /// </summary>
        /// <param name="lineStart">Line "starting" point.</param>
        /// <param name="lineEnd">Line "ending" point.</param>
        /// <param name="point">Point to calculate the distance to.</param>
        /// <returns>Distance between point and line.</returns>
        public static float PointToLineDistance(Vector2 lineStart, Vector2 lineEnd, Vector2 point)
        {
            var dx = lineEnd.x - lineStart.x;
            var dy = lineEnd.y - lineStart.y;
            return (dy * point.x - dx * point.y + lineEnd.x * lineStart.y - lineEnd.y * lineStart.x) / Mathf.Sqrt(dx * dx + dy * dy);
        }

        /// <summary>
        /// Calculates distances from line to each of 2 points.
        /// </summary>
        /// <param name="lineStart">Line "starting" point.</param>
        /// <param name="lineEnd">Line "ending" point.</param>
        /// <param name="point1">Point to calculate the distance to.</param>
        /// <param name="point2">Point to calculate the distance to.</param>
        /// <param name="dist1">Contains returned distance from line to the first point.</param>
        /// <param name="dist2">Contains returned distance from line to the second point.</param>
        public static void PointToLineDistance2(Vector2 lineStart, Vector2 lineEnd, Vector2 point1, Vector2 point2,
                                                out float dist1, out float dist2)
        {
            var dx = lineEnd.x - lineStart.x;
            var dy = lineEnd.y - lineStart.y;
            var c = lineEnd.x * lineStart.y - lineEnd.y * lineStart.x;
            var length = Mathf.Sqrt(dx * dx + dy * dy);
            dist1 = (dy * point1.x - dx * point1.y + c) / length;
            dist2 = (dy * point2.x - dx * point2.y + c) / length;
        }

        /// <summary>
        /// Rotates a point around (0,0) by an angle.
        /// </summary>
        /// <param name="point">Point to rotate.</param>
        /// <param name="angle">Angle in degrees to rotate by.</param>
        /// <returns>Transformed point.</returns>
        public static Vector2 Rotate(Vector2 point, float angle)
        {
            var rad = angle * Mathf.Deg2Rad;
            var cos = Mathf.Cos(rad);
            var sin = Mathf.Sin(rad);
            return new Vector2(point.x * cos - point.y * sin, point.x * sin + point.y * cos);
        }
    }
}