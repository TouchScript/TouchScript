/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using UnityEngine;
using UnityEngine.UI;

namespace TouchScript.Behaviors.Visualizer
{
    public class TouchProxy : TouchProxyBase
    {
        public Text Text;

        #region Protected methods

        protected override void updateOnce(ITouch touch)
        {
            base.updateOnce(touch);

            gameObject.name = "Touch id: " + touch.Id;

            if (Text == null) return;
            if (!ShowTouchId && !ShowTags) return;

            string text = "";
            if (ShowTouchId) text += "Id: " + touch.Id;
            if (ShowTags)
            {
                if (text != "") text += "\n";
                text += "Tags: " + touch.Tags.ToString();
            }
            Text.text = text;
        }

        #endregion
    }

    public class TouchProxyBase : MonoBehaviour
    {
        #region Public properties

        public int Size
        {
            get { return size; }
            set
            {
                size = value;
                rect.sizeDelta = Vector2.one*size;
            }
        }

        public bool ShowTouchId { get; set; }

        public bool ShowTags { get; set; }

        #endregion

        #region Private variables

        protected RectTransform rect;
        protected int size = 1;

        #endregion

        #region Public methods

        public void Init(RectTransform parent, ITouch touch)
        {
            show();
            rect.SetParent(parent);
            updateOnce(touch);
            update(touch);
        }

        public void UpdateTouch(ITouch touch)
        {
            update(touch);
        }

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

        protected virtual void hide()
        {
            gameObject.SetActive(false);
            gameObject.name = "inactive touch";
        }

        protected virtual void show()
        {
            gameObject.SetActive(true);
        }

        protected virtual void updateOnce(ITouch touch)
        {
        }

        public virtual void update(ITouch touch)
        {
            rect.anchoredPosition = touch.Position;
        }

        #endregion
    }
}