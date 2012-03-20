using System;
using System.Runtime.Serialization;

namespace PrestoCommon.Exceptions
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class CustomVariableMissingException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CustomVariableMissingException"/> class.
        /// </summary>
        public CustomVariableMissingException() : base() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomVariableMissingException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public CustomVariableMissingException(string message) : base(message) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomVariableMissingException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="ex">The ex.</param>
        public CustomVariableMissingException(string message, Exception ex) : base(message, ex) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomVariableMissingException"/> class.
        /// </summary>
        /// <param name="serializationInfo">The serialization info.</param>
        /// <param name="streamingContext">The streaming context.</param>
        protected CustomVariableMissingException(SerializationInfo serializationInfo, StreamingContext streamingContext)
            : base(serializationInfo, streamingContext) { }
    }
}
