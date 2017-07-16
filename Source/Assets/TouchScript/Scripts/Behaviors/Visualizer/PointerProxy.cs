/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System.Text;
using TouchScript.Pointers;
using TouchScript.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace TouchScript.Behaviors.Visualizer
{
    /// <summary>
    /// Visual cursor implementation used by TouchScript.
    /// </summary>
    [HelpURL("http://touchscript.github.io/docs/html/T_TouchScript_Behaviors_Visualizer_TouchProxy.htm")]
    public abstract class TextPointerProxy<T> : PointerProxy where T : IPointer
    {
        #region Public properties

        /// <summary>
        /// Gets or sets a value indicating whether pointer id text should be displayed on screen.
        /// </summary>
        /// <value> <c>true</c> if pointer id text should be displayed on screen; otherwise, <c>false</c>. </value>
        public bool ShowPointerId = true;

        /// <summary>
        /// Gets or sets a value indicating whether pointer flags text should be displayed on screen.
        /// </summary>
        /// <value> <c>true</c> if pointer flags text should be displayed on screen; otherwise, <c>false</c>. </value>
        public bool ShowFlags = false;

        /// <summary>
        /// The link to UI.Text component.
        /// </summary>
        public Text Text;

        #endregion

        #region Private variables

        private static StringBuilder stringBuilder = new StringBuilder(64);

        #endregion

        #region Public methods

        #endregion

        #region Protected methods

        /// <inheritdoc />
        protected override void updateOnce(IPointer pointer)
        {
            base.updateOnce(pointer);

            stringBuilder.Length = 0;
            stringBuilder.Append("Pointer id: ");
            stringBuilder.Append(pointer.Id);
            gameObject.name = stringBuilder.ToString();

            if (Text == null) return;
            if (!shouldShowText())
            {
                Text.text = "";
                return;
            }

            stringBuilder.Length = 0;
            generateText((T) pointer, stringBuilder);

            Text.text = stringBuilder.ToString();
        }

        protected virtual void generateText(T pointer, StringBuilder str)
        {
            if (ShowPointerId)
            {
                str.Append("Id: ");
                str.Append(pointer.Id);
            }
            if (ShowFlags)
            {
                if (str.Length > 0) str.Append("\n");
                str.Append("Flags: ");
                BinaryUtils.ToBinaryString(pointer.Flags, str, 8);
            }
        }

        protected virtual bool shouldShowText()
        {
            return ShowPointerId || ShowFlags;
        }

        protected virtual uint gethash(T pointer)
        {
            var hash = (uint) state;
            if (ShowFlags) hash += pointer.Flags << 3;
            return hash;
        }

        protected sealed override uint getPointerHash(IPointer pointer)
        {
            return gethash((T) pointer);
        }

        #endregion
    }

    /// <summary>
    /// Base class for <see cref="PointerVisualizer"/> cursors.
    /// </summary>
    public class PointerProxy : MonoBehaviour
    {
        #region Consts

        public enum ProxyState
        {
            Released,
            Pressed,
            Over,
            OverPressed
        }

        #endregion

        #region Public properties

        /// <summary>
        /// Gets or sets cursor size.
        /// </summary>
        /// <value> Cursor size in pixels. </value>
        public float Size
        {
            get { return size; }
            set
            {
                size = value;
                if (size > 0)
                {
                    rect.sizeDelta = Vector2.one * size;
                }
                else
                {
                    size = 0;
                    rect.sizeDelta = Vector2.one * defaultSize;
                }
            }
        }

        #endregion

        #region Private variables

        protected ProxyState state;
        protected object stateData;

        /// <summary>
        /// Cached RectTransform.
        /// </summary>
        protected RectTransform rect;

        /// <summary>
        /// Cursor size.
        /// </summary>
        protected float size = 0;

        protected float defaultSize;

        protected uint hash = uint.MaxValue;

        #endregion

        #region Public methods

        /// <summary>
        /// Initializes (resets) the cursor.
        /// </summary>
        /// <param name="parent"> Parent container. </param>
        /// <param name="pointer"> Pointer this cursor represents. </param>
        public void Init(RectTransform parent, IPointer pointer)
        {
            hash = uint.MaxValue;

            show();
            rect.SetParent(parent);
            rect.SetAsLastSibling();
            state = ProxyState.Released;
            UpdatePointer(pointer);
        }

        /// <summary>
        /// Updates the pointer. This method is called when the pointer is moved.
        /// </summary>
        /// <param name="pointer"> Pointer this cursor represents. </param>
        public void UpdatePointer(IPointer pointer)
        {
            rect.anchoredPosition = pointer.Position;
            var newHash = getPointerHash(pointer);
            if (newHash != hash) updateOnce(pointer);
            hash = newHash;

            update(pointer);
        }

        public void SetState(IPointer pointer, ProxyState newState, object data = null)
        {
            state = newState;
            stateData = data;

            var newHash = getPointerHash(pointer);
            if (newHash != hash) updateOnce(pointer);
            hash = newHash;
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
            defaultSize = rect.sizeDelta.x;
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
        protected virtual void updateOnce(IPointer pointer) {}

        /// <summary>
        /// This method is called every time when the pointer changes.
        /// </summary>
        /// <param name="pointer"> The pointer. </param>
        protected virtual void update(IPointer pointer) {}

        protected virtual uint getPointerHash(IPointer pointer)
        {
            return (uint) state;
        }

        #endregion
    }
}