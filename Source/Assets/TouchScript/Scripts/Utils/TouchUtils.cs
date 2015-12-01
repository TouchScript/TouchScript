/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using TouchScript.Hit;
using TouchScript.Layers;
using UnityEngine;

namespace TouchScript.Utils
{
    public static class TouchUtils
    {

        public static bool IsTouchOnTarget(ITouch touch, Transform target)
        {
            if (touch == null || touch.Layer == null || target == null) return false;
            TouchHit hit;
            if ((touch.Layer.Hit(touch.Position, out hit) == TouchLayer.LayerHitResult.Hit) &&
                (target == hit.Transform || hit.Transform.IsChildOf(target)))
                return true;
            return false;
        }

        public static bool IsTouchOnTarget(ITouch touch)
        {
            if (touch == null) return false;
            return IsTouchOnTarget(touch, touch.Target);
        }

    }
}
