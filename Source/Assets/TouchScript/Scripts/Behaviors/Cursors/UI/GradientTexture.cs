/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TouchScript.Behaviors.Cursors.UI
{
    /// <summary>
    /// Generates a texture with a circle gradient.
    /// </summary>
    [HelpURL("http://touchscript.github.io/docs/html/T_TouchScript_Behaviors_Cursors_UI_GradientTexture.htm")]
    public class GradientTexture : MonoBehaviour
    {
        /// <summary>
        /// Resolution in pixels.
        /// </summary>
        public enum Res
        {
            /// <summary>
            /// 16x16
            /// </summary>
            Pix16 = 16,

            /// <summary>
            /// 32x32
            /// </summary>
            Pix32 = 32,

            /// <summary>
            /// 64x64
            /// </summary>
            Pix64 = 64,

            /// <summary>
            /// 128x128
            /// </summary>
            Pix128 = 128,

            /// <summary>
            /// 256x256
            /// </summary>
            Pix256 = 256,

            /// <summary>
            /// 512x512
            /// </summary>
            Pix512 = 512
        }

        /// <summary>
        /// The gradient.
        /// </summary>
        public Gradient Gradient = new Gradient();

        /// <summary>
        /// Gradient's name. Used to cache textures.
        /// </summary>
        public string Name = "Gradient";

        /// <summary>
        /// Texture resolution.
        /// </summary>
        public Res Resolution = Res.Pix128;

        private Texture2D texture;
        private static Dictionary<int, Texture2D> textureCache = new Dictionary<int, Texture2D>();

        /// <summary>
        /// Generates the gradient texture.
        /// </summary>
        /// <returns>Generated texture.</returns>
        public Texture2D Generate()
        {
            var res = (int) Resolution;
            var tex = new Texture2D(res, 1, TextureFormat.ARGB32, false, true)
            {
                name = Name,
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };

            var colors = new Color[res];
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