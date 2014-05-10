using System;
using TouchScript.Hit;
using UnityEngine;

namespace TouchScript.Layers
{
    [AddComponentMenu("TouchScript/Layers/Fullscreen Layer")]
    public sealed class FullscreenLayer : TouchLayer
    {

        #region Public methods

        /// <inheritdoc />
        public override LayerHitResult Hit(Vector2 position, out ITouchHit hit)
        {
            if (base.Hit(position, out hit) == LayerHitResult.Miss) return LayerHitResult.Miss;

            hit = TouchHitFactory.Instance.GetTouchHit(transform);
            var hitTests = transform.GetComponents<HitTest>();
            if (hitTests.Length == 0) return LayerHitResult.Hit;

            foreach (var test in hitTests)
            {
                if (!test.enabled) continue;
                var hitResult = test.IsHit(hit);
                if (hitResult == HitTest.ObjectHitResult.Miss || hitResult == HitTest.ObjectHitResult.Discard) return LayerHitResult.Miss;
            }

            return LayerHitResult.Hit;
        }

        #endregion

        #region Protected functions

        /// <inheritdoc />
        protected override void setName()
        {
            if (String.IsNullOrEmpty(Name)) Name = "Fullscreen";
        }

        #endregion
    }
}
