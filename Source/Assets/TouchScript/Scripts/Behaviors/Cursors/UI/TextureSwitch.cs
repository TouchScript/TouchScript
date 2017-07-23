/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using UnityEngine;

namespace TouchScript.Behaviors.Cursors.UI
{
    /// <summary>
    /// A helper class to turn on and off <see cref="CanvasRenderer"/> without causing allocations.
    /// </summary>
    public class TextureSwitch : MonoBehaviour
    {

        private CanvasRenderer r;

        /// <summary>
        /// Shows this instance.
        /// </summary>
        public void Show()
        {
            r.SetAlpha(1);
        }

        /// <summary>
        /// Hides this instance.
        /// </summary>
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