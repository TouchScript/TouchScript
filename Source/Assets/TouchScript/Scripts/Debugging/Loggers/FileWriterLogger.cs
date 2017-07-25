/*
 * @author Valentin Simonov / http://va.lent.in/
 */

#if TOUCHSCRIPT_DEBUG

using System;
using System.Collections.Generic;
using System.IO;
using TouchScript.Debugging.Filters;
using TouchScript.Pointers;
using TouchScript.Utils;
using UnityEngine;

namespace TouchScript.Debugging.Loggers
{
    public class FileWriterLogger : IPointerLogger
    {
        private int eventCount = 0;
        private BinaryWriter writer;

        /// <inheritdoc />
        public int PointerCount
        {
            get { throw new NotImplementedException("FileWriterLogger doesn't support reading data."); }
        }

        public FileWriterLogger()
        {
            var path = Path.Combine(Application.dataPath, "../TouchEvents.bin");
            try
            {
                writer = new BinaryWriter(new FileStream(path, FileMode.Create));
            }
            catch (IOException e)
            {
                Debug.LogFormat("Error creating file at '{0}'. {1}", path, e.Message);
            }
        }

        /// <inheritdoc />
        public void Log(Pointer pointer, PointerEvent evt)
        {
            var path = TransformUtils.GetHeirarchyPath(pointer.GetPressData().Target);

            writer.Write((uint) pointer.Type);
            writer.Write(eventCount);
            writer.Write(DateTime.Now.Ticks);
            writer.Write(pointer.Id);
            writer.Write((uint) evt);
            writer.Write((uint) pointer.Buttons);
            writer.Write(pointer.Position.x);
            writer.Write(pointer.Position.y);
            writer.Write(pointer.PreviousPosition.x);
            writer.Write(pointer.PreviousPosition.y);
            writer.Write(pointer.Flags);
            writer.Write(path ?? "");

            eventCount++;
        }

        /// <inheritdoc />
        public List<PointerData> GetFilteredPointerData(IPointerDataFilter filter = null)
        {
            throw new NotImplementedException("FileWriterLogger doesn't support reading data.");
        }

        /// <inheritdoc />
        public List<PointerLog> GetFilteredLogsForPointer(int id, IPointerLogFilter filter = null)
        {
            throw new NotImplementedException("FileWriterLogger doesn't support reading data.");
        }

        public void Dispose()
        {
            if (writer != null) writer.Close();
        }
    }
}

#endif