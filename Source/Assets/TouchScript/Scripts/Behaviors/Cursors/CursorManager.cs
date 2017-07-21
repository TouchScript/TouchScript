/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System.Collections.Generic;
using TouchScript.Utils;
using TouchScript.Pointers;
using TouchScript.Utils.Attributes;
using UnityEngine;

namespace TouchScript.Behaviors.Cursors
{
    /// <summary>
    /// <para>Pointer visualizer which shows pointer circles with debug text using Unity UI.</para>
    /// <para>The script should be placed on an element with RectTransform or a Canvas. A reference prefab is provided in TouchScript package.</para>
    /// </summary>
    [HelpURL("http://touchscript.github.io/docs/html/T_TouchScript_Behaviors_Visualizer_TouchVisualizer.htm")]
    public class CursorManager : MonoBehaviour
    {
        #region Public properties

        public PointerCursor MouseCursor
        {
            get { return mouseCursor; }
            set { mouseCursor = value; }
        }

        public PointerCursor TouchCursor
        {
            get { return touchCursor; }
            set { touchCursor = value; }
        }

        public PointerCursor PenCursor
        {
            get { return penCursor; }
            set { penCursor = value; }
        }

        public PointerCursor ObjectCursor
        {
            get { return objectCursor; }
            set { objectCursor = value; }
        }

        /// <summary>
        /// Gets or sets whether <see cref="CursorManager"/> is using DPI to scale pointer cursors.
        /// </summary>
        /// <value> <c>true</c> if DPI value is used; otherwise, <c>false</c>. </value>
        public bool UseDPI
        {
            get { return useDPI; }
            set
            {
                useDPI = value;
                updateCursorSize();
            }
        }

        /// <summary>
        /// Gets or sets the size of pointer cursors in cm. This value is only used when <see cref="UseDPI"/> is set to <c>true</c>.
        /// </summary>
        /// <value> The size of pointer cursors in cm. </value>
        public float CursorSize
        {
            get { return cursorSize; }
            set
            {
                cursorSize = value;
                updateCursorSize();
            }
        }

        public uint CursorPixelSize
        {
            get { return cursorPixelSize; }
            set
            {
                cursorPixelSize = value;
                updateCursorSize();
            }
        }

        #endregion

        #region Private variables

        [SerializeField]
        private bool generalProps; // Used in the custom inspector

        [SerializeField]
        private bool advancedProps; // Used in the custom inspector

        [SerializeField]
        private PointerCursor mouseCursor;

        [SerializeField]
        private PointerCursor touchCursor;

        [SerializeField]
        private PointerCursor penCursor;

        [SerializeField]
        private PointerCursor objectCursor;

        [SerializeField]
        [ToggleLeft]
        private bool useDPI = true;

        [SerializeField]
        private float cursorSize = 1f;

        [SerializeField]
        private uint cursorPixelSize = 64;

        private RectTransform rect;
        private ObjectPool<PointerCursor> mousePool;
        private ObjectPool<PointerCursor> touchPool;
        private ObjectPool<PointerCursor> penPool;
        private ObjectPool<PointerCursor> objectPool;
        private Dictionary<int, PointerCursor> cursors = new Dictionary<int, PointerCursor>(10);

        #endregion

        #region Unity methods

        private void Awake()
        {
            mousePool = new ObjectPool<PointerCursor>(2, instantiateMouseProxy, null, clearProxy);
            touchPool = new ObjectPool<PointerCursor>(10, instantiateTouchProxy, null, clearProxy);
            penPool = new ObjectPool<PointerCursor>(2, instantiatePenProxy, null, clearProxy);
            objectPool = new ObjectPool<PointerCursor>(2, instantiateObjectProxy, null, clearProxy);

            updateCursorSize();

            rect = transform as RectTransform;
            if (rect == null)
            {
                Debug.LogError("CursorManager must be on an UI element!");
                enabled = false;
            }
        }

        private void OnEnable()
        {
            if (TouchManager.Instance != null)
            {
                TouchManager.Instance.PointersAdded += pointersAddedHandler;
                TouchManager.Instance.PointersRemoved += pointersRemovedHandler;
                TouchManager.Instance.PointersPressed += pointersPressedHandler;
                TouchManager.Instance.PointersReleased += pointersReleasedHandler;
                TouchManager.Instance.PointersUpdated += PointersUpdatedHandler;
                TouchManager.Instance.PointersCancelled += pointersCancelledHandler;
            }
        }

        private void OnDisable()
        {
            if (TouchManager.Instance != null)
            {
                TouchManager.Instance.PointersAdded -= pointersAddedHandler;
                TouchManager.Instance.PointersRemoved -= pointersRemovedHandler;
                TouchManager.Instance.PointersPressed -= pointersPressedHandler;
                TouchManager.Instance.PointersReleased -= pointersReleasedHandler;
                TouchManager.Instance.PointersUpdated -= PointersUpdatedHandler;
                TouchManager.Instance.PointersCancelled -= pointersCancelledHandler;
            }
        }

        #endregion

        #region Private functions

        private PointerCursor instantiateMouseProxy()
        {
            return Instantiate(mouseCursor);
        }

        private PointerCursor instantiateTouchProxy()
        {
            return Instantiate(touchCursor);
        }

        private PointerCursor instantiatePenProxy()
        {
            return Instantiate(penCursor);
        }

        private PointerCursor instantiateObjectProxy()
        {
            return Instantiate(objectCursor);
        }

        private void clearProxy(PointerCursor cursor)
        {
            cursor.Hide();
        }

        private void updateCursorSize()
        {
            if (useDPI) cursorPixelSize = (uint)(cursorSize * TouchManager.Instance.DotsPerCentimeter);
        }

        #endregion

        #region Event handlers

        private void pointersAddedHandler(object sender, PointerEventArgs e)
        {
            updateCursorSize();

            var count = e.Pointers.Count;
            for (var i = 0; i < count; i++)
            {
                var pointer = e.Pointers[i];
                // Don't show internal pointers
                if ((pointer.Flags & Pointer.FLAG_INTERNAL) > 0) continue;

                PointerCursor cursor;
                switch (pointer.Type)
                {
                    case Pointer.PointerType.Mouse:
                        cursor = mousePool.Get();
                        break;
                    case Pointer.PointerType.Touch:
                        cursor = touchPool.Get();
                        break;
                    case Pointer.PointerType.Pen:
                        cursor = penPool.Get();
                        break;
                    case Pointer.PointerType.Object:
                        cursor = objectPool.Get();
                        break;
                    default:
                        continue;
                }

                cursor.Size = cursorPixelSize;
                cursor.Init(rect, pointer);
                cursors.Add(pointer.Id, cursor);
            }
        }

        private void pointersRemovedHandler(object sender, PointerEventArgs e)
        {
            var count = e.Pointers.Count;
            for (var i = 0; i < count; i++)
            {
                var pointer = e.Pointers[i];
                PointerCursor cursor;
                if (!cursors.TryGetValue(pointer.Id, out cursor)) continue;
                cursors.Remove(pointer.Id);

                switch (pointer.Type)
                {
                    case Pointer.PointerType.Mouse:
                        mousePool.Release(cursor);
                        break;
                    case Pointer.PointerType.Touch:
                        touchPool.Release(cursor);
                        break;
                    case Pointer.PointerType.Pen:
                        penPool.Release(cursor);
                        break;
                    case Pointer.PointerType.Object:
                        objectPool.Release(cursor);
                        break;
                }
            }
        }

        private void pointersPressedHandler(object sender, PointerEventArgs e)
        {
            var count = e.Pointers.Count;
            for (var i = 0; i < count; i++)
            {
                var pointer = e.Pointers[i];
                PointerCursor cursor;
                if (!cursors.TryGetValue(pointer.Id, out cursor)) continue;
                cursor.SetState(pointer, PointerCursor.ProxyState.Pressed);
            }
        }

        private void PointersUpdatedHandler(object sender, PointerEventArgs e)
        {
            var count = e.Pointers.Count;
            for (var i = 0; i < count; i++)
            {
                var pointer = e.Pointers[i];
                PointerCursor cursor;
                if (!cursors.TryGetValue(pointer.Id, out cursor)) continue;
                cursor.UpdatePointer(pointer);
            }
        }

        private void pointersReleasedHandler(object sender, PointerEventArgs e)
        {
            var count = e.Pointers.Count;
            for (var i = 0; i < count; i++)
            {
                var pointer = e.Pointers[i];
                PointerCursor cursor;
                if (!cursors.TryGetValue(pointer.Id, out cursor)) continue;
                cursor.SetState(pointer, PointerCursor.ProxyState.Released);
            }
        }

        private void pointersCancelledHandler(object sender, PointerEventArgs e)
        {
            pointersRemovedHandler(sender, e);
        }

        #endregion
    }
}