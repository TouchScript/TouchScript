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

        public Vector3 Point { get { return point; } }

        #endregion

        #region Private variables

        private Vector3 point;

        #endregion

        #region Constructors

        internal TouchHit()
        {}

        #endregion

        #region Internal methods

        internal void InitWith(Transform transform, Vector3 point)
        {
            Transform = transform;
            this.point = point;
        }

        #endregion
    }
}