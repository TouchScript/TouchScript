using UnityEngine;
using System.Collections.Generic;
using TouchScript.Gestures;
using TouchScript.Hit;
using TouchScript.InputSources;
using TouchScript.Pointers;

namespace TouchScript.Examples.Cube
{
    public class RedirectInput : InputSource
    {

        public int Width = 512;
        public int Height = 512;

        private MetaGesture gesture;
        private Dictionary<int, int> map = new Dictionary<int, int>();

        public override bool CancelPointer(Pointer pointer, bool shouldReturn)
        {
            base.CancelPointer(pointer, shouldReturn);

            map.Remove(pointer.Id);
            if (shouldReturn)
            {
                TouchHit hit;
                if (gesture.GetTargetHitResult(pointer.Position, out hit))
                {
                    var newPointer = PointerFactory.Create(pointer.Type, this);
                    newPointer.CopyFrom(pointer);
                    addPointer(newPointer, processCoords(pointer.Hit.RaycastHit.textureCoord));
                    pressPointer(newPointer.Id);
                    map.Add(pointer.Id, newPointer.Id);
                }
            }
			return true;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            gesture = GetComponent<MetaGesture>();
            if (gesture)
            {
                gesture.PointerPressed += pointerPressedHandler;
                gesture.PointerMoved += pointerMovedhandler;
                gesture.PointerCancelled += pointerCancelledhandler;
                gesture.PointerReleased += pointerReleasedHandler;
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            if (gesture)
            {
                gesture.PointerPressed -= pointerPressedHandler;
                gesture.PointerMoved -= pointerMovedhandler;
                gesture.PointerCancelled -= pointerCancelledhandler;
                gesture.PointerReleased -= pointerReleasedHandler;
            }
        }

        private Vector2 processCoords(Vector2 value)
        {
            return new Vector2(value.x * Width, value.y * Height);
        }

        private void pointerPressedHandler(object sender, MetaGestureEventArgs metaGestureEventArgs)
        {
            var pointer = metaGestureEventArgs.Pointer;
            if (pointer.InputSource == this) return;

            var newPointer = PointerFactory.Create(pointer.Type, this);
            newPointer.CopyFrom(pointer);
            addPointer(newPointer, processCoords(pointer.Hit.RaycastHit.textureCoord));
            pressPointer(newPointer.Id);
            map.Add(pointer.Id, newPointer.Id);
        }

        private void pointerMovedhandler(object sender, MetaGestureEventArgs metaGestureEventArgs)
        {
            int id;
            TouchHit hit;
            var pointer = metaGestureEventArgs.Pointer;
            if (pointer.InputSource == this) return;

            if (!map.TryGetValue(pointer.Id, out id)) return;
            if (!gesture.GetTargetHitResult(pointer.Position, out hit)) return;
            movePointer(id, processCoords(hit.RaycastHit.textureCoord));
        }

        private void pointerReleasedHandler(object sender, MetaGestureEventArgs metaGestureEventArgs)
        {
            int id;
            var pointer = metaGestureEventArgs.Pointer;
            if (pointer.InputSource == this) return;

            if (!map.TryGetValue(pointer.Id, out id)) return;
            map.Remove(pointer.Id);
            releasePointer(id);
            removePointer(id);
        }

        private void pointerCancelledhandler(object sender, MetaGestureEventArgs metaGestureEventArgs)
        {
            int id;
            var pointer = metaGestureEventArgs.Pointer;
            if (pointer.InputSource == this) return;

            if (!map.TryGetValue(pointer.Id, out id)) return;
            map.Remove(pointer.Id);
            cancelPointer(id);
        }

    }

}