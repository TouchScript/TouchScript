/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System.Collections.Generic;
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
        public Texture2D TouchTexture
        {
            get { return texture; }
            set
            {
                texture = value;
                update();
            }
        }

        /// <summary>
        /// Font color for touch ids.
        /// </summary>
        public Color FontColor
        {
            get { return fontColor; }
            set { fontColor = value; }
        }

        public bool UseDPI
        {
            get { return useDPI; }
            set
            {
                useDPI = value;
                update();
            }
        }

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
        private Texture2D texture;
        [SerializeField]
        private Color fontColor = new Color(0, 1, 1, 1);
        [SerializeField]
        private bool useDPI = true;
        [SerializeField]
        private float touchSize = 1f;

        private Dictionary<int, ITouchPoint> dummies = new Dictionary<int, ITouchPoint>();
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

        private void OnGUI()
        {
            if (TouchTexture == null) return;
            if (style == null) style = new GUIStyle(GUI.skin.label);
            checkDPI();

            style.fontSize = fontSize;

            foreach (KeyValuePair<int, ITouchPoint> dummy in dummies)
            {
                var x = dummy.Value.Position.x;
                var y = Screen.height - dummy.Value.Position.y;
                GUI.DrawTexture(new Rect(x - halfWidth, y - halfHeight, width, height), TouchTexture, ScaleMode.ScaleToFit);

                var id = dummy.Value.Id.ToString();
                GUI.color = Color.black;
                GUI.Label(new Rect(x + xOffset + shadowOffset, y + yOffset + shadowOffset, labelWidth, labelHeight), id, style);
                GUI.color = fontColor;
                GUI.Label(new Rect(x + xOffset, y + yOffset, labelWidth, labelHeight), id, style);
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
            if (!useDPI)
            {
                width = 32;
                height = 32;
                scale = 1/4f;
                computeConsts();
            } else
            {
                dpi = TouchManager.Instance.DPI;
                textureDPI = texture.width * TouchManager.INCH_TO_CM / touchSize;
                scale = dpi / textureDPI;
                width = (int)(texture.width * scale);
                height = (int)(texture.height * scale);
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

        private void updateDummy(ITouchPoint dummy)
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
                ITouchPoint dummy;
                if (!dummies.TryGetValue(touchPoint.Id, out dummy)) return;
                updateDummy(touchPoint);
            }
        }

        private void touchesEndedHandler(object sender, TouchEventArgs e)
        {
            foreach (var touchPoint in e.TouchPoints)
            {
                ITouchPoint dummy;
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