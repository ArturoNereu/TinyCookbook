using System;
using System.Runtime.Serialization;

namespace Unity.Serialization
{
    [Serializable]
    public class BufferOverflowException : Exception
    {
        public BufferOverflowException(string message) : base(message)
        {

        }

        public BufferOverflowException(string message, Exception inner) : base(message, inner)
        {

        }

        protected BufferOverflowException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
