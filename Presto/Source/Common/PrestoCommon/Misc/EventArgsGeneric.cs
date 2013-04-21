using System;

namespace PrestoCommon.Misc
{
    public class EventArgs<T> : EventArgs
    {
        private T _value;

        public T Value
        {
            get { return _value; }
        }

        public EventArgs(T value)
        {
            this._value = value;
        }
    }
}
