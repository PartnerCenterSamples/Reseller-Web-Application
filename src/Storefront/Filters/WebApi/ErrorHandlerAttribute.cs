// -----------------------------------------------------------------------
// <copyright file="ErrorHandler.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Storefront.Filters.WebApi
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Dynamic;
    using System.Net;
    using System.Net.Http;
    using System.Web;
    using System.Web.Http.Filters;
    using BusinessLogic.Exceptions;
    using Newtonsoft.Json;
    using PartnerCenter.Exceptions;

    /// <summary>
    /// A filter that handles portal errors and returns a unified error response.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public sealed class ErrorHandlerAttribute : ExceptionFilterAttribute
    {
        /// <summary>
        /// Intercepts unhandled exceptions and crafts the error response appropriately.
        /// </summary>
        /// <param name="actionExecutedContext">The context for the action.</param>
        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {
            dynamic errorResponsePayload = new ExpandoObject();
            HttpStatusCode errorResponseCode = HttpStatusCode.InternalServerError;

            if (actionExecutedContext.Exception is PartnerDomainException partnerDomainException)
            {
                Trace.TraceError("ErrorHandler: Intercepted PartnerDomainException: {0}.", actionExecutedContext.Exception.ToString());

                switch (partnerDomainException.ErrorCode)
                {
                    case ErrorCode.SubscriptionNotFound:
                    case ErrorCode.PartnerOfferNotFound:
                    case ErrorCode.InvalidFileType:
                    case ErrorCode.InvalidInput:
                    case ErrorCode.MicrosoftOfferImmutable:
                    case ErrorCode.SubscriptionExpired:
                    case ErrorCode.InvalidAddress:
                    case ErrorCode.DomainNotAvailable:
                    case ErrorCode.MaximumRequestSizeExceeded:
                    case ErrorCode.AlreadyExists:
                    case ErrorCode.PaymentGatewayPaymentError:
                    case ErrorCode.PaymentGatewayIdentityFailureDuringConfiguration: // treat this as a retryable bad input. 
                        errorResponseCode = HttpStatusCode.BadRequest;
                        break;

                    case ErrorCode.PaymentGatewayFailure:
                    case ErrorCode.DownstreamServiceError:
                        errorResponseCode = HttpStatusCode.BadGateway;
                        break;
                    default:
                        errorResponseCode = HttpStatusCode.InternalServerError;
                        break;
                }

                errorResponsePayload.ErrorCode = partnerDomainException.ErrorCode;
                errorResponsePayload.Details = partnerDomainException.Details;
            }
            else
            {
                errorResponsePayload.Details = new Dictionary<string, string>();

                if (actionExecutedContext.Exception is PartnerException partnerCenterException &&
                    (partnerCenterException.ErrorCategory == PartnerErrorCategory.BadInput || partnerCenterException.ErrorCategory == PartnerErrorCategory.AlreadyExists))
                {
                    Trace.TraceError("ErrorHandler: Intercepted PartnerException: {0}.", actionExecutedContext.Exception.ToString());
                    errorResponseCode = HttpStatusCode.BadRequest;

                    // can be null. 
                    if (partnerCenterException.ServiceErrorPayload != null)
                    {
                        switch (partnerCenterException.ServiceErrorPayload.ErrorCode)
                        {
                            case "27002":
                                errorResponsePayload.ErrorCode = ErrorCode.InvalidAddress;
                                break;
                            case "27100":
                                errorResponsePayload.ErrorCode = ErrorCode.DomainNotAvailable;
                                break;
                            default:
                                errorResponsePayload.ErrorCode = ErrorCode.InvalidInput;
                                PartnerDomainException tempException = new PartnerDomainException(ErrorCode.DownstreamServiceError).AddDetail("ErrorMessage", partnerCenterException.Message);
                                errorResponsePayload.Details = tempException.Details;
                                break;
                        }
                    }
                    else
                    {
                        // since ServiceErrorPayload is not available. Its better to mark it as downstream service error and send the exception message.                                                 
                        errorResponsePayload.ErrorCode = ErrorCode.DownstreamServiceError;
                        PartnerDomainException tempException = new PartnerDomainException(ErrorCode.DownstreamServiceError).AddDetail("ErrorMessage", partnerCenterException.Message);
                        errorResponsePayload.Details = tempException.Details;
                    }
                }
                else
                {
                    if (actionExecutedContext.Exception is HttpException httpException && httpException.WebEventCode == 3004)
                    {
                        // the maximum request size has been exceeded
                        Trace.TraceError("ErrorHandler: Maximum request size exceeded: {0}.", actionExecutedContext.Exception.ToString());

                        errorResponseCode = HttpStatusCode.BadRequest;
                        errorResponsePayload.ErrorCode = ErrorCode.MaximumRequestSizeExceeded;
                    }
                    else
                    {
                        // any other exception will be treated as a server failure or bug
                        Trace.TraceError("ErrorHandler: Intercepted Exception: {0}. Returning 500 as response.", actionExecutedContext.Exception.ToString());

                        errorResponsePayload.ErrorCode = ErrorCode.ServerError;
                    }
                }
            }

            actionExecutedContext.Response = new HttpResponseMessage(errorResponseCode) { Content = new StringContent(JsonConvert.SerializeObject(errorResponsePayload)) };
        }
    }
}