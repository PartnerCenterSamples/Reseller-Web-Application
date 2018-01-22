// -----------------------------------------------------------------------
// <copyright file="Extensions.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.CustomerPortal
{
    using System;
    using System.Globalization;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using BusinessLogic.Exceptions;

    /// <summary>
    /// Groups useful extension methods.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Ensures that a given object is not null. Throws an exception otherwise.
        /// </summary>
        /// <param name="objectToValidate">The object we are validating.</param>
        /// <param name="caption">The name to report in the exception.</param>
        public static void AssertNotNull(this object objectToValidate, string caption)
        {
            if (objectToValidate == null)
            {
                throw new ArgumentNullException(caption);
            }
        }

        /// <summary>
        /// Ensures that a string is not empty. Throws an exception if so.
        /// </summary>
        /// <param name="nonEmptyString">The string to validate.</param>
        /// <param name="caption">The name to report in the exception.</param>
        public static void AssertNotEmpty(this string nonEmptyString, string caption)
        {
            if (string.IsNullOrWhiteSpace(nonEmptyString))
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, Resources.AssertStringNotEmptyInvalidError, caption ?? Resources.AssertStringNotEmptyInvalidPrefix)); 
            }
        }

        /// <summary>
        /// Ensures that a given number is greater than zero. Throws an exception otherwise.
        /// </summary>
        /// <param name="number">The number to validate.</param>
        /// <param name="caption">The name to report in the exception.</param>
        public static void AssertPositive(this int number, string caption)
        {
            if (number <= 0)
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, Resources.AssertNumberPositiveInvalidError, caption ?? Resources.AssertNumberPositiveInvalidPrefix)); 
            }
        }

        /// <summary>
        /// Ensures that a given number is greater than zero. Throws an exception otherwise.
        /// </summary>
        /// <param name="number">The number to validate.</param>
        /// <param name="caption">The name to report in the exception.</param>
        public static void AssertPositive(this decimal number, string caption)
        {
            if (number <= 0)
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, Resources.AssertNumberPositiveInvalidError, caption ?? Resources.AssertNumberPositiveInvalidPrefix));                
            }
        }

        /// <summary>
        /// Checks if an exception is fatal.
        /// </summary>
        /// <param name="ex">The exception to check.</param>
        /// <returns>True if Exception is fatal and process should die.</returns>
        public static bool IsFatal(this Exception ex)
        {
            return ex != null && (ex is OutOfMemoryException || ex is AppDomainUnloadedException || ex is BadImageFormatException
                || ex is CannotUnloadAppDomainException || ex is InvalidProgramException || ex is ThreadAbortException || ex is StackOverflowException);
        }

        /// <summary>
        /// Appends a details to a partner domain exception.
        /// </summary>
        /// <param name="exception">The exception to append to.</param>
        /// <param name="key">The detail key.</param>
        /// <param name="value">The detail value.</param>
        /// <returns>The updated exception.</returns>
        public static PartnerDomainException AddDetail(this PartnerDomainException exception, string key, string value)
        {
            exception.AssertNotNull(nameof(exception));
            key.AssertNotEmpty(nameof(key));
            value.AssertNotEmpty(nameof(value));

            exception.Details[key] = value;

            return exception;
        }

        /// <summary>
        /// Ensures that an HTTP response is a successful one. Throws a <see cref="PartnerDomainException"/> otherwise.
        /// </summary>
        /// <param name="httpStatusCode">The HTTP code.</param>
        /// <param name="errorCode">The error code to report in case of failure.</param>
        /// <param name="errorMessage">The error message to report.</param>
        /// <param name="responseBody">A response body to include in the exception if raised.</param>
        public static void AssertHttpResponseSuccess(this int httpStatusCode, ErrorCode errorCode, string errorMessage, object responseBody = null)
        {
            if (!new HttpResponseMessage((HttpStatusCode)httpStatusCode).IsSuccessStatusCode)
            {
                var formattedErrorMessage = string.Format(
                    CultureInfo.InvariantCulture,
                    "{0}. Response code: {1}. Response body: {2}.",
                    errorMessage,
                    httpStatusCode,
                    responseBody);

                throw new PartnerDomainException(errorCode, formattedErrorMessage).AddDetail("ResponseBody", responseBody?.ToString());
            }
        }
    }
}