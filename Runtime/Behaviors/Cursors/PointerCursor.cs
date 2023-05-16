/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System.Text;
using TMPro;
using TouchScript.Pointers;
using TouchScript.Utils;
using UnityEngine;

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
        public TMP_Text Text;

        #endregion

        #region Private variables

        private static StringBuilder s_stringBuilder = new StringBuilder(64);

        #endregion

        #region Protected methods

        /// <inheritdoc />
        protected override void UpdateOnce(IPointer pointer)
        {
            base.UpdateOnce(pointer);

            if (Text == null) return;
            if (!TextIsVisible())
            {
                Text.enabled = false;
                return;
            }

            Text.enabled = true;
            s_stringBuilder.Length = 0;
            GenerateText((T) pointer, s_stringBuilder);

            Text.text = s_stringBuilder.ToString();
        }

        /// <summary>
        /// Generates text for pointer.
        /// </summary>
        /// <param name="pointer">The pointer.</param>
        /// <param name="str">The string builder to use.</param>
        protected virtual void GenerateText(T pointer, StringBuilder str)
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
        protected virtual bool TextIsVisible()
        {
            return ShowPointerId || ShowFlags;
        }

        /// <summary>
        /// Typed version of <see cref="GetPointerHash"/>. Returns a hash of a cursor state.
        /// </summary>
        /// <param name="pointer">The pointer.</param>
        /// <returns>Integer hash.</returns>
        protected virtual uint GetHash(T pointer)
        {
            var hash = (uint) State;
            if (ShowFlags) hash += pointer.Flags << 3;
            return hash;
        }

        /// <inheritdoc />
        protected sealed override uint GetPointerHash(IPointer pointer)
        {
            return GetHash((T) pointer);
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
            get => _size;
            set
            {
                _size = value;
                if (_size > 0)
                {
                    Rect.sizeDelta = Vector2.one * _size;
                }
                else
                {
                    _size = 0;
                    Rect.sizeDelta = Vector2.one * _defaultSize;
                }
            }
        }

        #endregion

        #region Private variables

        /// <summary>
        /// Current cursor state.
        /// </summary>
        protected CursorState State;

        /// <summary>
        /// Current cursor state data.
        /// </summary>
        protected object StateData;

        /// <summary>
        /// Cached RectTransform.
        /// </summary>
        protected RectTransform Rect;

        /// <summary>
        /// Cursor size.
        /// </summary>
        private float _size = 0;

        /// <summary>
        /// Initial cursor size in pixels.
        /// </summary>
        private float _defaultSize;

        /// <summary>
        /// Last data hash.
        /// </summary>
        private uint _hash = uint.MaxValue;

        private CanvasGroup _group;

        #endregion

        #region Public methods

        /// <summary>
        /// Initializes (resets) the cursor.
        /// </summary>
        /// <param name="parent"> Parent container. </param>
        /// <param name="pointer"> Pointer this cursor represents. </param>
        public void Init(RectTransform parent, IPointer pointer)
        {
            _hash = uint.MaxValue;
            _group = GetComponent<CanvasGroup>();

            Show();
            Rect.SetParent(parent);
            Rect.SetAsLastSibling();
            State = CursorState.Released;

            UpdatePointer(pointer);
        }

        /// <summary>
        /// Updates the pointer. This method is called when the pointer is moved.
        /// </summary>
        /// <param name="pointer"> Pointer this cursor represents. </param>
        public void UpdatePointer(IPointer pointer)
        {
            Rect.anchoredPosition = pointer.Position;
            var newHash = GetPointerHash(pointer);
            if (newHash != _hash) UpdateOnce(pointer);
            _hash = newHash;

            UpdatePointerInternal(pointer);
        }

        /// <summary>
        /// Sets the state of the cursor.
        /// </summary>
        /// <param name="pointer">The pointer.</param>
        /// <param name="newState">The new state.</param>
        /// <param name="data">State data.</param>
        public void SetState(IPointer pointer, CursorState newState, object data = null)
        {
            State = newState;
            StateData = data;

            var newHash = GetPointerHash(pointer);
            if (newHash != _hash) UpdateOnce(pointer);
            _hash = newHash;
        }

        /// <summary>
        /// Hides this instance.
        /// </summary>
        public void Hide()
        {
            HideInternal();
        }

        #endregion

        #region Unity methods

        private void Awake()
        {
            Rect = transform as RectTransform;
            if (Rect == null)
            {
                Debug.LogError("PointerCursor must be on an UI element!");
                enabled = false;
                return;
            }
            Rect.anchorMin = Rect.anchorMax = Vector2.zero;
            _defaultSize = Rect.sizeDelta.x;
        }

        #endregion

        #region Protected methods

        /// <summary>
        /// Hides (clears) this instance.
        /// </summary>
        protected virtual void HideInternal()
        {
            _group.alpha = 0;
#if UNITY_EDITOR
            gameObject.name = "Inactive Pointer";
#endif
        }

        /// <summary>
        /// Shows this instance.
        /// </summary>
        protected virtual void Show()
        {
            _group.alpha = 1;
#if UNITY_EDITOR
            gameObject.name = "Pointer";
#endif
        }

        /// <summary>
        /// This method is called once when the cursor is initialized.
        /// </summary>
        /// <param name="pointer"> The pointer. </param>
        protected virtual void UpdateOnce(IPointer pointer) {}

        /// <summary>
        /// This method is called every time when the pointer changes.
        /// </summary>
        /// <param name="pointer"> The pointer. </param>
        protected virtual void UpdatePointerInternal(IPointer pointer) {}

        /// <summary>
        /// Returns pointer hash.
        /// </summary>
        /// <param name="pointer">The pointer.</param>
        /// <returns>Integer hash value.</returns>
        protected virtual uint GetPointerHash(IPointer pointer)
        {
            return (uint) State;
        }

        #endregion
    }
}