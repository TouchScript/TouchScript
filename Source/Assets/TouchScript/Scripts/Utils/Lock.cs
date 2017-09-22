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
        /// If unlocked, set the value.
        /// </summary>
        public void TrySetValue(T value)
        {
            if (!Locked)
            {
                Value = value;
            }
        }

        public void SetLock()
        {
            Locked = true;
        }

        public void ClearLock()
        {
            Locked = false;
        }
    }
}