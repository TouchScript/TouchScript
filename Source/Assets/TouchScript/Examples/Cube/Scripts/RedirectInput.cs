/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using UnityEngine;
using System.Collections.Generic;
using TouchScript.Gestures;
using TouchScript.Hit;
using TouchScript.InputSources;
using TouchScript.Pointers;
using TouchScript.Utils;

namespace TouchScript.Examples.Cube
{
    /// <exclude />
    public class RedirectInput : InputSource
    {

        public int Width = 512;
        public int Height = 512;

        private MetaGesture gesture;
        private Dictionary<int, Pointer> map = new Dictionary<int, Pointer>();

        public override bool CancelPointer(Pointer pointer, bool shouldReturn)
        {
            base.CancelPointer(pointer, shouldReturn);

            map.Remove(pointer.Id);
            if (shouldReturn)
            {
                HitData hit;
                if (PointerUtils.IsPointerOnTarget(pointer, transform, out hit))
                {
                    var newPointer = PointerFactory.Create(pointer.Type, this);
                    newPointer.CopyFrom(pointer);
                    newPointer.Position = processCoords(hit.RaycastHit.textureCoord);
                    addPointer(newPointer);
                    pressPointer(newPointer);
                    map.Add(pointer.Id, newPointer);
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
                gesture.PointerUpdated += pointerUpdatedHandler;
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
                gesture.PointerUpdated -= pointerUpdatedHandler;
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
			if (pointer.InputSource == (IInputSource)this) return;

            var newPointer = PointerFactory.Create(pointer.Type, this);
            newPointer.CopyFrom(pointer);
            newPointer.Position = processCoords(pointer.GetPressData().RaycastHit.textureCoord);
            newPointer.Flags = pointer.Flags | Pointer.FLAG_ARTIFICIAL | Pointer.FLAG_INTERNAL;
            addPointer(newPointer);
            pressPointer(newPointer);
            map.Add(pointer.Id, newPointer);
        }

        private void pointerUpdatedHandler(object sender, MetaGestureEventArgs metaGestureEventArgs)
        {
			var pointer = metaGestureEventArgs.Pointer;

			if (pointer.InputSource == (IInputSource)this) return;

            Pointer newPointer;
            if (!map.TryGetValue(pointer.Id, out newPointer)) return;
            HitData hit;
            if (!PointerUtils.IsPointerOnTarget(pointer, transform, out hit)) return;
            newPointer.Position = processCoords(hit.RaycastHit.textureCoord);
            newPointer.Flags = pointer.Flags | Pointer.FLAG_ARTIFICIAL;
            updatePointer(newPointer);
        }

        private void pointerReleasedHandler(object sender, MetaGestureEventArgs metaGestureEventArgs)
        {
            var pointer = metaGestureEventArgs.Pointer;
			if (pointer.InputSource == (IInputSource)this) return;

            Pointer newPointer;
            if (!map.TryGetValue(pointer.Id, out newPointer)) return;
            map.Remove(pointer.Id);
            releasePointer(newPointer);
            removePointer(newPointer);
        }

        private void pointerCancelledhandler(object sender, MetaGestureEventArgs metaGestureEventArgs)
        {
            var pointer = metaGestureEventArgs.Pointer;
			if (pointer.InputSource == (IInputSource)this) return;

            Pointer newPointer;
            if (!map.TryGetValue(pointer.Id, out newPointer)) return;
            map.Remove(pointer.Id);
            cancelPointer(newPointer);
        }

    }

}