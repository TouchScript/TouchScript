/*
 * @author Valentin Simonov / http://va.lent.in/
 * Source code copied from UnityEngine.UI.ObjectPool:
 * https://bitbucket.org/Unity-Technologies/ui/src/ccb946ecc23815d1a7099aee0ed77b0cde7ff278/UnityEngine.UI/UI/Core/Utility/ObjectPool.cs?at=5.1
 */

using System;
using System.Collections.Generic;
using UnityEngine.Events;

#if OBJECTPOOL_DEBUG
using UnityEngine;
#endif

namespace TouchScript.Utils
{
    /// <exclude />
    public class ObjectPool<T> where T : class
    {
        public delegate T0 UnityFunc<T0>();

        private readonly Stack<T> stack;
        private readonly UnityAction<T> onGet;
        private readonly UnityAction<T> onRelease;
        private readonly UnityFunc<T> onNew;

        public string Name { get; set; }

        public int CountAll { get; private set; }

        public int CountActive
        {
            get { return CountAll - CountInactive; }
        }

        public int CountInactive
        {
            get { return stack.Count; }
        }

        public ObjectPool(int capacity, UnityFunc<T> actionNew, UnityAction<T> actionOnGet = null,
                          UnityAction<T> actionOnRelease = null, string name = null)
        {
            if (actionNew == null) throw new ArgumentException("New action can't be null!");
            stack = new Stack<T>(capacity);
            onNew = actionNew;
            onGet = actionOnGet;
            onRelease = actionOnRelease;
            Name = name;
        }

        public void WarmUp(int count)
        {
            for (var i = 0; i < count; i++)
            {
                var element = onNew();
                CountAll++;
                stack.Push(element);
            }
        }

        public T Get()
        {
#if OBJECTPOOL_DEBUG
            var created = false;
#endif
            T element;
            if (stack.Count == 0)
            {
#if OBJECTPOOL_DEBUG
                created = true;
                logWarning("Created an object.");
#endif
                element = onNew();
                CountAll++;
            }
            else
            {
                element = stack.Pop();
            }
            if (onGet != null) onGet(element);
#if OBJECTPOOL_DEBUG
            log(string.Format("Getting object from pool. New: {0}, count: {1}, left: {2}", created, CountAll, stack.Count));
#endif
            return element;
        }

        public void Release(T element)
        {
#if OBJECTPOOL_DEBUG
            if (stack.Count > 0 && ReferenceEquals(stack.Peek(), element))
                logError("Internal error. Trying to destroy object that is already released to pool.");
#endif
            if (onRelease != null) onRelease(element);
            stack.Push(element);
#if OBJECTPOOL_DEBUG
            log(string.Format("Returned object to pool. Left: {0}", stack.Count));
#endif
        }

        public void Release(object element)
        {
            var obj = (T) element;
            if (obj == null) return;
            Release(obj);
        }

#if OBJECTPOOL_DEBUG
        private void log(string message)
        {
            if (string.IsNullOrEmpty(Name)) return;
            UnityEngine.Debug.LogFormat("[{0}] ObjectPool ({1}): {2}", DateTime.Now.ToString("hh:mm:ss.fff"), Name, message);
        }

        private void logWarning(string message)
        {
            if (string.IsNullOrEmpty(Name)) return;
            UnityEngine.Debug.LogWarningFormat("[{0}] ObjectPool ({1}): {2}", DateTime.Now.ToString("hh:mm:ss.fff"), Name, message);
        }

        private void logError(string message)
        {
            if (string.IsNullOrEmpty(Name)) return;
            UnityEngine.Debug.LogErrorFormat("[{0}] ObjectPool ({1}): {2}", DateTime.Now.ToString("hh:mm:ss.fff"), Name, message);
        }
#endif
    }
}