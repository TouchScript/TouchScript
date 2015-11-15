/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using UnityEngine;

namespace TouchScript.Utils.Geom
{

    public static class TwoD
    {
        public static float PointToLineDistance(Vector2 lineStart, Vector2 lineEnd, Vector2 point)
        {
            var dx = lineEnd.x - lineStart.x;
            var dy = lineEnd.y - lineStart.y;
            return (dy*point.x - dx*point.y + lineEnd.x*lineStart.y - lineEnd.y*lineStart.x)/Mathf.Sqrt(dx*dx + dy*dy);
        }

        public static void PointToLineDistance2(Vector2 lineStart, Vector2 lineEnd, Vector2 point1, Vector2 point2, out float dist1, out float dist2)
        {
            var dx = lineEnd.x - lineStart.x;
            var dy = lineEnd.y - lineStart.y;
            var c = lineEnd.x*lineStart.y - lineEnd.y*lineStart.x;
            var length = Mathf.Sqrt(dx*dx + dy*dy);
            dist1 = (dy * point1.x - dx * point1.y + c) / length;
            dist2 = (dy * point2.x - dx * point2.y + c) / length;
        }

        public static Vector2 Rotate(Vector2 point, float angle)
        {
            var rad = angle*Mathf.Deg2Rad;
            var cos = Mathf.Cos(rad);
            var sin = Mathf.Sin(rad);
            return new Vector2(point.x * cos - point.y * sin, point.x * sin + point.y * cos);
        }
    }
}
