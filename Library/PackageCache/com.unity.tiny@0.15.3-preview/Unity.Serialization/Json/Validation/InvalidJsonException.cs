using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Unity.Serialization.Json
{
    /// <summary>
    /// The exception thrown when JSON input is invalid.
    /// </summary>
    [Serializable]
    public class InvalidJsonException : Exception
    {
        /// <summary>
        /// The line the validator stopped at.
        /// </summary>
        public int Line { get; set; }

        /// <summary>
        /// The character the validator stopped at.
        /// </summary>
        public int Character { get; set; }

        public InvalidJsonException(string message)
            : base(message)
        {

        }

        public InvalidJsonException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected InvalidJsonException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            Line = info.GetInt32(nameof(Line));
            Character = info.GetInt32(nameof(Character));
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            info.AddValue(nameof(Line), Line);
            info.AddValue(nameof(Character), Character);
            base.GetObjectData(info, context);
        }
    }
}
