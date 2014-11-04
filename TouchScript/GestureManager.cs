/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using UnityEngine;

namespace TouchScript
{
    /// <summary>
    /// Internal facade for current instance of <see cref="ITouchManager"/>.
    /// </summary>
    internal sealed class GestureManager : MonoBehaviour
    {
        #region Public properties

        public static IGestureManager Instance
        {
            get { return GestureManagerInstance.Instance; }
        }

        #endregion
    }
}
