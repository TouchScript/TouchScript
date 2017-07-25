/*
 * @author Valentin Simonov / http://va.lent.in/
 */

#if TOUCHSCRIPT_DEBUG

using System;
using System.Collections.Generic;
using System.IO;
using TouchScript.Debugging.Filters;
using TouchScript.Pointers;
using UnityEngine;

namespace TouchScript.Debugging.Loggers
{
    public class FileReaderLogger : IPointerLogger
    {
        public const int MIN_POINTER_LIST_SIZE = 1000;

        private int pointerCount = 0;
        private BinaryReader reader;

        protected List<PointerData> data = new List<PointerData>(1);
        protected List<List<PointerLog>> events = new List<List<PointerLog>>(1);

        /// <inheritdoc />
        public int PointerCount
        {
            get { return pointerCount; }
        }

        public FileReaderLogger(string path)
        {
            try
            {
                reader = new BinaryReader(new FileStream(path, FileMode.Open));
            }
            catch (IOException e)
            {
                Debug.LogFormat("Error opening file at '{0}'. {1}", path, e.Message);
            }

            try
            {
                while (true)
                {
                    var type = (Pointer.PointerType) reader.ReadUInt32();
                    var log = new PointerLog()
                    {
                        Id = reader.ReadInt32(),
                        Tick = reader.ReadInt64(),
                        PointerId = reader.ReadInt32(),
                        Event = (PointerEvent) reader.ReadUInt32(),
                        State = new PointerState()
                        {
                            Buttons = (Pointer.PointerButtonState) reader.ReadUInt32(),
                            Position = new Vector2(reader.ReadSingle(), reader.ReadSingle()),
                            PreviousPosition = new Vector2(reader.ReadSingle(), reader.ReadSingle()),
                            Flags = reader.ReadUInt32(),
                            Target = null,
                            TargetPath = reader.ReadString(),
                        }
                    };

                    checkId(log.PointerId, type);
                    var list = getPointerList(log.PointerId);
                    list.Add(log);
                }
            }
            finally
            {
                reader.Close();
            }
        }

        /// <inheritdoc />
        public void Log(Pointer pointer, PointerEvent evt)
        {
            throw new NotImplementedException("FileReaderLogger doesn't support writing data.");
        }

        /// <inheritdoc />
        public List<PointerData> GetFilteredPointerData(IPointerDataFilter filter = null)
        {
            //if (filter == null) 
            return new List<PointerData>(data);
        }

        /// <inheritdoc />
        public List<PointerLog> GetFilteredLogsForPointer(int id, IPointerLogFilter filter = null)
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

        public void Dispose() {}

        private IList<PointerLog> getPointerList(int id)
        {
            return events[id];
        }

        private void checkId(int id, Pointer.PointerType type)
        {
            if (id > pointerCount) throw new InvalidOperationException("Pointer id desync!");
            else if (id == pointerCount)
            {
                var list = new List<PointerLog>(MIN_POINTER_LIST_SIZE);
                events.Add(list);
                data.Add(new PointerData()
                {
                    Id = id,
                    Type = type,
                });
                pointerCount++;
            }
        }
    }
}

#endif