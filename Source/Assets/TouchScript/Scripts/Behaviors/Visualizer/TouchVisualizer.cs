/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System.Collections.Generic;
using TouchScript.Utils;
using UnityEngine;

namespace TouchScript.Behaviors.Visualizer
{
    /// <summary>
    /// Visual debugger to show touches as GUI elements.
    /// </summary>
	public class TouchVisualizer : MonoBehaviour
    {
        #region Public properties

        public TouchProxy TouchProxy
        {
            get { return touchProxy; }
            set { touchProxy = value; }
        }

        /// <summary>
        /// Show touch id near touch circles.
        /// </summary>
        public bool ShowTouchId
        {
            get { return showTouchId; }
            set { showTouchId = value; }
        }

        /// <summary>
        /// Show tag list near touch circles.
        /// </summary>
        public bool ShowTags
        {
            get { return showTags; }
            set { showTags = value; }
        }

        /// <summary>Gets or sets whether <see cref="TouchDebugger"/> is using DPI to scale touch cursors.</summary>
        /// <value><c>true</c> if dpi value is used; otherwise, <c>false</c>.</value>
        public bool UseDPI
        {
            get { return useDPI; }
            set
            {
                useDPI = value;
                update();
            }
        }

        /// <summary>Gets or sets the size of touch cursors in cm.</summary>
        /// <value>The size of touch cursors in cm.</value>
        public float TouchSize
        {
            get { return touchSize; }
            set
            {
                touchSize = value;
                update();
            }
        }

        #endregion

        #region Private variables

        [SerializeField]
        private TouchProxy touchProxy;

        [SerializeField]
        private bool showTouchId = true;

        [SerializeField]
        private bool showTags = false;

        [SerializeField]
        private bool useDPI = true;

        [SerializeField]
        private float touchSize = 1f;

        private RectTransform rect;
        private ObjectPool<TouchProxy> pool; 
        private Dictionary<int, TouchProxy> proxies = new Dictionary<int, TouchProxy>(10);
        private float textureDPI, scale, dpi, shadowOffset;

        #endregion

        #region Unity methods

        private void Awake()
        {
            pool = new ObjectPool<TouchProxy>(10, instantiateProxy, null, clearProxy);
            rect = transform as RectTransform;
            if (rect == null)
            {
                Debug.LogError("TouchVisualizer must be on an UI element!");
            }
        }

        private void OnEnable()
        {
            update();

            if (TouchManager.Instance != null)
            {
                TouchManager.Instance.TouchesBegan += touchesBeganHandler;
                TouchManager.Instance.TouchesEnded += touchesEndedHandler;
                TouchManager.Instance.TouchesMoved += touchesMovedHandler;
                TouchManager.Instance.TouchesCancelled += touchesCancelledHandler;
            }
        }

        private void OnDisable()
        {
            if (TouchManager.Instance != null)
            {
                TouchManager.Instance.TouchesBegan -= touchesBeganHandler;
                TouchManager.Instance.TouchesEnded -= touchesEndedHandler;
                TouchManager.Instance.TouchesMoved -= touchesMovedHandler;
                TouchManager.Instance.TouchesCancelled -= touchesCancelledHandler;
            }
        }

        #endregion

        #region Private functions

        private void checkDPI()
        {
            if (useDPI && !Mathf.Approximately(dpi, TouchManager.Instance.DPI)) update();
        }

        private void update()
        {
        }

        private TouchProxy instantiateProxy()
        {
            return Instantiate(touchProxy);
        }

        private void clearProxy(TouchProxy proxy)
        {
            proxy.Hide();
        }

        #endregion

        #region Event handlers

        private void touchesBeganHandler(object sender, TouchEventArgs e)
        {
            if (touchProxy == null) return;

            var count = e.Touches.Count;
            for (var i = 0; i < count; i++)
            {
                var touch = e.Touches[i];
                var proxy = pool.Get();
                proxy.Init(rect, touch);
                proxies.Add(touch.Id, proxy);
            }
        }

        private void touchesMovedHandler(object sender, TouchEventArgs e)
        {
            var count = e.Touches.Count;
            for (var i = 0; i < count; i++)
            {
                var touch = e.Touches[i];
                TouchProxy proxy;
                if (!proxies.TryGetValue(touch.Id, out proxy)) return;
                proxy.UpdateTouch(touch);
            }
        }

        private void touchesEndedHandler(object sender, TouchEventArgs e)
        {
            var count = e.Touches.Count;
            for (var i = 0; i < count; i++)
            {
                var touch = e.Touches[i];
                TouchProxy proxy;
                if (!proxies.TryGetValue(touch.Id, out proxy)) return;
                proxies.Remove(touch.Id);
                pool.Release(proxy);
            }
        }

        private void touchesCancelledHandler(object sender, TouchEventArgs e)
        {
            touchesEndedHandler(sender, e);
        }

        #endregion
    }
}