/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using UnityEngine;

namespace TouchScript.Hit
{
    /// <summary>
    /// A factory which is used to create instances of ITouchHit.
    /// </summary>
    public sealed class TouchHitFactory : ITouchHitFactory
    {
        /// <summary>
        /// A static instance of a TouchHitFactory which is used to create instances of ITouchHit.
        /// </summary>
        public static ITouchHitFactory Instance
        {
            get { return instance ?? (instance = new TouchHitFactory()); }
        }

        private static TouchHitFactory instance;

        private TouchHitFactory() {}

        /// <inheritdoc />
        public ITouchHit GetTouchHit(RaycastHit value)
        {
            var result = new TouchHit3D();
            result.InitWith(value);
            return result;
        }

        /// <inheritdoc />
        public ITouchHit GetTouchHit(RaycastHit2D value)
        {
            var result = new TouchHit2D();
            result.InitWith(value);
            return result;
        }

        /// <inheritdoc />
        public ITouchHit GetTouchHit(Transform value)
        {
            var result = new TouchHit();
            result.InitWith(value);
            return result;
        }
    }
}
