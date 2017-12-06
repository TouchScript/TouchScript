using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TouchScript.Editor.Utils
{
    internal sealed class SerializedPropertyUserData<TUserData> where TUserData : class
    {
        const int GCThreshold = 1000;

        static SerializedPropertyUserData<TUserData> s_instance = new SerializedPropertyUserData<TUserData>();

        public static SerializedPropertyUserData<TUserData> Instance { get { return s_instance; } }

        private struct Storage
        {
            internal Object targetObject;
            internal TUserData userData;
        }

        private Dictionary<string, Storage> storage = new Dictionary<string, Storage>();
        private int accessCount = 0;

        internal TUserData this[SerializedProperty property]
        {
            get
            {
                GC();
                var key = MakeKey(property);
                Storage s;
                if (!storage.TryGetValue(key, out s))
                    return null;
                return s.userData;
            }
            set
            {
                var key = MakeKey(property);
                storage[key] = new Storage
                {
                    targetObject = property.serializedObject.targetObject,
                    userData = value,
                };
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
            return to.GetInstanceID().ToString() + "," + property.propertyPath;
        }

        private void GC()
        {
            ++ accessCount;
            if (accessCount < GCThreshold)
                return;
            accessCount = 0;
            var newStorage = new Dictionary<string, Storage>();
            foreach (var kv in storage)
            {
                if (kv.Value.targetObject != null)
                    newStorage[kv.Key] = kv.Value;
                //else Debug.LogFormat("GC: Expired: {0}", kv.Key);
            }
            storage = newStorage;
        }
    }
}