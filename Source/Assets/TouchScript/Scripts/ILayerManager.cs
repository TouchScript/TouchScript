/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using System.Collections.Generic;
using TouchScript.Hit;
using TouchScript.Layers;
using TouchScript.Pointers;
using UnityEngine;

namespace TouchScript
{
    public interface ILayerManager
    {
        /// <summary>
        /// Gets the list of <see cref="TouchLayer"/>.
        /// </summary>
        /// <value>A sorted list of currently active layers.</value>
        IList<TouchLayer> Layers { get; }

        int LayerCount { get; }

        bool HasExclusive { get; }

        /// <summary>
        /// Adds a layer in a specific position.
        /// </summary>
        /// <param name="layer">The layer to add.</param>
        /// <param name="index">Layer index to add the layer to or <c>-1</c> to add to the end of the list.</param>
        /// <param name="addIfExists">if set to <c>true</c> move the layer to another index if it is already added; don't move otherwise.</param>
        /// <returns>
        /// True if the layer was added.
        /// </returns>
        bool AddLayer(TouchLayer layer, int index = -1, bool addIfExists = true);

        /// <summary>
        /// Removes a layer.
        /// </summary>
        /// <param name="layer">The layer to remove.</param>
        /// <returns>True if the layer was removed.</returns>
        bool RemoveLayer(TouchLayer layer);

        /// <summary>
        /// Swaps layers.
        /// </summary>
        /// <param name="at">Layer index 1.</param>
        /// <param name="to">Layer index 2.</param>
        void ChangeLayerIndex(int at, int to);

        void ForEach(Func<TouchLayer, bool> action);

        bool GetHitTarget(IPointer pointer, out HitData hit);

        void SetExclusive(Transform target, bool includeChildren = false);

        void SetExclusive(IEnumerable<Transform> targets);

        bool IsExclusive(Transform target);

        void ClearExclusive();

    }
}
