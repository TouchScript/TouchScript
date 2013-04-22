/**
 * @author Valentin Simonov / http://va.lent.in/
 */

using UnityEngine;

namespace TouchScript.Hit
{
    /// <summary>
    /// Parent class for all hit test handlers.
    /// </summary>
    public abstract class HitTest : MonoBehaviour
    {

        public enum ObjectHitResult
        {
            Error = 0,
            Hit = 1,
            Miss = 2,
            Discard = 3
        }

        public virtual ObjectHitResult IsHit(RaycastHit hit)
        {
            return ObjectHitResult.Hit;
        }
    }
}