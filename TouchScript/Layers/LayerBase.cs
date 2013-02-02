/**
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using UnityEngine;

namespace TouchScript.Layers
{
    [ExecuteInEditMode()]
    public class LayerBase : MonoBehaviour
    {
        public String Name;

        public virtual HitResult Hit(Vector2 position, out RaycastHit hit, out Camera hitCamera)
        {
            hit = new RaycastHit();
            hitCamera = null;
            return HitResult.Miss;
        }

        protected virtual void Awake()
        {
            if (GetComponents<LayerBase>().Length > 1)
            {
                DestroyImmediate(this);
                return;
            }

            setName();
            if (Application.isPlaying) TouchManager.AddLayer(this);
        }

        protected virtual void OnDestroy()
        {
            if (Application.isPlaying) TouchManager.RemoveLayer(this);
        }

        protected virtual void setName()
        {
            if (String.IsNullOrEmpty(Name) && camera != null) Name = camera.name;
        }
    }

    public enum HitResult
    {
        Hit,
        Miss,
        Loss,
        Error
    }
}