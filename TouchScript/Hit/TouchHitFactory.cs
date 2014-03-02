using UnityEngine;

namespace TouchScript.Hit
{
    internal sealed class TouchHitFactory
    {

        public static TouchHitFactory Instance
        {
            get
            {
                if (instance == null) instance = new TouchHitFactory();
                return instance;
            }
        }

        private static TouchHitFactory instance;

        private TouchHitFactory()
        {}

        public ITouchHit GetTouchHit(RaycastHit value)
        {
            var result = new TouchHit3D();
            result.InitWith(value);
            return result;
        }

        public ITouchHit GetTouchHit(RaycastHit2D value)
        {
            var result = new TouchHit2D();
            result.InitWith(value);
            return result;
        }

        public ITouchHit GetTouchHit(Transform value)
        {
            var result = new TouchHit();
            result.InitWith(value);
            return result;
        }

    }
}
