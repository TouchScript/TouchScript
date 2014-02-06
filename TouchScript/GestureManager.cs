/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using UnityEngine;

namespace TouchScript
{
    [AddComponentMenu("TouchScript/Gesture Manager")]
    public class GestureManager : MonoBehaviour
    {
        #region Public properties

        public static IGestureManager Instance { get { return GestureManagerInstance.Instance; } }

        #endregion

    }
}