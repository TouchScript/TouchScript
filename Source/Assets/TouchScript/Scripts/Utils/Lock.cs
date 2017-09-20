namespace TouchScript.Utils
{
    /// <summary>
    /// Holds a value which can only be changed when unlocked.
    /// </summary>
    public class Lock<T>
    {
        public T Value { get; private set; }

        public bool Locked { get; private set; }

        /// <summary>
        /// If unlocked, set the value and lock it.
        /// </summary>
        public void TrySetValue(T value)
        {
            if (!Locked)
            {
                Locked = true;
                Value = value;
            }
        }

        public void Unlock()
        {
            Locked = false;
        }
    }
}