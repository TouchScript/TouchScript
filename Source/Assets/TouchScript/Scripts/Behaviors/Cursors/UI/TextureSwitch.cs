/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using UnityEngine;
using UnityEngine.UI;

namespace TouchScript.Behaviors.Cursors.UI
{
    public class TextureSwitch : MonoBehaviour
    {

        private CanvasRenderer r;

        public void Show()
        {
            r.SetAlpha(1);
        }

        public void Hide()
        {
            r.SetAlpha(0);
        }

        private void Awake()
        {
            r = GetComponent<CanvasRenderer>();
        }

    }
}