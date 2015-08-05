/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using UnityEngine;

namespace TouchScript.Behaviors
{
    public interface ITransformer
    {
        void ApplyTransform(Transform target);
    }
}
