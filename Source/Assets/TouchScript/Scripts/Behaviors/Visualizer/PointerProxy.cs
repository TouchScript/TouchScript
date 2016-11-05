/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System.Text;
using TouchScript.Pointers;
using UnityEngine;
using UnityEngine.UI;

namespace TouchScript.Behaviors.Visualizer
{
    /// <summary>
    /// Visual cursor implementation used by TouchScript.
    /// </summary>
    [HelpURL("http://touchscript.github.io/docs/html/T_TouchScript_Behaviors_Visualizer_TouchProxy.htm")]
    public class PointerProxy : PointerProxyBase
    {
        /// <summary>
        /// The link to UI.Text component.
        /// </summary>
        public Text Text;

        private StringBuilder stringBuilder = new StringBuilder(64);

        #region Protected methods

        /// <inheritdoc />
        protected override void updateOnce(Pointer pointer)
        {
            base.updateOnce(pointer);

            stringBuilder.Length = 0;
            stringBuilder.Append("Pointer id: ");
            stringBuilder.Append(pointer.Id);
            gameObject.name = stringBuilder.ToString();

            if (Text == null) return;
			if (!ShowPointerId && !ShowFlags)
            {
                Text.text = "";
                return;
            }

            stringBuilder.Length = 0;
            if (ShowPointerId)
            {
                stringBuilder.Append("Id: ");
                stringBuilder.Append(pointer.Id);
            }
            if (ShowFlags)
            {
                if (stringBuilder.Length > 0) stringBuilder.Append("\n");
                stringBuilder.Append("Flags: ");
                stringBuilder.Append(pointer.Flags);
            }

            Text.text = stringBuilder.ToString();
        }

        #endregion
    }

    /// <summary>
    /// Base class for <see cref="PointerVisualizer"/> cursors.
    /// </summary>
    public class PointerProxyBase : MonoBehaviour
    {
        #region Public properties

        /// <summary>
        /// Gets or sets cursor size.
        /// </summary>
        /// <value> Cursor size in pixels. </value>
        public uint Size
        {
            get { return size; }
            set
            {
                size = value;
                rect.sizeDelta = Vector2.one * size;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether pointer id text should be displayed on screen.
        /// </summary>
        /// <value> <c>true</c> if pointer id text should be displayed on screen; otherwise, <c>false</c>. </value>
        public bool ShowPointerId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether pointer flags text should be displayed on screen.
        /// </summary>
        /// <value> <c>true</c> if pointer flags text should be displayed on screen; otherwise, <c>false</c>. </value>
        public bool ShowFlags { get; set; }

        #endregion

        #region Private variables

        /// <summary>
        /// Cached RectTransform.
        /// </summary>
        protected RectTransform rect;

        /// <summary>
        /// Cursor size.
        /// </summary>
        protected uint size = 1;

        protected uint hash = uint.MaxValue;

        #endregion

        #region Public methods

        /// <summary>
        /// Initializes (resets) the cursor.
        /// </summary>
        /// <param name="parent"> Parent container. </param>
        /// <param name="pointer"> Pointer this cursor represents. </param>
        public void Init(RectTransform parent, Pointer pointer)
        {
            hash = uint.MaxValue;

            show();
            rect.SetParent(parent);
            rect.SetAsLastSibling();
            update(pointer);
        }

        /// <summary>
        /// Updates the pointer. This method is called when the pointer is moved.
        /// </summary>
        /// <param name="pointer"> Pointer this cursor represents. </param>
        public void UpdatePointer(Pointer pointer)
        {
            update(pointer);
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
                Debug.LogError("PointerProxy must be on an UI element!");
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
            gameObject.name = "inactive pointer";
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
        /// <param name="pointer"> The pointer. </param>
        protected virtual void updateOnce(Pointer pointer) {}

        /// <summary>
        /// This method is called every time when the pointer changes.
        /// </summary>
        /// <param name="pointer"> The pointer. </param>
        public virtual void update(Pointer pointer)
        {
            rect.anchoredPosition = pointer.Position;
            var newHash = getPointerHash(pointer);
            if (newHash != hash) updateOnce(pointer);
            hash = newHash;
        }

        #endregion

        #region Private functions

        private uint getPointerHash(Pointer pointer)
        {
            return pointer.Flags;
        }

        #endregion
    }
}