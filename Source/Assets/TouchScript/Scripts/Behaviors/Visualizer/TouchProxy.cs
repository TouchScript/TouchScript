/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using UnityEngine;
using UnityEngine.UI;

namespace TouchScript.Behaviors.Visualizer
{
    public class TouchProxy : MonoBehaviour
    {

        public Text Text;

        private RectTransform rect;

        public void Init(RectTransform parent, ITouch touch)
        {
            gameObject.SetActive(true);
            rect.SetParent(parent);
            gameObject.name = "Touch id: " + touch.Id;
            Text.text = "Id: " + touch.Id + "\nTags: " + touch.Tags.ToString();

            UpdateTouch(touch);
        }

        public void UpdateTouch(ITouch touch)
        {
            rect.anchoredPosition = touch.Position;
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            gameObject.name = "inactive touch";
        }

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

    }
}
