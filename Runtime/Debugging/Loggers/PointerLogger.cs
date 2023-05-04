/*
 * @author Valentin Simonov / http://va.lent.in/
 */

#if TOUCHSCRIPT_DEBUG

using System;
using System.Collections.Generic;
using TouchScript.Debugging.Filters;
using TouchScript.Pointers;
using TouchScript.Utils;

namespace TouchScript.Debugging.Loggers
{
    /// <summary>
    /// A default implementation of <see cref="IPointerLogger"/> used in editor.
    /// </summary>
    /// <seealso cref="TouchScript.Debugging.Loggers.IPointerLogger" />
    public class PointerLogger : IPointerLogger
    {
        #region Consts

        public const int MIN_POINTER_LIST_SIZE = 1000;

        #endregion

        #region Public properties

        /// <inheritdoc />
        public int PointerCount
        {
            get { return pointerCount; }
        }

        #endregion

        #region Private variables

        private int pointerCount = 0;
        private int eventCount = 0;

        protected List<PointerData> data = new List<PointerData>(1);
        protected List<List<PointerLog>> events = new List<List<PointerLog>>(1);

        #endregion

        #region Public methods

        /// <inheritdoc />
        public virtual void Log(Pointer pointer, PointerEvent evt)
        {
            var id = checkId(pointer);

            var list = getPointerList(id);
            var log = new PointerLog()
            {
                Id = eventCount,
                Tick = DateTime.Now.Ticks,
                PointerId = id,
                Event = evt,
                State = new PointerState()
                {
                    Buttons = pointer.Buttons,
                    Position = pointer.Position,
                    PreviousPosition = pointer.PreviousPosition,
                    Flags = pointer.Flags,
                    Target = pointer.GetPressData().Target,
                    TargetPath = TransformUtils.GetHeirarchyPath(pointer.GetPressData().Target),
                }
            };
            list.Add(log);
            eventCount++;
        }

        /// <inheritdoc />
        public virtual List<PointerData> GetFilteredPointerData(IPointerDataFilter filter = null)
        {
            //if (filter == null) 
            return new List<PointerData>(data);
        }

        /// <inheritdoc />
        public virtual List<PointerLog> GetFilteredLogsForPointer(int id, IPointerLogFilter filter = null)
        {
            if (id < 0 || id >= pointerCount)
                return new List<PointerLog>();

            List<PointerLog> list = events[id];
            if (filter == null)
                return new List<PointerLog>(list);

            var count = list.Count;
            List<PointerLog> filtered = new List<PointerLog>(count);
            for (var i = 0; i < count; i++)
            {
                var item = list[i];
                if (filter.Applies(ref item)) filtered.Add(item);
            }
            return filtered;
        }

        /// <inheritdoc />
        public virtual void Dispose() {}

        #endregion

        #region Private functions

        private IList<PointerLog> getPointerList(int id)
        {
            return events[id];
        }

        private int checkId(Pointer pointer)
        {
            var id = pointer.Id;
            if (id > pointerCount) throw new InvalidOperationException("Pointer id desync!");
            if (id != pointerCount) return id;

            var list = new List<PointerLog>(MIN_POINTER_LIST_SIZE);
            events.Add(list);
            data.Add(new PointerData()
            {
                Id = id,
                Type = pointer.Type,
            });
            pointerCount++;

            return id;
        }

        #endregion
    }
}

#endif