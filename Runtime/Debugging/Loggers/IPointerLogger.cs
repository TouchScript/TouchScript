/*
 * @author Valentin Simonov / http://va.lent.in/
 */

#if TOUCHSCRIPT_DEBUG

using System;
using System.Collections.Generic;
using TouchScript.Debugging.Filters;
using TouchScript.InputSources;
using TouchScript.Pointers;
using UnityEngine;

namespace TouchScript.Debugging.Loggers
{
    /// <summary>
    /// A logger to record pointer events.
    /// </summary>
    public interface IPointerLogger
    {
        /// <summary>
        /// The number of different pointers recorded by this logger.
        /// </summary>
        int PointerCount { get; }

        /// <summary>
        /// Logs the specified event.
        /// </summary>
        /// <param name="pointer">The pointer.</param>
        /// <param name="evt">The event.</param>
        void Log(Pointer pointer, PointerEvent evt);

        /// <summary>
        /// Returns a list of pointers.
        /// </summary>
        /// <param name="filter">The filter to use.</param>
        /// <returns>A list of <see cref="PointerData"/> objects.</returns>
        List<PointerData> GetFilteredPointerData(IPointerDataFilter filter = null);

        /// <summary>
        /// Returns a lost of pointer events for a pointer.
        /// </summary>
        /// <param name="id">The pointer id.</param>
        /// <param name="filter">The filter to use.</param>
        /// <returns>A list of <see cref="PointerLog"/> entries.</returns>
        List<PointerLog> GetFilteredLogsForPointer(int id, IPointerLogFilter filter = null);

        /// <summary>
        /// Releases resources.
        /// </summary>
        void Dispose();
    }

    /// <summary>
    /// Pointer event.
    /// </summary>
    [Serializable]
    public struct PointerLog
    {
        public int Id;
        public long Tick;
        public int PointerId;
        public PointerEvent Event;
        public PointerState State;
    }

    /// <summary>
    /// Pointer state during an event.
    /// </summary>
    [Serializable]
    public struct PointerState
    {
        public Pointer.PointerButtonState Buttons;
        public Vector2 Position;
        public Vector2 PreviousPosition;
        public uint Flags;
        public Transform Target;
        public string TargetPath;
    }

    /// <summary>
    /// Static pointer data.
    /// </summary>
    [Serializable]
    public struct PointerData
    {
        public int Id;
        public Pointer.PointerType Type;
        public IInputSource InputSource;
    }

    /// <summary>
    /// Pointer event type.
    /// </summary>
    public enum PointerEvent
    {
        None,
        IdAllocated,
        Added,
        Updated,
        Pressed,
        Released,
        Removed,
        Cancelled
    }
}

#endif