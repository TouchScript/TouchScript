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

        /// <summary>Gets or sets the texture to use.</summary>
        public Texture2D TouchTexture
        {
            get { return texture; }
            set
            {
                texture = value;
                update();
            }
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

        /// <summary>Gets or sets font color for touch ids.</summary>
        public Color FontColor
        {
            get { return fontColor; }
            set { fontColor = value; }
        }

        #endregion

        #region Private variables

        [SerializeField]
        private bool showTouchId = true;

        [SerializeField]
        private bool showTags = false;

        [SerializeField]
        private Texture2D texture;

        [SerializeField]
        private bool useDPI = true;

        [SerializeField]
        private float touchSize = 1f;

        [SerializeField]
        private Color fontColor = new Color(0, 1, 1, 1);

        private Dictionary<int, ITouch> dummies = new Dictionary<int, ITouch>(10);
        private Dictionary<int, string> tags = new Dictionary<int, string>(10);
        private float textureDPI, scale, dpi, shadowOffset;
        private int textureWidth, textureHeight, halfTextureWidth, halfTextureHeight, xOffset, yOffset, labelWidth, labelHeight, fontSize;
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

            foreach (KeyValuePair<int, ITouch> dummy in dummies)
            {
                var x = dummy.Value.Position.x;
                var y = Screen.height - dummy.Value.Position.y;
                GUI.DrawTexture(new Rect(x - halfTextureWidth, y - halfTextureHeight, textureWidth, textureHeight), TouchTexture, ScaleMode.ScaleToFit);

                string text;
                int id = dummy.Value.Id;
                int line = 0;
                if (ShowTouchId)
                {
                    text = "id: " + id;
                    GUI.color = Color.black;
                    GUI.Label(new Rect(x + xOffset + shadowOffset, y + yOffset + shadowOffset, labelWidth, labelHeight), text, style);
                    GUI.color = fontColor;
                    GUI.Label(new Rect(x + xOffset, y + yOffset, labelWidth, labelHeight), text, style);
                    line++;
                }

                if (ShowTags && tags.ContainsKey(id))
                {
                    text = "tags: " + tags[id];
                    GUI.color = Color.black;
                    GUI.Label(new Rect(x + xOffset + shadowOffset, y + yOffset + fontSize * line + shadowOffset, labelWidth, labelHeight), text, style);
                    GUI.color = fontColor;
                    GUI.Label(new Rect(x + xOffset, y + yOffset + fontSize * line, labelWidth, labelHeight), text, style);
                }
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
            if (useDPI)
            {
                dpi = TouchManager.Instance.DPI;
                textureDPI = texture.width * TouchManager.INCH_TO_CM / touchSize;
                scale = dpi / textureDPI;
                textureWidth = (int)(texture.width * scale);
                textureHeight = (int)(texture.height * scale);
                computeConsts();
            }
            else
            {
                textureWidth = 32;
                textureHeight = 32;
                scale = 1 / 4f;
                computeConsts();
            }
        }

        private void computeConsts()
        {
            halfTextureWidth = textureWidth / 2;
            halfTextureHeight = textureHeight / 2;
            xOffset = (int)(textureWidth * .35f);
            yOffset = (int)(textureHeight * .35f);
            fontSize = (int)(32 * scale);
            shadowOffset = 2 * scale;
            labelWidth = 20 * fontSize;
            labelHeight = 2 * fontSize;
        }

        private void updateDummy(ITouch dummy)
        {
            dummies[dummy.Id] = dummy;
        }

        #endregion

        #region Event handlers

        private void touchesBeganHandler(object sender, TouchEventArgs e)
        {
            foreach (var touch in e.Touches)
            {
                dummies.Add(touch.Id, touch);
                if (touch.Tags.Count > 0)
                {
                    tags.Add(touch.Id, touch.Tags.ToString());
                }
            }
        }

        private void touchesMovedHandler(object sender, TouchEventArgs e)
        {
            foreach (var touch in e.Touches)
            {
                ITouch dummy;
                if (!dummies.TryGetValue(touch.Id, out dummy)) return;
                updateDummy(touch);
            }
        }

        private void touchesEndedHandler(object sender, TouchEventArgs e)
        {
            foreach (var touch in e.Touches)
            {
                ITouch dummy;
                if (!dummies.TryGetValue(touch.Id, out dummy)) return;
                dummies.Remove(touch.Id);
            }
        }

        private void touchesCancelledHandler(object sender, TouchEventArgs e)
        {
            touchesEndedHandler(sender, e);
        }

        #endregion
    }
}