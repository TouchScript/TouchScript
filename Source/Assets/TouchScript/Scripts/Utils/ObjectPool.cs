/*
 * @author Valentin Simonov / http://va.lent.in/
 * Source code copied from UnityEngine.UI.ObjectPool:
 * https://bitbucket.org/Unity-Technologies/ui/src/ccb946ecc23815d1a7099aee0ed77b0cde7ff278/UnityEngine.UI/UI/Core/Utility/ObjectPool.cs?at=5.1
 */

using System.Collections.Generic;
using UnityEngine.Events;

namespace TouchScript.Utils
{
    internal class ObjectPool<T> where T : new()
    {
        public delegate T0 UnityFunc<T0>();

        private readonly Stack<T> stack;
        private readonly UnityAction<T> onGet;
        private readonly UnityAction<T> onRelease;
        private readonly UnityFunc<T> onNew;

        public int CountAll { get; private set; }

        public int CountActive
        {
            get { return CountAll - CountInactive; }
        }

        public int CountInactive
        {
            get { return stack.Count; }
        }

        public ObjectPool(int capacity, UnityFunc<T> actionNew, UnityAction<T> actionOnGet,
                          UnityAction<T> actionOnRelease)
        {
            stack = new Stack<T>(capacity);
            onNew = actionNew;
            onGet = actionOnGet;
            onRelease = actionOnRelease;
        }

        public void WarmUp(int count)
        {
            for (var i = 0; i < count; i++)
            {
                T element;
                if (onNew != null) element = onNew();
                else element = new T();
                CountAll++;
                stack.Push(element);
            }
        }

        public T Get()
        {
            T element;
            if (stack.Count == 0)
            {
                if (onNew != null) element = onNew();
                else element = new T();
                CountAll++;
            }
            else
            {
                element = stack.Pop();
            }
            if (onGet != null)
                onGet(element);
            return element;
        }

        public void Release(T element)
        {
            if (stack.Count > 0 && ReferenceEquals(stack.Peek(), element))
                UnityEngine.Debug.LogError("Internal error. Trying to destroy object that is already released to pool.");
            if (onRelease != null)
                onRelease(element);
            stack.Push(element);
        }
    }
}