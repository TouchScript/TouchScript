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
        public virtual bool IsHit(RaycastHit hit)
        {
            return true;
        }
    }
}