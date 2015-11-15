/*
 * @author DenizPiri / denizpiri@hotmail.com
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;

namespace TouchScript.Utils
{
    internal static class EventHandlerExtensions
    {
        public static Exception InvokeHandleExceptions<T>(this EventHandler<T> handler, object sender, T args)
            where T : EventArgs
        {
            try
            {
                handler(sender, args);
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogException(ex);
                return ex;
            }
            return null;
        }

        public static Exception InvokeHandleExceptions(this EventHandler handler, object sender, EventArgs args)
        {
            try
            {
                handler(sender, args);
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogException(ex);
                return ex;
            }
            return null;
        }
    }
}