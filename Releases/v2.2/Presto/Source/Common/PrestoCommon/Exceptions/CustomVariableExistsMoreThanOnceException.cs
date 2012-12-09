using System;
using System.Runtime.Serialization;

namespace PrestoCommon.Exceptions
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class CustomVariableExistsMoreThanOnceException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CustomVariableMissingException"/> class.
        /// </summary>
        public CustomVariableExistsMoreThanOnceException() : base() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomVariableMissingException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public CustomVariableExistsMoreThanOnceException(string message) : base(message) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomVariableMissingException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="ex">The ex.</param>
        public CustomVariableExistsMoreThanOnceException(string message, Exception ex) : base(message, ex) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomVariableMissingException"/> class.
        /// </summary>
        /// <param name="serializationInfo">The serialization info.</param>
        /// <param name="streamingContext">The streaming context.</param>
        protected CustomVariableExistsMoreThanOnceException(SerializationInfo serializationInfo, StreamingContext streamingContext)
            : base(serializationInfo, streamingContext) { }
    }
}
