using System;
using System.Collections.Generic;

namespace TouchScript.Utils
{
    internal sealed class TimedTouchSequence
    {
        private List<ITouch> points = new List<ITouch>();
        private List<float> timestamps = new List<float>();

        public TimedTouchSequence()
        {}

        public void Add(ITouch touch, float time)
        {
            points.Add(touch);
            timestamps.Add(time);
        }

        public void Clear()
        {
            points.Clear();
            timestamps.Clear();
        }

        public IList<ITouch> FindTouchPointsLaterThan(float time)
        {
            var list = new List<ITouch>();
            for (var i = points.Count - 1; i >= 0; i--)
            {
                if (timestamps[i] > time) list.Add(points[i]);
                else break;
            }
            list.Reverse();
            return list;
        }

        public IList<ITouch> FindTouchPointsLaterThan(float time, Predicate<ITouch> predicate)
        {
            var list = new List<ITouch>();
            for (var i = points.Count - 1; i >= 0; i--)
            {
                if (timestamps[i] > time)
                {
                    if (predicate(points[i])) list.Add(points[i]);
                } else break;
            }
            list.Reverse();
            return list;
        }
    }
}