/*
 * @author Valentin Simonov / http://va.lent.in/
 */

#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_IOS || UNITY_ANDROID
using System.Collections.Generic;
using TouchScript.Pointers;
using TouchScript.Utils;
using TuioNet.Common;
using TuioNet.Tuio11;
using UnityEngine;

namespace TouchScript.InputSources
{
    /// <summary>
    /// Processes TUIO 1.1 input.
    /// </summary>
    [AddComponentMenu("TouchScript/Input Sources/TUIO 1.1 Input")]
    public sealed class Tuio11Input : TuioInput, ITuio11CursorListener, ITuio11ObjectListener
    {
        private Tuio11Client _client;

        protected override void Init()
        {
            if (IsInitialized) return;
            _client = new Tuio11Client(_connectionType, _ipAddress, _port, false);
            Connect();
            IsInitialized = true;
        }

        protected override void Connect()
        {
            _client?.Connect();
            _client?.AddCursorListener(this);
            _client?.AddObjectListener(this);
        }

        protected override void Disconnect()
        {
            _client?.RemoveAllTuioListeners();
            _client?.Disconnect();
        }

        public override bool UpdateInput()
        {
            _client.ProcessMessages();
            return true;
        }

        public void AddObject(Tuio11Object tuio11Object)
        {
            lock (this)
            {
                var screenPosition = new Vector2
                {
                    x = tuio11Object.Position.X * ScreenWidth,
                    y = (1f - tuio11Object.Position.Y) * ScreenHeight
                };
                var objectPointer = AddObject(screenPosition);
                UpdateObjectProperties(objectPointer, tuio11Object);
                ObjectToInternalId.Add(tuio11Object.SymbolId, objectPointer);
            }
        }

        public void UpdateObject(Tuio11Object tuio11Object)
        {
            lock (this)
            {
                if (!ObjectToInternalId.TryGetValue(tuio11Object.SymbolId, out var objectPointer)) return;
                var screenPosition = new Vector2
                {
                    x = tuio11Object.Position.X * ScreenWidth,
                    y = (1f - tuio11Object.Position.Y) * ScreenHeight
                };
                objectPointer.Position = RemapCoordinates(screenPosition);
                UpdateObjectProperties(objectPointer, tuio11Object);
                UpdatePointer(objectPointer);
            }
        }

        public void RemoveObject(Tuio11Object tuio11Object)
        {
            lock (this)
            {
                if (!ObjectToInternalId.TryGetValue(tuio11Object.SymbolId, out var objectPointer)) return;
                ObjectToInternalId.Remove(tuio11Object.SymbolId);
                ReleasePointer(objectPointer);
                RemovePointer(objectPointer);
            }
        }

        public void AddCursor(Tuio11Cursor tuio11Cursor)
        {
            lock(this)
            {
                var screenPosition = new Vector2
                {
                    x = tuio11Cursor.Position.X * ScreenWidth,
                    y = (1f - tuio11Cursor.Position.Y) * ScreenHeight
                };
                TouchToInternalId.Add(tuio11Cursor.CursorId, AddTouch(screenPosition));
            }
        }

        public void UpdateCursor(Tuio11Cursor tuio11Cursor)
        {
            lock (this)
            {
                if (!TouchToInternalId.TryGetValue(tuio11Cursor.CursorId, out var touchPointer)) return;
                var screenPosition = new Vector2
                {
                    x = tuio11Cursor.Position.X * ScreenWidth,
                    y = (1f - tuio11Cursor.Position.Y) * ScreenHeight
                };
                touchPointer.Position = RemapCoordinates(screenPosition);
                UpdatePointer(touchPointer);
            }
        }

        public void RemoveCursor(Tuio11Cursor tuio11Cursor)
        {
            lock (this)
            {
                if (!TouchToInternalId.TryGetValue(tuio11Cursor.CursorId, out var touchPointer)) return;
                TouchToInternalId.Remove(tuio11Cursor.CursorId);
                ReleasePointer(touchPointer);
                RemovePointer(touchPointer);
            }
        }

        private void UpdateObjectProperties(ObjectPointer pointer, Tuio11Object tuio11Object)
        {
            pointer.ObjectId = (int)tuio11Object.SymbolId;
            pointer.Angle = tuio11Object.Angle;
        }
    }
}

#endif