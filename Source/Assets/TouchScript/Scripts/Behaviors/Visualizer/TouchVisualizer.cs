/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System.Collections.Generic;
using TouchScript.Utils;
using UnityEngine;

namespace TouchScript.Behaviors.Visualizer
{
    /// <summary>
    /// <para>Touch visualizer which shows touch circles with debug text using Unity UI.</para>
    /// <para>The script should be placed on an element with RectTransform or a Canvas. A reference prefab is provided in TouchScript package.</para>
    /// </summary>
    [HelpURL("http://touchscript.github.io/docs/Index.html?topic=html/T_TouchScript_Behaviors_TouchVisualizer.htm")]
    public class TouchVisualizer : MonoBehaviour
    {
        #region Public properties

        /// <summary>
        /// Gets or sets touch UI element prefab which represents a touch on screen.
        /// </summary>
        /// <value> A prefab with a script derived from TouchProxyBase. </value>
        public TouchProxyBase TouchProxy
        {
            get { return touchProxy; }
            set
            {
                touchProxy = value;
                updateDefaultSize();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether touch id text should be displayed on screen.
        /// </summary>
        /// <value> <c>true</c> if touch id text should be displayed on screen; otherwise, <c>false</c>. </value>
        public bool ShowTouchId
        {
            get { return showTouchId; }
            set { showTouchId = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether touch tags text should be displayed on screen.
        /// </summary>
        /// <value> <c>true</c> if touch tags text should be displayed on screen; otherwise, <c>false</c>. </value>
        public bool ShowTags
        {
            get { return showTags; }
            set { showTags = value; }
        }

        /// <summary>
        /// Gets or sets whether <see cref="TouchVisualizer"/> is using DPI to scale touch cursors.
        /// </summary>
        /// <value> <c>true</c> if DPI value is used; otherwise, <c>false</c>. </value>
        public bool UseDPI
        {
            get { return useDPI; }
            set { useDPI = value; }
        }

        /// <summary>
        /// Gets or sets the size of touch cursors in cm. This value is only used when <see cref="UseDPI"/> is set to <c>true</c>.
        /// </summary>
        /// <value> The size of touch cursors in cm. </value>
        public float TouchSize
        {
            get { return touchSize; }
            set { touchSize = value; }
        }

        #endregion

        #region Private variables

        [SerializeField]
        private TouchProxyBase touchProxy;

        [SerializeField]
        private bool showTouchId = true;

        [SerializeField]
        private bool showTags = false;

        [SerializeField]
        private bool useDPI = true;

        [SerializeField]
        private float touchSize = 1f;

        private int defaultSize = 64;
        private RectTransform rect;
        private ObjectPool<TouchProxyBase> pool;
        private Dictionary<int, TouchProxyBase> proxies = new Dictionary<int, TouchProxyBase>(10);

        #endregion

        #region Unity methods

        private void Awake()
        {
            pool = new ObjectPool<TouchProxyBase>(10, instantiateProxy, null, clearProxy);
            rect = transform as RectTransform;
            if (rect == null)
            {
                Debug.LogError("TouchVisualizer must be on an UI element!");
                enabled = false;
            }
            updateDefaultSize();
        }

        private void OnEnable()
        {
            if (TouchManager.Instance != null)
            {
                TouchManager.Instance.TouchBegan += touchBeganHandler;
                TouchManager.Instance.TouchEnded += touchEndedHandler;
                TouchManager.Instance.TouchMoved += touchMovedHandler;
                TouchManager.Instance.TouchCancelled += touchCancelledHandler;
            }
        }

        private void OnDisable()
        {
            if (TouchManager.Instance != null)
            {
                TouchManager.Instance.TouchBegan -= touchBeganHandler;
                TouchManager.Instance.TouchEnded -= touchEndedHandler;
                TouchManager.Instance.TouchMoved -= touchMovedHandler;
                TouchManager.Instance.TouchCancelled -= touchCancelledHandler;
            }
        }

        #endregion

        #region Private functions

        private TouchProxyBase instantiateProxy()
        {
            return Instantiate(touchProxy);
        }

        private void clearProxy(TouchProxyBase proxy)
        {
            proxy.Hide();
        }

        private int getTouchSize()
        {
            if (useDPI) return (int) (touchSize * TouchManager.Instance.DotsPerCentimeter);
            return defaultSize;
        }

        private void updateDefaultSize()
        {
            if (touchProxy != null)
            {
                var rt = touchProxy.GetComponent<RectTransform>();
                if (rt) defaultSize = (int) rt.sizeDelta.x;
            }
        }

        #endregion

        #region Event handlers

        private void touchBeganHandler(object sender, TouchEventArgs e)
        {
            if (touchProxy == null) return;

            var touch = e.Touch;
            var proxy = pool.Get();
            proxy.Size = getTouchSize();
            proxy.ShowTouchId = showTouchId;
            proxy.ShowTags = showTags;
            proxy.Init(rect, touch);
            proxies.Add(touch.Id, proxy);
        }

        private void touchMovedHandler(object sender, TouchEventArgs e)
        {
            var touch = e.Touch;
            TouchProxyBase proxy;
            if (!proxies.TryGetValue(touch.Id, out proxy)) return;
            proxy.UpdateTouch(touch);
        }

        private void touchEndedHandler(object sender, TouchEventArgs e)
        {
            var touch = e.Touch;
            TouchProxyBase proxy;
            if (!proxies.TryGetValue(touch.Id, out proxy)) return;
            proxies.Remove(touch.Id);
            pool.Release(proxy);
        }

        private void touchCancelledHandler(object sender, TouchEventArgs e)
        {
            touchEndedHandler(sender, e);
        }

        #endregion
    }
}