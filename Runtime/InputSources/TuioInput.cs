using System.Collections.Generic;
using TouchScript.Pointers;
using TouchScript.Utils;
using TuioNet.Common;
using UnityEngine;

namespace TouchScript.InputSources
{
    public abstract class TuioInput : InputSource
    {
        [SerializeField] protected TuioConnectionType _connectionType;
        [SerializeField] protected int _port = 3333;
        [SerializeField] protected string _ipAddress = "127.0.0.1";
        protected bool IsInitialized = false;

        protected readonly Dictionary<uint, TouchPointer> TouchToInternalId = new();
        protected readonly Dictionary<uint, ObjectPointer> ObjectToInternalId = new();

        private readonly ObjectPool<TouchPointer> _touchPool;
        private readonly ObjectPool<ObjectPointer> _objectPool;

        protected TuioInput()
        {
            _touchPool = new ObjectPool<TouchPointer>(50, () => new TouchPointer(this), null, ResetPointer);
            _objectPool = new ObjectPool<ObjectPointer>(50, () => new ObjectPointer(this), null, ResetPointer);
        }

        protected abstract void Connect();
        protected abstract void Disconnect();

        protected override void OnEnable()
        {
            base.OnEnable();
            ScreenWidth = Screen.width;
            ScreenHeight = Screen.height;
        }

        protected override void OnDisable()
        {
            foreach (var touchPointer in TouchToInternalId.Values)
            {
                CancelPointer(touchPointer);
            }

            foreach (var objectPointer in ObjectToInternalId.Values)
            {
                CancelPointer(objectPointer);
            }
            
            TouchToInternalId.Clear();
            ObjectToInternalId.Clear();
            Disconnect();
            base.OnDisable();
        }

        public override bool CancelPointer(Pointer pointer, bool shouldReturn)
        {
            lock (this)
            {
                if (pointer.Type == Pointer.PointerType.Touch)
                {
                    uint? touchId = null;
                    foreach (var kvp in TouchToInternalId)
                    {
                        if(kvp.Value.Id != pointer.Id) continue;
                        touchId = kvp.Key;
                        break;
                    }

                    if (touchId == null) return false;
                    CancelPointer(pointer);
                    if (shouldReturn)
                    {
                        TouchToInternalId[touchId.Value] = ReturnTouch(pointer as TouchPointer);
                    }
                    else
                    {
                        TouchToInternalId.Remove(touchId.Value);
                    }

                    return true;
                }

                uint? objectId = null;
                foreach (var kvp in ObjectToInternalId)
                {
                    if (kvp.Value.Id != pointer.Id) continue;
                    objectId = kvp.Key;
                    break;
                }

                if (objectId == null) return false;
                CancelPointer(pointer);
                if (shouldReturn)
                {
                    ObjectToInternalId[objectId.Value] = ReturnObject(pointer as ObjectPointer);
                }
                else
                {
                    ObjectToInternalId.Remove(objectId.Value);
                }

                return true;
            }
        }

        public override void INTERNAL_DiscardPointer(Pointer pointer)
        {
            switch (pointer.Type)
            {
                case Pointer.PointerType.Touch:
                    _touchPool.Release(pointer as TouchPointer);
                    break;
                case Pointer.PointerType.Object:
                    _objectPool.Release(pointer as ObjectPointer);
                    break;
            }
        }
        
        protected ObjectPointer AddObject(Vector2 position)
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
        
        protected TouchPointer AddTouch(Vector2 position)
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

        protected override void UpdateCoordinatesRemapper(ICoordinatesRemapper remapper)
        {
            
        }
        
        private void ResetPointer(Pointer pointer)
        {
            pointer.INTERNAL_Reset();
        }
    }
}