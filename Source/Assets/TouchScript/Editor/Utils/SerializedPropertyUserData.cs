using System.Collections.Generic;
using System.Text;
using UnityEditor;

namespace TouchScript.Editor.Utils
{
    internal sealed class SerializedPropertyUserData<TUserData> where TUserData : class
    {
        public static SerializedPropertyUserData<TUserData> Instance { get { return instance; } }

        private static SerializedPropertyUserData<TUserData> instance = new SerializedPropertyUserData<TUserData>();
        private static StringBuilder sb = new StringBuilder();
        private Dictionary<string, TUserData> storage = new Dictionary<string, TUserData>();

        internal TUserData this[SerializedProperty property]
        {
            get
            {
                var key = MakeKey(property);
                TUserData data;
                if (!storage.TryGetValue(key, out data)) return null;
                return data;
            }
            set
            {
                var key = MakeKey(property);
                storage[key] = value;
            }
        }

        private string MakeKey(SerializedProperty property)
        {
            if (property == null)
                throw new System.ArgumentException();
            var so = property.serializedObject;
            if (so == null)
                throw new System.ArgumentException();
            var to = so.targetObject;
            if (to == null)
                throw new System.ArgumentException();
            sb.Length = 0;
            sb.Append(to.GetInstanceID().ToString());
            sb.Append(property.propertyPath);
            return sb.ToString();
        }
    }
}