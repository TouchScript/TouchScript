/*
 * @author DenizPiri / denizpiri@hotmail.com
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using UnityEngine;

namespace TouchScript.Utils
{
    /// <summary>
    /// Extension methods for event handling.
    /// </summary>
    public static class EventHandlerExtensions
    {
        /// <summary>
        /// Invokes an event handling exceptions.
        /// </summary>
        /// <typeparam name="T"> EventArgs type. </typeparam>
        /// <param name="handler"> Event. </param>
        /// <param name="sender"> Event sender. </param>
        /// <param name="args"> EventArgs. </param>
        /// <returns> The exception caught or <c>null</c>. </returns>
        public static Exception InvokeHandleExceptions<T>(this EventHandler<T> handler, object sender, T args)
            where T : EventArgs
        {
            try
            {
                handler(sender, args);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                return ex;
            }
            return null;
        }

        /// <summary>
        /// Invokes an event handling exceptions.
        /// </summary>
        /// <param name="handler"> Event. </param>
        /// <param name="sender"> Event sender. </param>
        /// <param name="args"> EventArgs. </param>
        /// <returns> The exception caught or <c>null</c>. </returns>
        public static Exception InvokeHandleExceptions(this EventHandler handler, object sender, EventArgs args)
        {
            try
            {
                handler(sender, args);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                return ex;
            }
            return null;
        }
    }
}