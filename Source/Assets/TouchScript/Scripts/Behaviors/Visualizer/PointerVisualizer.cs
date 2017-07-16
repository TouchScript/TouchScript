/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System.Collections.Generic;
using TouchScript.Utils;
using TouchScript.Pointers;
using TouchScript.Utils.Attributes;
using UnityEngine;

namespace TouchScript.Behaviors.Visualizer
{
    /// <summary>
    /// <para>Pointer visualizer which shows pointer circles with debug text using Unity UI.</para>
    /// <para>The script should be placed on an element with RectTransform or a Canvas. A reference prefab is provided in TouchScript package.</para>
    /// </summary>
    [HelpURL("http://touchscript.github.io/docs/html/T_TouchScript_Behaviors_Visualizer_TouchVisualizer.htm")]
    public class PointerVisualizer : MonoBehaviour
    {
        #region Public properties

        public PointerProxy MousePointerProxy
        {
            get { return mousePointerProxy; }
            set { mousePointerProxy = value; }
        }

        public PointerProxy TouchPointerProxy
        {
            get { return touchPointerProxy; }
            set { touchPointerProxy = value; }
        }

        public PointerProxy PenPointerProxy
        {
            get { return penPointerProxy; }
            set { penPointerProxy = value; }
        }

        public PointerProxy ObjectPointerProxy
        {
            get { return objectPointerProxy; }
            set { objectPointerProxy = value; }
        }

        /// <summary>
        /// Gets or sets whether <see cref="PointerVisualizer"/> is using DPI to scale pointer cursors.
        /// </summary>
        /// <value> <c>true</c> if DPI value is used; otherwise, <c>false</c>. </value>
        public bool UseDPI
        {
            get { return useDPI; }
            set { useDPI = value; }
        }

        /// <summary>
        /// Gets or sets the size of pointer cursors in cm. This value is only used when <see cref="UseDPI"/> is set to <c>true</c>.
        /// </summary>
        /// <value> The size of pointer cursors in cm. </value>
        public float PointerSize
        {
            get { return pointerSize; }
            set { pointerSize = value; }
        }

        #endregion

        #region Private variables

        [SerializeField]
        private bool generalProps; // Used in the custom inspector

        [SerializeField]
        private bool advancedProps; // Used in the custom inspector

        [SerializeField]
        private PointerProxy mousePointerProxy;

        [SerializeField]
        private PointerProxy touchPointerProxy;

        [SerializeField]
        private PointerProxy penPointerProxy;

        [SerializeField]
        private PointerProxy objectPointerProxy;

        [SerializeField]
        [ToggleLeft]
        private bool useDPI = true;

        [SerializeField]
        private float pointerSize = 1f;

        private RectTransform rect;
        private ObjectPool<PointerProxy> mousePool;
        private ObjectPool<PointerProxy> touchPool;
        private ObjectPool<PointerProxy> penPool;
        private ObjectPool<PointerProxy> objectPool;
        private Dictionary<int, PointerProxy> proxies = new Dictionary<int, PointerProxy>(10);

        #endregion

        #region Unity methods

        private void Awake()
        {
            mousePool = new ObjectPool<PointerProxy>(2, instantiateMouseProxy, null, clearProxy);
            touchPool = new ObjectPool<PointerProxy>(10, instantiateTouchProxy, null, clearProxy);
            penPool = new ObjectPool<PointerProxy>(2, instantiatePenProxy, null, clearProxy);
            objectPool = new ObjectPool<PointerProxy>(2, instantiateObjectProxy, null, clearProxy);

            rect = transform as RectTransform;
            if (rect == null)
            {
                Debug.LogError("PointerVisualizer must be on an UI element!");
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

        private PointerProxy instantiateMouseProxy()
        {
            return Instantiate(mousePointerProxy);
        }

        private PointerProxy instantiateTouchProxy()
        {
            return Instantiate(touchPointerProxy);
        }

        private PointerProxy instantiatePenProxy()
        {
            return Instantiate(penPointerProxy);
        }

        private PointerProxy instantiateObjectProxy()
        {
            return Instantiate(objectPointerProxy);
        }

        private void clearProxy(PointerProxy proxy)
        {
            proxy.Hide();
        }

        private uint getPointerSize()
        {
            if (useDPI) return (uint) (pointerSize * TouchManager.Instance.DotsPerCentimeter);
            return 0;
        }

        #endregion

        #region Event handlers

        private void pointersAddedHandler(object sender, PointerEventArgs e)
        {
            var count = e.Pointers.Count;
            for (var i = 0; i < count; i++)
            {
                var pointer = e.Pointers[i];
                PointerProxy proxy;
                switch (pointer.Type)
                {
                    case Pointer.PointerType.Mouse:
                        proxy = mousePool.Get();
                        break;
                    case Pointer.PointerType.Touch:
                        proxy = touchPool.Get();
                        break;
                    case Pointer.PointerType.Pen:
                        proxy = penPool.Get();
                        break;
                    case Pointer.PointerType.Object:
                        proxy = objectPool.Get();
                        break;
                    default:
                        continue;
                }

                proxy.Size = getPointerSize();
                proxy.Init(rect, pointer);
                proxies.Add(pointer.Id, proxy);
            }
        }

        private void pointersRemovedHandler(object sender, PointerEventArgs e)
        {
            var count = e.Pointers.Count;
            for (var i = 0; i < count; i++)
            {
                var pointer = e.Pointers[i];
                PointerProxy proxy;
                if (!proxies.TryGetValue(pointer.Id, out proxy)) continue;
                proxies.Remove(pointer.Id);

                switch (pointer.Type)
                {
                    case Pointer.PointerType.Mouse:
                        mousePool.Release(proxy);
                        break;
                    case Pointer.PointerType.Touch:
                        touchPool.Release(proxy);
                        break;
                    case Pointer.PointerType.Pen:
                        penPool.Release(proxy);
                        break;
                    case Pointer.PointerType.Object:
                        objectPool.Release(proxy);
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
                PointerProxy proxy;
                if (!proxies.TryGetValue(pointer.Id, out proxy)) continue;
                proxy.SetState(pointer, PointerProxy.ProxyState.Pressed);
            }
        }

        private void PointersUpdatedHandler(object sender, PointerEventArgs e)
        {
            var count = e.Pointers.Count;
            for (var i = 0; i < count; i++)
            {
                var pointer = e.Pointers[i];
                PointerProxy proxy;
                if (!proxies.TryGetValue(pointer.Id, out proxy)) continue;
                proxy.UpdatePointer(pointer);
            }
        }

        private void pointersReleasedHandler(object sender, PointerEventArgs e)
        {
            var count = e.Pointers.Count;
            for (var i = 0; i < count; i++)
            {
                var pointer = e.Pointers[i];
                PointerProxy proxy;
                if (!proxies.TryGetValue(pointer.Id, out proxy)) continue;
                proxy.SetState(pointer, PointerProxy.ProxyState.Released);
            }
        }

        private void pointersCancelledHandler(object sender, PointerEventArgs e)
        {
            pointersRemovedHandler(sender, e);
        }

        #endregion
    }
}