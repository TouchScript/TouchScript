using TouchScript.Devices.Display;
using UnityEngine;

namespace TouchScript.WindowsPhone.Devices.Display
{
    public class DisplayDevice : ScriptableObject, IDisplayDevice
    {

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public virtual float DPI
        {
            get { return dpi; }
            set { dpi = value; }
        }

        [SerializeField]
        protected new string name = "Unknown Device";

        [SerializeField]
        protected float dpi = 96;

        protected virtual void OnEnable()
        {
            base.name = name;
        }

    }
}

