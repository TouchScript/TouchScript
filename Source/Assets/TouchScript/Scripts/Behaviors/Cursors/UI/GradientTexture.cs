/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TouchScript.Behaviors.Cursors.UI
{
    public class GradientTexture : MonoBehaviour
    {

        public enum Res
        {
            Pix16 = 16,
            Pix32 = 32,
            Pix64 = 64,
            Pix128 = 128,
            Pix256 = 256,
            Pix512 = 512
        }

        public Gradient Gradient = new Gradient();
        public string Name = "Gradient";
        public Res Resolution = Res.Pix128;

        private Texture2D texture;

        private static Dictionary<int, Texture2D> textureCache = new Dictionary<int, Texture2D>(); 

        public Texture2D Generate()
        {
            var res = (int) Resolution;
            var tex = new Texture2D(res, 1, TextureFormat.ARGB32, false, true);
            tex.name = Name;
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            Color[] colors = new Color[res];
            float div = res;
            for (var i = 0; i < res; i++)
            {
                float t = i / div;
                colors[i] = Gradient.Evaluate(t);
            }
            tex.SetPixels(colors);
            tex.Apply(false, true);

            return tex;
        }

        private void Start()
        {
            var hash = Name.GetHashCode();
            if (!textureCache.TryGetValue(hash, out texture))
            {
                texture = Generate();
                textureCache.Add(hash, texture);
            }
            apply();
        }

        private void OnValidate()
        {
            refresh();
        }

        private void refresh()
        {
            if (texture != null)
                DestroyImmediate(texture);
            texture = Generate();
            apply();
        }

        private void apply()
        {
            var r = GetComponent<RawImage>();
            if (r == null) throw new Exception("GradientTexture must be on an UI element with RawImage component.");
            r.texture = texture;
        }
    }
}