/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System.Collections.Generic;
using TouchScript.Events;
using UnityEngine;

namespace TouchScript.Debugging
{
    /// <summary>
    /// Visual debugger to show touches as GUI elements.
    /// </summary>
    [AddComponentMenu("TouchScript/Touch Debugger")]
    public class TouchDebugger : MonoBehaviour
    {
        #region Public properties

        /// <summary>
        /// Texture to use.
        /// </summary>
        public Texture2D TouchTexture;

        /// <summary>
        /// Font color for touch ids.
        /// </summary>
        public Color FontColor;

        #endregion

        #region Private variables

        private Dictionary<int, TouchPoint> dummies = new Dictionary<int, TouchPoint>();

        #endregion

        #region Unity methods

        private void OnEnable()
        {
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

        private void OnGUI()
        {
            if (TouchTexture == null) return;

            GUI.color = FontColor;

            foreach (KeyValuePair<int, TouchPoint> dummy in dummies)
            {
                var x = dummy.Value.Position.x;
                var y = Screen.height - dummy.Value.Position.y;
                GUI.DrawTexture(new Rect(x - TouchTexture.width/2, y - TouchTexture.height/2, TouchTexture.width, TouchTexture.height), TouchTexture, ScaleMode.ScaleToFit);
                GUI.Label(new Rect(x + TouchTexture.width, y - 9, 60, 25), dummy.Value.Id.ToString());
            }
        }

        #endregion

        #region Private functions

        private void updateDummy(TouchPoint dummy)
        {
            dummies[dummy.Id] = dummy;
        }

        #endregion

        #region Event handlers

        private void touchesBeganHandler(object sender, TouchEventArgs e)
        {
            foreach (var touchPoint in e.TouchPoints)
            {
                dummies.Add(touchPoint.Id, touchPoint);
            }
        }

        private void touchesMovedHandler(object sender, TouchEventArgs e)
        {
            foreach (var touchPoint in e.TouchPoints)
            {
                TouchPoint dummy;
                if (!dummies.TryGetValue(touchPoint.Id, out dummy)) return;
                updateDummy(touchPoint);
            }
        }

        private void touchesEndedHandler(object sender, TouchEventArgs e)
        {
            foreach (var touchPoint in e.TouchPoints)
            {
                TouchPoint dummy;
                if (!dummies.TryGetValue(touchPoint.Id, out dummy)) return;
                dummies.Remove(touchPoint.Id);
            }
        }

        private void touchesCancelledHandler(object sender, TouchEventArgs e)
        {
            touchesEndedHandler(sender, e);
        }

        #endregion
    }
}