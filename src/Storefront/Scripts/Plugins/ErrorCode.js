Microsoft.WebPortal.ErrorCode = {
    /// <summary>
    /// The server had a failure it can't understand.
    /// </summary>
    ServerError: 0,

    /// <summary>
    /// The resource already exists.
    /// </summary>
    AlreadyExists: 1,

    /// <summary>
    /// An invalid set of inputs was provided.
    /// </summary>
    InvalidInput: 2,

    /// <summary>
    /// A dependent service has failed.
    /// </summary>
    DownstreamServiceError: 3,

    /// <summary>
    /// The given subscription could not be found.
    /// </summary>
    SubscriptionNotFound: 4,

    /// <summary>
    /// The subscription is expired.
    /// </summary>
    SubscriptionExpired: 5,

    /// <summary>
    /// The subscription could not be updated.
    /// </summary>
    SubscriptionUpdateFailure: 6,

    /// <summary>
    /// The requested partner offer was not found.
    /// </summary>
    PartnerOfferNotFound: 7,

    /// <summary>
    /// Failure in accessing persistence.
    /// </summary>
    PersistenceFailure: 8,

    /// <summary>
    /// Unexpected file type.
    /// </summary>
    InvalidFileType: 9,

    /// <summary>
    /// Failure in payment gateway.
    /// </summary>
    PaymentGatewayFailure: 10,

    /// <summary>
    /// A failure due to an attempt to update the Microsoft offer associated with a partner offer.
    /// </summary>
    MicrosoftOfferImmutable: 11,

    /// <summary>
    /// Maximum request size exceeded error.
    /// </summary>
    MaximumRequestSizeExceeded: 12,

    /// <summary>
    /// Invalid address.
    /// </summary>
    InvalidAddress: 13,

    /// <summary>
    /// Domain not available.
    /// </summary>
    DomainNotAvailable: 14,

    /// <summary>
    /// Purchasing a deleted offer is not allowed.
    /// </summary>
    PurchaseDeletedOfferNotAllowed: 15,

    /// <summary>
    /// Failure in payment gateway authentication during configuration.
    /// </summary>
    PaymentGatewayIdentityFailureDuringConfiguration: 16,

    /// <summary>
    /// Failure in payment gateway authentication during payment.
    /// </summary>
    PaymentGatewayIdentityFailureDuringPayment: 17,

    /// <summary>
    /// Failure in payment gateway during payment.
    /// </summary>
    PaymentGatewayPaymentError: 18
};

//@ sourceURL=ErrorCode.js