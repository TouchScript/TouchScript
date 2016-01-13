/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace TouchScript.Behaviors.Visualizer
{
    /// <summary>
    /// Visual cursor implementation used by TouchScript.
    /// </summary>
    [HelpURL("http://touchscript.github.io/docs/html/T_TouchScript_Behaviors_TouchProxy.htm")]
    public class TouchProxy : TouchProxyBase
    {
        /// <summary>
        /// The link to UI.Text component.
        /// </summary>
        public Text Text;

        private StringBuilder stringBuilder = new StringBuilder(64);

        #region Protected methods

        /// <inheritdoc />
        protected override void updateOnce(TouchPoint touch)
        {
            base.updateOnce(touch);

            stringBuilder.Length = 0;
            stringBuilder.Append("Touch id: ");
            stringBuilder.Append(touch.Id);
            gameObject.name = stringBuilder.ToString();

            if (Text == null) return;
            if (!ShowTouchId && !ShowTags) return;

            stringBuilder.Length = 0;
            if (ShowTouchId)
            {
                stringBuilder.Append("Id: ");
                stringBuilder.Append(touch.Id);
            }
            if (ShowTags)
            {
                if (stringBuilder.Length > 0) stringBuilder.Append("\n");
                stringBuilder.Append("Tags: ");
                stringBuilder.Append(touch.Tags.ToString());
            }
            Text.text = stringBuilder.ToString();
        }

        #endregion
    }

    /// <summary>
    /// Base class for <see cref="TouchVisualizer"/> cursors.
    /// </summary>
    public class TouchProxyBase : MonoBehaviour
    {
        #region Public properties

        /// <summary>
        /// Gets or sets cursor size.
        /// </summary>
        /// <value> Cursor size in pixels. </value>
        public int Size
        {
            get { return size; }
            set
            {
                size = value;
                rect.sizeDelta = Vector2.one * size;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether touch id text should be displayed on screen.
        /// </summary>
        /// <value> <c>true</c> if touch id text should be displayed on screen; otherwise, <c>false</c>. </value>
        public bool ShowTouchId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether touch tags text should be displayed on screen.
        /// </summary>
        /// <value> <c>true</c> if touch tags text should be displayed on screen; otherwise, <c>false</c>. </value>
        public bool ShowTags { get; set; }

        #endregion

        #region Private variables

        /// <summary>
        /// Cached RectTransform.
        /// </summary>
        protected RectTransform rect;

        /// <summary>
        /// Cursor size.
        /// </summary>
        protected int size = 1;

        #endregion

        #region Public methods

        /// <summary>
        /// Initializes (resets) the cursor.
        /// </summary>
        /// <param name="parent"> Parent container. </param>
        /// <param name="touch"> Touch this cursor represents. </param>
        public void Init(RectTransform parent, TouchPoint touch)
        {
            show();
            rect.SetParent(parent);
            rect.SetAsLastSibling();
            updateOnce(touch);
            update(touch);
        }

        /// <summary>
        /// Updates the touch. This method is called when the touch is moved.
        /// </summary>
        /// <param name="touch"> Touch this cursor represents. </param>
        public void UpdateTouch(TouchPoint touch)
        {
            update(touch);
        }

        /// <summary>
        /// Hides this instance.
        /// </summary>
        public void Hide()
        {
            hide();
        }

        #endregion

        #region Unity methods

        private void Awake()
        {
            rect = transform as RectTransform;
            if (rect == null)
            {
                Debug.LogError("TouchProxy must be on an UI element!");
                enabled = false;
                return;
            }
            rect.anchorMin = rect.anchorMax = Vector2.zero;
        }

        #endregion

        #region Protected methods

        /// <summary>
        /// Hides (clears) this instance.
        /// </summary>
        protected virtual void hide()
        {
            gameObject.SetActive(false);
            gameObject.name = "inactive touch";
        }

        /// <summary>
        /// Shows this instance.
        /// </summary>
        protected virtual void show()
        {
            gameObject.SetActive(true);
        }

        /// <summary>
        /// This method is called once when the cursor is initialized.
        /// </summary>
        /// <param name="touch"> The touch. </param>
        protected virtual void updateOnce(TouchPoint touch) {}

        /// <summary>
        /// This method is called every time when the touch changes.
        /// </summary>
        /// <param name="touch"> The touch. </param>
        public virtual void update(TouchPoint touch)
        {
            rect.anchoredPosition = touch.Position;
        }

        #endregion
    }
}