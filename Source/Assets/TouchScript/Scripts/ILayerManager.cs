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
    /// <summary>
    /// Core manager which controls TouchLayers.
    /// </summary>
    public interface ILayerManager
    {
        /// <summary>
        /// Gets the list of <see cref="TouchLayer"/>.
        /// </summary>
        /// <value>A sorted list of currently active layers.</value>
        IList<TouchLayer> Layers { get; }

        /// <summary>
        /// Gets the number of active layers.
        /// </summary>
        /// <value> The number of active layers. </value>
        int LayerCount { get; }

        /// <summary>
        /// Indicates whether there are currently any exclusive transforms.
        /// </summary>
        /// <value>
        ///   <c>true</c> if any exclusive transforms are registered; otherwise, <c>false</c>.
        /// </value>
        /// <seealso cref="SetExclusive(UnityEngine.Transform,bool)"/>
        /// <seealso cref="ClearExclusive"/>
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

        /// <summary>
        /// Executes an action over all layers in order.
        /// </summary>
        /// <param name="action">The action to execute. If it returns true, execution stops.</param>
        void ForEach(Func<TouchLayer, bool> action);

        /// <summary>
        /// Detects if the pointer hits any object in the scene.
        /// </summary>
        /// <param name="pointer">The pointer.</param>
        /// <param name="hit">Hit structure to fill on success.</param>
        /// <returns>True if any object is hit.</returns>
        bool GetHitTarget(IPointer pointer, out HitData hit);

        /// <summary>
        /// Sets the exclusive transform. Only exclusive transforms will be able to receive pointers.
        /// </summary>
        /// <param name="target">The exclusive transform.</param>
        /// <param name="includeChildren">if set to <c>true</c> target's children will also be added.</param>
        void SetExclusive(Transform target, bool includeChildren = false);

        /// <summary>
        /// Sets the exclusive transforms. Only exclusive transforms will be able to receive pointers.
        /// </summary>
        /// <param name="targets">The exclusive transforms to set.</param>
        void SetExclusive(IEnumerable<Transform> targets);

        /// <summary>
        /// Determines whether the specified target is exclusive.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <returns>
        ///   <c>true</c> if the specified target is exclusive; otherwise, <c>false</c>.
        /// </returns>
        bool IsExclusive(Transform target);

        /// <summary>
        /// Clears the exclusive transforms list.
        /// </summary>
        void ClearExclusive();
    }
}