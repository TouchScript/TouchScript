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
    internal sealed class LayerManagerInstance : MonoBehaviour, ILayerManager
    {
        #region Public properties

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

        #endregion

        #region Private variables

        private static LayerManagerInstance instance;
        private static bool shuttingDown = false;

        private List<TouchLayer> layers = new List<TouchLayer>(10);
        private int layerCount = 0;

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

            gameObject.hideFlags = HideFlags.HideInHierarchy;
            DontDestroyOnLoad(gameObject);
        }

        private void OnApplicationQuit()
        {
            shuttingDown = true;
        }

        #endregion

        #region Private functions

        #endregion

    }
}
