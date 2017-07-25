/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using System.Collections.Generic;
using TouchScript.Hit;
using TouchScript.Layers;
using TouchScript.Pointers;
using UnityEngine;

namespace TouchScript.Core
{
    /// <summary>
    /// Internal implementation of <see cref="ILayerManager"/>.
    /// </summary>
    internal sealed class LayerManagerInstance : MonoBehaviour, ILayerManager
    {
        #region Public properties

        /// <summary>
        /// Gets the instance of GestureManager singleton.
        /// </summary>
        public static ILayerManager Instance
        {
            get
            {
                if (shuttingDown) return null;
                if (instance == null)
                {
                    if (!Application.isPlaying) return null;
                    var objects = FindObjectsOfType<LayerManagerInstance>();
                    if (objects.Length == 0)
                    {
                        var go = new GameObject("GestureManager Instance");
                        instance = go.AddComponent<LayerManagerInstance>();
                    }
                    else if (objects.Length >= 1)
                    {
                        instance = objects[0];
                    }
                }
                return instance;
            }
        }

        /// <inheritdoc />
        public IList<TouchLayer> Layers
        {
            get { return new List<TouchLayer>(layers); }
        }

        /// <inheritdoc />
        public int LayerCount
        {
            get { return layerCount; }
        }

        /// <inheritdoc />
        public bool HasExclusive
        {
            get { return exclusiveCount > 0; }
        }

        #endregion

        #region Private variables

        private static LayerManagerInstance instance;
        private static bool shuttingDown = false;

        private ITouchManager manager;
        private List<TouchLayer> layers = new List<TouchLayer>(10);
        private int layerCount = 0;

        private HashSet<int> exclusive = new HashSet<int>();
        private int exclusiveCount = 0;
        private int clearExclusiveDelay = -1;

        #endregion

        #region Temporary variables

        // Used in SetExclusive().
        private List<Transform> tmpList = new List<Transform>(20);

        #endregion

        #region Public methods

        /// <inheritdoc />
        public bool AddLayer(TouchLayer layer, int index = -1, bool addIfExists = true)
        {
            if (layer == null) return false;

            var i = layers.IndexOf(layer);
            if (i != -1)
            {
                if (!addIfExists) return false;
                layers.RemoveAt(i);
                layerCount--;
            }
            if (index == 0)
            {
                layers.Insert(0, layer);
                layerCount++;
                return i == -1;
            }
            if (index == -1 || index >= layerCount)
            {
                layers.Add(layer);
                layerCount++;
                return i == -1;
            }
            if (i != -1)
            {
                if (index < i) layers.Insert(index, layer);
                else layers.Insert(index - 1, layer);
                layerCount++;
                return false;
            }
            layers.Insert(index, layer);
            layerCount++;
            return true;
        }

        /// <inheritdoc />
        public bool RemoveLayer(TouchLayer layer)
        {
            if (layer == null) return false;
            var result = layers.Remove(layer);
            if (result) layerCount--;
            return result;
        }

        /// <inheritdoc />
        public void ChangeLayerIndex(int at, int to)
        {
            if (at < 0 || at >= layerCount) return;
            if (to < 0 || to >= layerCount) return;
            var data = layers[at];
            layers.RemoveAt(at);
            layers.Insert(to, data);
        }

        /// <inheritdoc />
        public void ForEach(Func<TouchLayer, bool> action)
        {
            for (var i = 0; i < layerCount; i++)
            {
                if (!action(layers[i])) break;
            }
        }

        /// <inheritdoc />
        public bool GetHitTarget(IPointer pointer, out HitData hit)
        {
            hit = default(HitData);

            for (var i = 0; i < layerCount; i++)
            {
                var touchLayer = layers[i];
                if (touchLayer == null) continue;
                var result = touchLayer.Hit(pointer, out hit);
                switch (result)
                {
                    case HitResult.Hit:
                        return true;
                    case HitResult.Discard:
                        return false;
                }
            }

            return false;
        }

        /// <inheritdoc />
        public void SetExclusive(Transform target, bool includeChildren = false)
        {
            if (target == null) return;
            exclusive.Clear();
            clearExclusiveDelay = -1;

            exclusive.Add(target.GetHashCode());
            exclusiveCount = 1;
            if (includeChildren)
            {
                target.GetComponentsInChildren(tmpList);
                foreach (var t in tmpList) exclusive.Add(t.GetHashCode());
                exclusiveCount += tmpList.Count;
            }
        }

        /// <inheritdoc />
        public void SetExclusive(IEnumerable<Transform> targets)
        {
            if (targets == null) return;
            exclusive.Clear();
            clearExclusiveDelay = -1;

            foreach (var t in targets)
            {
                exclusive.Add(t.GetHashCode());
                exclusiveCount++;
            }
        }

        /// <inheritdoc />
        public bool IsExclusive(Transform target)
        {
            return exclusive.Contains(target.GetHashCode());
        }

        /// <inheritdoc />
        public void ClearExclusive()
        {
            // It is incorrect to just set exclusiveCount to zero since the exclusive list is actually needed the next frame. Only after the next frame's FrameEnded event the list can be cleared.
            // If we are inside the Pointer Frame, we need to wait for the second FrameEnded (this frame's event included). Otherwise, we need to wait for the next FrameEnded event.
            clearExclusiveDelay = manager.IsInsidePointerFrame ? 2 : 1;
        }

        #endregion

        #region Unity

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else if (instance != this)
            {
                Destroy(this);
                return;
            }

            manager = TouchManager.Instance;

            gameObject.hideFlags = HideFlags.HideInHierarchy;
            DontDestroyOnLoad(gameObject);
        }

        private void OnEnable()
        {
            manager.FrameFinished += frameFinishedHandler;
        }

        private void OnDisable()
        {
            manager.FrameFinished -= frameFinishedHandler;
        }

        private void OnApplicationQuit()
        {
            shuttingDown = true;
        }

        #endregion

        #region Private functions

        #endregion

        #region Event handlers

        private void frameFinishedHandler(object sender, EventArgs eventArgs)
        {
            clearExclusiveDelay--;
            if (clearExclusiveDelay == 0)
            {
                exclusive.Clear();
                exclusiveCount = 0;
            }
        }

        #endregion
    }
}