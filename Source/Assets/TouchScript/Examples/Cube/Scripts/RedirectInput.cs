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

        public override void CancelPointer(Pointer pointer, bool @return)
        {
            base.CancelPointer(pointer, @return);

            map.Remove(pointer.Id);
            if (@return)
            {
                TouchHit hit;
                if (!gesture.GetTargetHitResult(pointer.Position, out hit)) return;
                map.Add(pointer.Id, beginPointer(processCoords(hit.RaycastHit.textureCoord), pointer.Tags).Id);
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            gesture = GetComponent<MetaGesture>();
            if (gesture)
            {
                gesture.PointerBegan += pointerBeganHandler;
                gesture.PointerMoved += pointerMovedhandler;
                gesture.PointerCancelled += pointerCancelledhandler;
                gesture.PointerEnded += pointerEndedHandler;
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            if (gesture)
            {
                gesture.PointerBegan -= pointerBeganHandler;
                gesture.PointerMoved -= pointerMovedhandler;
                gesture.PointerCancelled -= pointerCancelledhandler;
                gesture.PointerEnded -= pointerEndedHandler;
            }
        }

        private Vector2 processCoords(Vector2 value)
        {
            return new Vector2(value.x * Width, value.y * Height);
        }

        private void pointerBeganHandler(object sender, MetaGestureEventArgs metaGestureEventArgs)
        {
            var pointer = metaGestureEventArgs.Pointer;
            if (pointer.InputSource == this) return;
            map.Add(pointer.Id, beginPointer(processCoords(pointer.Hit.RaycastHit.textureCoord), pointer.Tags).Id);
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

        private void pointerEndedHandler(object sender, MetaGestureEventArgs metaGestureEventArgs)
        {
            int id;
            var pointer = metaGestureEventArgs.Pointer;
            if (pointer.InputSource == this) return;
            if (!map.TryGetValue(pointer.Id, out id)) return;
            endPointer(id);
        }

        private void pointerCancelledhandler(object sender, MetaGestureEventArgs metaGestureEventArgs)
        {
            int id;
            var pointer = metaGestureEventArgs.Pointer;
            if (pointer.InputSource == this) return;
            if (!map.TryGetValue(pointer.Id, out id)) return;
            cancelPointer(id);
        }

    }

}