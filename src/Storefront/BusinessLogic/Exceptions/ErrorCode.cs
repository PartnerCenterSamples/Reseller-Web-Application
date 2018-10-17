// -----------------------------------------------------------------------
// <copyright file="ErrorCode.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Storefront.BusinessLogic.Exceptions
{
    /// <summary>
    /// Categorizes errors that can happen during business domain operations.
    /// </summary>
    public enum ErrorCode
    {
        /// <summary>
        /// The server had a failure it can't understand.
        /// </summary>
        ServerError,

        /// <summary>
        /// The resource already exists.
        /// </summary>
        AlreadyExists,

        /// <summary>
        /// An invalid set of inputs was provided.
        /// </summary>
        InvalidInput,

        /// <summary>
        /// A dependent service has failed.
        /// </summary>
        DownstreamServiceError,

        /// <summary>
        /// The given subscription could not be found.
        /// </summary>
        SubscriptionNotFound,

        /// <summary>
        /// The subscription is expired.
        /// </summary>
        SubscriptionExpired,

        /// <summary>
        /// The subscription could not be updated.
        /// </summary>
        SubscriptionUpdateFailure,

        /// <summary>
        /// The requested partner offer was not found.
        /// </summary>
        PartnerOfferNotFound,

        /// <summary>
        /// Failure in accessing persistence.
        /// </summary>
        PersistenceFailure,

        /// <summary>
        /// Unexpected file type.
        /// </summary>
        InvalidFileType,

        /// <summary>
        /// Failure in payment gateway.
        /// </summary>
        PaymentGatewayFailure,

        /// <summary>
        /// A failure due to an attempt to update the Microsoft offer associated with a partner offer.
        /// </summary>
        MicrosoftOfferImmutable,

        /// <summary>
        /// Maximum request size exceeded error.
        /// </summary>
        MaximumRequestSizeExceeded,

        /// <summary>
        /// Invalid address.
        /// </summary>
        InvalidAddress,

        /// <summary>
        /// Domain not available.
        /// </summary>
        DomainNotAvailable,

        /// <summary>
        /// Purchasing a delete offer.
        /// </summary>
        PurchaseDeletedOfferNotAllowed,

        /// <summary>
        /// Failure in payment gateway authentication during configuration.
        /// </summary>
        PaymentGatewayIdentityFailureDuringConfiguration,

        /// <summary>
        /// Failure in payment gateway authentication during payment.
        /// </summary>
        PaymentGatewayIdentityFailureDuringPayment,

        /// <summary>
        /// Failure in payment gateway during payment.
        /// </summary>
        PaymentGatewayPaymentError
    }
}