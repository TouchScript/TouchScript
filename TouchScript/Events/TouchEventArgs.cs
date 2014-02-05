/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using System.Collections.Generic;

namespace TouchScript.Events
{
    /// <summary>
    /// Touch event arguments.
    /// </summary>
    public class TouchEventArgs : EventArgs
    {
        /// <summary>
        /// List of touch points participating in the event.
        /// </summary>
        public List<TouchPoint> TouchPoints { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TouchEventArgs"/> class.
        /// </summary>
        /// <param name="touchPoints">List of touch points.</param>
        public TouchEventArgs(List<TouchPoint> touchPoints)
        {
            TouchPoints = touchPoints;
        }
    }
}