using System;
using System.Runtime.Serialization;

namespace BackgroundProcessing.Core
{
    /// <summary>
    /// Exceptions related to background processing execution.
    /// </summary>
    [Serializable]
    public class BackgroundProcessingException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BackgroundProcessingException"/> class.
        /// </summary>
        public BackgroundProcessingException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BackgroundProcessingException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public BackgroundProcessingException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BackgroundProcessingException"/> class with a specified error message
        /// and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="inner">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public BackgroundProcessingException(string message, Exception inner)
            : base(message, inner)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BackgroundProcessingException"/> class with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
        protected BackgroundProcessingException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
