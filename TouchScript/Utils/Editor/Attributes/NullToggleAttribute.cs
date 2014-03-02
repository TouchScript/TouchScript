using UnityEngine;

namespace TouchScript.Utils.Editor.Attributes
{
    public class NullToggleAttribute : PropertyAttribute
    {
        public int NullIntValue = 0;
        public float NullFloatValue = 0f;
        public Object NullObjectValue = null;
    }
}