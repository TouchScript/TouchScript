/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System.Collections.Generic;
using TouchScript.Layers;
using UnityEngine;

namespace TouchScript
{
    [AddComponentMenu("TouchScript/Touch Manager")]
    public class TouchManager : MonoBehaviour
    {
        #region Constants

        /// <summary>
        /// Ratio of cm to inch
        /// </summary>
        public const float CM_TO_INCH = 0.393700787f;

        /// <summary>
        /// Ratio of inch to cm
        /// </summary>
        public const float INCH_TO_CM = 1/CM_TO_INCH;

        #endregion

        #region Public properties

        /// <summary>
        /// TouchManager singleton instance.
        /// </summary>
        public static ITouchManager Instance { get { return TouchManagerInstance.Instance; } }

        /// <summary>
        /// Current DPI.
        /// </summary>
        public float DPI
        {
            get
            {
                if (Instance == null)
                {
                    return Application.isEditor ? editorDpi : liveDpi;
                }
                return Instance.DPI;
            }
            set
            {
                if (Instance == null)
                {
                    if (Application.isEditor) editorDpi = value;
                    else liveDpi = value;
                } else
                {
                    Instance.DPI = value;
                }
            }
        }

        /// <summary>
        /// DPI while testing in editor.
        /// </summary>
        public float EditorDPI
        {
            get
            {
                if (Instance == null) return editorDpi;
                return Instance.EditorDPI;
            }
            set
            {
                if (Instance == null) editorDpi = value;
                else Instance.EditorDPI = value;
            }
        }

        /// <summary>
        /// DPI of target touch device.
        /// </summary>
        public float LiveDPI
        {
            get
            {
                if (Instance == null) return liveDpi;
                return Instance.LiveDPI;
            }
            set
            {
                if (Instance == null) liveDpi = value;
                else Instance.LiveDPI = value;
            }
        }

        #endregion

        #region Private variables

        [SerializeField]
        private float liveDpi = 72;

        [SerializeField]
        private float editorDpi = 72;

        [SerializeField]
        private List<TouchLayer> layers = new List<TouchLayer>();

        #endregion

        #region Unity

        private void OnEnable()
        {
            if (Instance == null) return;
            
            Instance.LiveDPI = liveDpi;
            Instance.EditorDPI = editorDpi;
            for (var i = 0; i < layers.Count; i++)
            {
                Instance.AddLayer(layers[i], i);
            }
        }

        #endregion
        
    }
}