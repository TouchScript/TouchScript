/*
 * @author Valentin Simonov / http://va.lent.in/
 */

namespace TouchScript
{
    /// <summary>
    /// An interface for objects which can expose some kind of debug information which can be turned on and off.
    /// </summary>
    public interface IDebuggable
    {
        /// <summary>
        /// Gets or sets if this object should show its debug information.
        /// </summary>
        bool DebugMode { get; set; }
    }
}