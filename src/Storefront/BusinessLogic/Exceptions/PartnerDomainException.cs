// -----------------------------------------------------------------------
// <copyright file="PartnerDomainException.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Storefront.BusinessLogic.Exceptions
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    /// <summary>
    /// A custom exception that will be used to communicate business errors within the portal.
    /// </summary>
    [Serializable]
    public class PartnerDomainException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PartnerDomainException"/> class.
        /// </summary>
        public PartnerDomainException()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PartnerDomainException"/> class.
        /// </summary>
        /// <param name="message">The exception message.</param>
        public PartnerDomainException(string message)
            : this(message, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PartnerDomainException"/> class.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
        public PartnerDomainException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PartnerDomainException"/> class.
        /// </summary>
        /// <param name="errorCode">The error code.</param>
        /// <param name="message">The exception message.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
        public PartnerDomainException(ErrorCode errorCode, string message = default(string), Exception innerException = null)
            : this(message, innerException)
        {
            ErrorCode = errorCode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PartnerDomainException"/> class.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
        protected PartnerDomainException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            Details = (Dictionary<string, string>)info.GetValue("Details", typeof(Dictionary<string, string>));
            ErrorCode = (ErrorCode)info.GetValue("ErrorCode", typeof(ErrorCode));
        }

        /// <summary>
        /// Gets the error code which specifies what happened in the business area that caused the error.
        /// </summary>
        public ErrorCode ErrorCode { get; private set; } = ErrorCode.ServerError;

        /// <summary>
        /// Gets a dictionary of the error details.
        /// </summary>
        public IDictionary<string, string> Details { get; private set; } = new Dictionary<string, string>();

        /// <summary>
        /// Required override to add in the serialized parameters.
        /// </summary>
        /// <param name="info">Serialization information.</param>
        /// <param name="context">Streaming context.</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AssertNotNull(nameof(info));

            info.AddValue("ErrorCode", ErrorCode);
            info.AddValue("Details", Details);

            base.GetObjectData(info, context);
        }
    }
}
