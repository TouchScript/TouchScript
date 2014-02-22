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

        public bool UseDPI = true;

        #endregion

        #region Private variables

        private Dictionary<int, TouchPoint> dummies = new Dictionary<int, TouchPoint>();
        private float textureDPI, scale, dpi, shadowOffset;
        private int width, height, halfWidth, halfHeight, xOffset, yOffset, labelWidth, labelHeight, fontSize;
        private GUIStyle style;

        #endregion

        #region Unity methods

        private void OnEnable()
        {
            if (TouchTexture == null)
            {
                Debug.LogError("Touch Debugger doesn't have touch texture assigned!");
                return;
            }

            updateDPI();

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
            if (style == null) style = new GUIStyle(GUI.skin.label);
            updateDPI();

            style.fontSize = fontSize;

            foreach (KeyValuePair<int, TouchPoint> dummy in dummies)
            {
                var x = dummy.Value.Position.x;
                var y = Screen.height - dummy.Value.Position.y;
                GUI.DrawTexture(new Rect(x - halfWidth, y - halfHeight, width, height), TouchTexture, ScaleMode.ScaleToFit);

                var id = dummy.Value.Id.ToString();
                GUI.color = Color.black;
                GUI.Label(new Rect(x + xOffset + shadowOffset, y + yOffset + shadowOffset, labelWidth, labelHeight), id, style);
                GUI.color = FontColor;
                GUI.Label(new Rect(x + xOffset, y + yOffset, labelWidth, labelHeight), id, style);
            }
        }

        #endregion

        #region Private functions

        private void updateDPI()
        {
            if (!UseDPI)
            {
                if (width != 0) return;

                width = 32;
                height = 32;
                scale = 1/4f;
                computeConsts();
            } else
            {
                if (Mathf.Approximately(dpi, TouchManager.Instance.DPI)) return;

                textureDPI = TouchTexture.width * TouchManager.INCH_TO_CM / 1.5f;
                scale = TouchManager.Instance.DPI / textureDPI;
                width = (int)(TouchTexture.width * scale);
                height = (int)(TouchTexture.height * scale);
                computeConsts();
            }
        }

        private void computeConsts()
        {
            halfWidth = width / 2;
            halfHeight = height / 2;
            xOffset = (int)(width*.3f);
            yOffset = (int)(height*.3f);
            fontSize = (int)(32 * scale);
            shadowOffset = 2*scale;
            labelWidth = 10*fontSize;
            labelHeight = 2*fontSize;
        }

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