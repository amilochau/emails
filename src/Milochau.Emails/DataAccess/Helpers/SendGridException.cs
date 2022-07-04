using System;
using System.Runtime.Serialization;

namespace Milochau.Emails.DataAccess.Helpers
{
    [Serializable]
    public class SendGridException : SystemException
    {
        /// <summary>Constructor</summary>
        public SendGridException() : base() { }

        /// <summary>Constructor</summary>
        public SendGridException(string message) : base(message) { }

        /// <summary>Constructor</summary>
        public SendGridException(string message, Exception innerException) : base(message, innerException) { }

        /// <summary>Constructor</summary>
        protected SendGridException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
