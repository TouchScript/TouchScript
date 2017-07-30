/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System.Text;
using TouchScript.Pointers;
using TouchScript.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace TouchScript.Behaviors.Cursors
{
    /// <summary>
    /// Abstract class for pointer cursors with text.
    /// </summary>
    /// <typeparam name="T">Pointer type.</typeparam>
    /// <seealso cref="TouchScript.Behaviors.Cursors.PointerCursor" />
    public abstract class TextPointerCursor<T> : PointerCursor where T : IPointer
    {
        #region Public properties

        /// <summary>
        /// Should the value of <see cref="Pointer.Id"/> be shown on screen on the cursor.
        /// </summary>
        public bool ShowPointerId = true;

        /// <summary>
        /// Should the value of <see cref="Pointer.Flags"/> be shown on screen on the cursor.
        /// </summary>
        public bool ShowFlags = false;

        /// <summary>
        /// The link to UI.Text component.
        /// </summary>
        public Text Text;

        #endregion

        #region Private variables

        private static StringBuilder stringBuilder = new StringBuilder(64);

        #endregion

        #region Protected methods

        /// <inheritdoc />
        protected override void updateOnce(IPointer pointer)
        {
            base.updateOnce(pointer);

            if (Text == null) return;
            if (!textIsVisible())
            {
                Text.enabled = false;
                return;
            }

            Text.enabled = true;
            stringBuilder.Length = 0;
            generateText((T) pointer, stringBuilder);

            Text.text = stringBuilder.ToString();
        }

        /// <summary>
        /// Generates text for pointer.
        /// </summary>
        /// <param name="pointer">The pointer.</param>
        /// <param name="str">The string builder to use.</param>
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

        /// <summary>
        /// Indicates if text should be visible.
        /// </summary>
        /// <returns><c>True</c> if pointer text should be displayed; <c>false</c> otherwise.</returns>
        protected virtual bool textIsVisible()
        {
            return ShowPointerId || ShowFlags;
        }

        /// <summary>
        /// Typed version of <see cref="getPointerHash"/>. Returns a hash of a cursor state.
        /// </summary>
        /// <param name="pointer">The pointer.</param>
        /// <returns>Integer hash.</returns>
        protected virtual uint gethash(T pointer)
        {
            var hash = (uint) state;
            if (ShowFlags) hash += pointer.Flags << 3;
            return hash;
        }

        /// <inheritdoc />
        protected sealed override uint getPointerHash(IPointer pointer)
        {
            return gethash((T) pointer);
        }

        #endregion
    }

    /// <summary>
    /// Visual cursor implementation used by TouchScript.
    /// </summary>
    [HelpURL("http://touchscript.github.io/docs/html/T_TouchScript_Behaviors_Cursors_PointerCursor.htm")]
    public class PointerCursor : MonoBehaviour
    {
        #region Consts

        /// <summary>
        /// Possible states of a cursor.
        /// </summary>
        public enum CursorState
        {
            /// <summary>
            /// Not pressed.
            /// </summary>
            Released,

            /// <summary>
            /// Pressed.
            /// </summary>
            Pressed,

            /// <summary>
            /// Over something.
            /// </summary>
            Over,

            /// <summary>
            /// Over and pressed.
            /// </summary>
            OverPressed
        }

        #endregion

        #region Public properties

        /// <summary>
        /// Cursor size in pixels.
        /// </summary>
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

        /// <summary>
        /// Current cursor state.
        /// </summary>
        protected CursorState state;

        /// <summary>
        /// CUrrent cursor state data.
        /// </summary>
        protected object stateData;

        /// <summary>
        /// Cached RectTransform.
        /// </summary>
        protected RectTransform rect;

        /// <summary>
        /// Cursor size.
        /// </summary>
        protected float size = 0;

        /// <summary>
        /// Initial cursor size in pixels.
        /// </summary>
        protected float defaultSize;

        /// <summary>
        /// Last data hash.
        /// </summary>
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
            state = CursorState.Released;
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

        /// <summary>
        /// Sets the state of the cursor.
        /// </summary>
        /// <param name="pointer">The pointer.</param>
        /// <param name="newState">The new state.</param>
        /// <param name="data">State data.</param>
        public void SetState(IPointer pointer, CursorState newState, object data = null)
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
                Debug.LogError("PointerCursor must be on an UI element!");
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

        /// <summary>
        /// Returns pointer hash.
        /// </summary>
        /// <param name="pointer">The pointer.</param>
        /// <returns>Integer hash value.</returns>
        protected virtual uint getPointerHash(IPointer pointer)
        {
            return (uint) state;
        }

        #endregion
    }
}