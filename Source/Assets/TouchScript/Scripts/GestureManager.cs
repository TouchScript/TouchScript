/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using UnityEngine;

namespace TouchScript
{
    /// <summary>
    /// Facade for current instance of <see cref="IGestureManager"/>.
    /// </summary>
    public sealed class GestureManager : MonoBehaviour
    {
        #region Public properties

        /// <summary>
        /// Gets the GestureManager instance.
        /// </summary>
        public static IGestureManager Instance
        {
            get { return GestureManagerInstance.Instance; }
        }

        #endregion
    }
}