/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using UnityEngine;

namespace TouchScript.Hit
{
    internal class TouchHit : ITouchHit
    {
        #region Public properties

        public Transform Transform { get; private set; }

        #endregion

        #region Constructors

        internal TouchHit() {}

        #endregion

        #region Internal methods

        internal void InitWith(Transform value)
        {
            Transform = value;
        }

        #endregion
    }
}
