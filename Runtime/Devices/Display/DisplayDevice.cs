/*
 * @author Valentin Simonov / http://va.lent.in/
 */

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace TouchScript.Devices.Display
{
    /// <summary>
    /// A simple display device which inherits from <see cref="ScriptableObject"/> and can be saved in Unity assets.
    /// </summary>
    [HelpURL("http://touchscript.github.io/docs/html/T_TouchScript_Devices_Display_DisplayDevice.htm")]
    [CreateAssetMenu(fileName = "DisplayDevice", menuName = "TouchScript/New Display Device", order = 0)]
    public class DisplayDevice : ScriptableObject, IDisplayDevice
    {
        /// <inheritdoc />
        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                base.name = value;
            }
        }

        /// <inheritdoc />
        public virtual float DPI
        {
            get { return dpi; }
        }

        /// <inheritdoc />
        public virtual float NativeDPI
        {
            get { return nativeDPI; }
        }

        /// <inheritdoc />
        public virtual Vector2 NativeResolution
        {
            get { return nativeResolution; }
        }

        /// <summary>
        /// Serialized device name.
        /// </summary>
        [SerializeField]
        protected new string name = "Unknown Device";

        /// <summary>
        /// Serialized device DPI.
        /// </summary>
        [SerializeField]
        protected float dpi = 96;

        /// <summary>
        /// Native device dpi.
        /// </summary>
        [SerializeField]
        protected float nativeDPI = 96;

        /// <summary>
        /// Native device resolution.
        /// </summary>
        [SerializeField]
        protected Vector2 nativeResolution = new Vector2(1920, 1080);

        /// <inheritdoc />
        public virtual void UpdateDPI() {}

        /// <summary>
        /// OnEnable Unity method.
        /// </summary>
        protected virtual void OnEnable()
        {
            base.name = name;
        }
    }
}