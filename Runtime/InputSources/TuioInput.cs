/*
 * @author Valentin Simonov / http://va.lent.in/
 */

// #if TOUCHSCRIPT_TUIO
// #if UNITY_EDITOR || UNITY_STANDALONE || UNITY_IOS || UNITY_ANDROID
using System;
using System.Collections.Generic;
using TouchScript.Pointers;
using TouchScript.Utils;
using TuioNet;
using TuioNet.Common;
using TuioNet.Tuio11;
using UnityEngine;

namespace TouchScript.InputSources
{
    /// <summary>
    /// Processes TUIO 1.1 input.
    /// </summary>
    [AddComponentMenu("TouchScript/Input Sources/TUIO Input")]
    [HelpURL("http://touchscript.github.io/docs/html/T_TouchScript_InputSources_TuioInput.htm")]
    public sealed class TuioInput : InputSource, ITuio11Listener
    {
        [SerializeField] private TuioConnectionType _connectionType;
        [SerializeField] private int _port = 3333;
        [SerializeField] private string _ipAddress = "127.0.0.1";
        private bool _isInitialized = false;
        private Tuio11Client _client;

        private Dictionary<Tuio11Cursor, TouchPointer> _cursorToInternalId = new();

        private Dictionary<Tuio11Object, ObjectPointer> _objectToInterlalId = new();

        private ObjectPool<TouchPointer> _touchPool;
        private ObjectPool<ObjectPointer> _objectPool;

        public TuioInput()
        {
            _touchPool = new ObjectPool<TouchPointer>(50, () => new TouchPointer(this), null, ResetPointer);
            _objectPool = new ObjectPool<ObjectPointer>(50, () => new ObjectPointer(this), null, ResetPointer);
        }
        
        protected override void Init()
        {
            if (_isInitialized) return;
            _client = new Tuio11Client(_connectionType, _ipAddress, _port, false);
            Connect();
            _isInitialized = true;
        }

        private void Connect()
        {
            _client?.Connect();
            _client?.AddTuioListener(this);
        }

        private void Disconnect()
        {
            _client?.RemoveAllTuioListeners();
            _client?.Disconnect();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            ScreenWidth = Screen.width;
            ScreenHeight = Screen.height;
        }

        protected override void OnDisable()
        {
            foreach (var kvp in _cursorToInternalId)
            {
                CancelPointer(kvp.Value);
            }

            foreach (var kvp in _objectToInterlalId)
            {
                CancelPointer(kvp.Value);
            }
            
            _cursorToInternalId.Clear();
            _objectToInterlalId.Clear();
            Disconnect();
            base.OnDisable();
        }

        public override bool UpdateInput()
        {
            _client.ProcessMessages();
            return true;
        }

        public override bool CancelPointer(Pointer pointer, bool shouldReturn)
        {
            lock (this)
            {
                if (pointer.Type == Pointer.PointerType.Touch)
                {
                    Tuio11Cursor cursor = null;
                    foreach (var kvp in _cursorToInternalId)
                    {
                        if (kvp.Value.Id != pointer.Id) continue;
                        cursor = kvp.Key;
                        break;
                    }

                    if (cursor == null) return false;
                    CancelPointer(pointer);
                    if (shouldReturn)
                    {
                        _cursorToInternalId[cursor] = ReturnTouch(pointer as TouchPointer);
                    }
                    else
                    {
                        _cursorToInternalId.Remove(cursor);
                    }

                    return true;
                }
                return false;
            }
        }

        public override void INTERNAL_DiscardPointer(Pointer pointer)
        {
            if (pointer.Type == Pointer.PointerType.Touch)
            {
                _touchPool.Release(pointer as TouchPointer);
            }
        }

        protected override void UpdateCoordinatesRemapper(ICoordinatesRemapper remapper)
        {
            
        }

        private void ResetPointer(Pointer pointer)
        {
            pointer.INTERNAL_Reset();
        }

        public void AddTuioObject(Tuio11Object tuio11Object)
        {
            lock (this)
            {
                var screenPosition = new Vector2
                {
                    x = tuio11Object.Position.X * ScreenWidth,
                    y = (1f - tuio11Object.Position.Y) * ScreenHeight
                };
                var touch = AddObject(screenPosition);
                UpdateObjectProperties(touch, tuio11Object);
                _objectToInterlalId.Add(tuio11Object, touch);
            }
        }

        public void UpdateTuioObject(Tuio11Object tuio11Object)
        {
            lock (this)
            {
                if (!_objectToInterlalId.TryGetValue(tuio11Object, out var touch)) return;
                var screenPosition = new Vector2
                {
                    x = tuio11Object.Position.X * ScreenWidth,
                    y = (1f - tuio11Object.Position.Y) * ScreenHeight
                };
                touch.Position = RemapCoordinates(screenPosition);
                UpdateObjectProperties(touch, tuio11Object);
                UpdatePointer(touch);
            }
        }

        public void RemoveTuioObject(Tuio11Object tuio11Object)
        {
            lock (this)
            {
                if (!_objectToInterlalId.TryGetValue(tuio11Object, out var touch)) return;
                _objectToInterlalId.Remove(tuio11Object);
                ReleasePointer(touch);
                RemovePointer(touch);
            }
        }

        public void AddTuioCursor(Tuio11Cursor tuio11Cursor)
        {
            lock(this)
            {
                var screenPosition = new Vector2
                {
                    x = tuio11Cursor.Position.X * ScreenWidth,
                    y = (1f - tuio11Cursor.Position.Y) * ScreenHeight
                };
                _cursorToInternalId.Add(tuio11Cursor, AddTouch(screenPosition));
            }
        }

        public void UpdateTuioCursor(Tuio11Cursor tuio11Cursor)
        {
            lock (this)
            {
                if (!_cursorToInternalId.TryGetValue(tuio11Cursor, out var touch)) return;
                var screenPosition = new Vector2
                {
                    x = tuio11Cursor.Position.X * ScreenWidth,
                    y = (1f - tuio11Cursor.Position.Y) * ScreenHeight
                };
                touch.Position = RemapCoordinates(screenPosition);
                UpdatePointer(touch);
            }
        }

        public void RemoveTuioCursor(Tuio11Cursor tuio11Cursor)
        {
            lock (this)
            {
                if (!_cursorToInternalId.TryGetValue(tuio11Cursor, out var touch)) return;
                _cursorToInternalId.Remove(tuio11Cursor);
                ReleasePointer(touch);
                RemovePointer(touch);
            }
        }

        public void AddTuioBlob(Tuio11Blob tuio11Blob)
        {
            throw new NotImplementedException();
        }

        public void UpdateTuioBlob(Tuio11Blob tuio11Blob)
        {
            throw new NotImplementedException();
        }

        public void RemoveTuioBlob(Tuio11Blob tuio11Blob)
        {
            throw new NotImplementedException();
        }

        public void Refresh(TuioTime tuioTime)
        {
            
        }

        private TouchPointer AddTouch(Vector2 position)
        {
            var pointer = _touchPool.Get();
            pointer.Position = RemapCoordinates(position);
            pointer.Buttons |= Pointer.PointerButtonState.FirstButtonDown |
                               Pointer.PointerButtonState.FirstButtonPressed;
            AddPointer(pointer);
            PressPointer(pointer);
            return pointer;
        }

        private TouchPointer ReturnTouch(TouchPointer pointer)
        {
            var newPointer = _touchPool.Get();
            newPointer.CopyFrom(pointer);
            pointer.Buttons |= Pointer.PointerButtonState.FirstButtonDown |
                Pointer.PointerButtonState.FirstButtonPressed;
            newPointer.Flags |= Pointer.FLAG_RETURNED;
            AddPointer(newPointer);
            PressPointer(newPointer);
            return newPointer;
        }

        private ObjectPointer AddObject(Vector2 position)
        {
            var pointer = _objectPool.Get();
            pointer.Position = RemapCoordinates(position);
            pointer.Buttons |= Pointer.PointerButtonState.FirstButtonDown |
                               Pointer.PointerButtonState.FirstButtonPressed;
            AddPointer(pointer);
            PressPointer(pointer);
            return pointer;
        }

        private ObjectPointer ReturnObject(ObjectPointer pointer)
        {
            var newPointer = _objectPool.Get();
            newPointer.CopyFrom(pointer);
            pointer.Buttons |= Pointer.PointerButtonState.FirstButtonDown |
                               Pointer.PointerButtonState.FirstButtonPressed;
            newPointer.Flags |= Pointer.FLAG_RETURNED;
            AddPointer(newPointer);
            PressPointer(newPointer);
            return newPointer;
        }
        
        private void UpdateObjectProperties(ObjectPointer touch, Tuio11Object tuio11Object)
        {
            touch.ObjectId = (int)tuio11Object.SymbolId;
            touch.Angle = tuio11Object.Angle;
        }
    }
}

// #endif
// #endif