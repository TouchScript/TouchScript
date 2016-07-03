/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System.Collections.Generic;
using TouchScript.Utils;
using UnityEngine;

namespace TouchScript.Behaviors.Visualizer
{
    /// <summary>
    /// <para>Pointer visualizer which shows pointer circles with debug text using Unity UI.</para>
    /// <para>The script should be placed on an element with RectTransform or a Canvas. A reference prefab is provided in TouchScript package.</para>
    /// </summary>
    [HelpURL("http://touchscript.github.io/docs/html/T_TouchScript_Behaviors_TouchVisualizer.htm")]
    public class PointerVisualizer : MonoBehaviour
    {
        #region Public properties

        /// <summary>
        /// Gets or sets pointer UI element prefab which represents a pointer on screen.
        /// </summary>
        /// <value> A prefab with a script derived from PointerProxyBase. </value>
        public PointerProxyBase PointerProxy
        {
            get { return pointerProxy; }
            set
            {
                pointerProxy = value;
                updateDefaultSize();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether pointer id text should be displayed on screen.
        /// </summary>
        /// <value> <c>true</c> if pointer id text should be displayed on screen; otherwise, <c>false</c>. </value>
        public bool ShowPointerId
        {
            get { return showPointerId; }
            set { showPointerId = value; }
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
        private PointerProxyBase pointerProxy;

        [SerializeField]
        private bool showPointerId = true;

        [SerializeField]
        private bool useDPI = true;

        [SerializeField]
        private float pointerSize = 1f;

        private int defaultSize = 64;
        private RectTransform rect;
        private ObjectPool<PointerProxyBase> pool;
        private Dictionary<int, PointerProxyBase> proxies = new Dictionary<int, PointerProxyBase>(10);

        #endregion

        #region Unity methods

        private void Awake()
        {
            pool = new ObjectPool<PointerProxyBase>(10, instantiateProxy, null, clearProxy);
            rect = transform as RectTransform;
            if (rect == null)
            {
                Debug.LogError("PointerVisualizer must be on an UI element!");
                enabled = false;
            }
            updateDefaultSize();
        }

        private void OnEnable()
        {
            if (TouchManager.Instance != null)
            {
                TouchManager.Instance.PointersBegan += pointersBeganHandler;
                TouchManager.Instance.PointersEnded += pointersEndedHandler;
                TouchManager.Instance.PointersMoved += pointersMovedHandler;
                TouchManager.Instance.PointersCancelled += pointersCancelledHandler;
            }
        }

        private void OnDisable()
        {
            if (TouchManager.Instance != null)
            {
                TouchManager.Instance.PointersBegan -= pointersBeganHandler;
                TouchManager.Instance.PointersEnded -= pointersEndedHandler;
                TouchManager.Instance.PointersMoved -= pointersMovedHandler;
                TouchManager.Instance.PointersCancelled -= pointersCancelledHandler;
            }
        }

        #endregion

        #region Private functions

        private PointerProxyBase instantiateProxy()
        {
            return Instantiate(pointerProxy);
        }

        private void clearProxy(PointerProxyBase proxy)
        {
            proxy.Hide();
        }

        private int getPointerSize()
        {
            if (useDPI) return (int) (pointerSize * TouchManager.Instance.DotsPerCentimeter);
            return defaultSize;
        }

        private void updateDefaultSize()
        {
            if (pointerProxy != null)
            {
                var rt = pointerProxy.GetComponent<RectTransform>();
                if (rt) defaultSize = (int) rt.sizeDelta.x;
            }
        }

        #endregion

        #region Event handlers

        private void pointersBeganHandler(object sender, PointerEventArgs e)
        {
            if (pointerProxy == null) return;

            var count = e.Pointers.Count;
            for (var i = 0; i < count; i++)
            {
                var pointer = e.Pointers[i];
                var proxy = pool.Get();
                proxy.Size = getPointerSize();
                proxy.ShowPointerId = showPointerId;
                proxy.Init(rect, pointer);
                proxies.Add(pointer.Id, proxy);
            }
        }

        private void pointersMovedHandler(object sender, PointerEventArgs e)
        {
            var count = e.Pointers.Count;
            for (var i = 0; i < count; i++)
            {
                var pointer = e.Pointers[i];
                PointerProxyBase proxy;
                if (!proxies.TryGetValue(pointer.Id, out proxy)) return;
                proxy.UpdatePointer(pointer);
            }
        }

        private void pointersEndedHandler(object sender, PointerEventArgs e)
        {
            var count = e.Pointers.Count;
            for (var i = 0; i < count; i++)
            {
                var pointer = e.Pointers[i];
                PointerProxyBase proxy;
                if (!proxies.TryGetValue(pointer.Id, out proxy)) return;
                proxies.Remove(pointer.Id);
                pool.Release(proxy);
            }
        }

        private void pointersCancelledHandler(object sender, PointerEventArgs e)
        {
            pointersEndedHandler(sender, e);
        }

        #endregion
    }
}