/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using TouchScript.Utils;
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

        private ObjectPool<TouchHit3D> touchHit3DPool = new ObjectPool<TouchHit3D>(10, null, null, (h) => h.INTERNAL_Reset());
        private ObjectPool<TouchHit2D> touchHit2DPool = new ObjectPool<TouchHit2D>(10, null, null, (h) => h.INTERNAL_Reset());
        private ObjectPool<TouchHit> touchHitPool = new ObjectPool<TouchHit>(10, null, null, (h) => h.INTERNAL_Reset());

        private TouchHitFactory()
        {
        }

        /// <inheritdoc />
        public ITouchHit GetTouchHit(RaycastHit value)
        {
            var result = touchHit3DPool.Get();
            result.INTERNAL_InitWith(value);
            return result;
        }

        /// <inheritdoc />
        public ITouchHit GetTouchHit(RaycastHit2D value)
        {
            var result = touchHit2DPool.Get();
            result.INTERNAL_InitWith(value);
            return result;
        }

        /// <inheritdoc />
        public ITouchHit GetTouchHit(Transform value)
        {
            var result = touchHitPool.Get();
            result.INTERNAL_InitWith(value);
            return result;
        }

        /// <inheritdoc />
        public void ReleaseTouchHit(ITouchHit value)
        {
            if (value is TouchHit3D)
            {
                touchHit3DPool.Release(value as TouchHit3D);
            }
            else if (value is TouchHit2D)
            {
                touchHit2DPool.Release(value as TouchHit2D);    
            } 
            else if (value is TouchHit)
            {
                touchHitPool.Release(value as TouchHit);
            }
        }

    }
}
