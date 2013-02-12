/**
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using System.Collections.Generic;

namespace TouchScript.Events
{
    /// <summary>
    /// Touch event arguments
    /// </summary>
    public class TouchEventArgs : EventArgs
    {
        public List<TouchPoint> TouchPoints;

        public TouchEventArgs(List<TouchPoint> touchPoints)
        {
            TouchPoints = touchPoints;
        }
    }
}