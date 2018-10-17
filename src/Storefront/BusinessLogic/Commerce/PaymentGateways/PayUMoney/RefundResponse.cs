// -----------------------------------------------------------------------
// <copyright file="RefundResponse.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Storefront.BusinessLogic.Commerce.PaymentGateways.PayUMoney
{
    /// <summary>
    /// Refund response class
    /// </summary>
    public class RefundResponse
    {
        /// <summary>
        /// Gets or sets error code. Always Null from PayUMoney documentation.
        /// </summary>
        public string ErrorCode { get; set; }

        /// <summary>
        /// Gets or sets Guid.
        /// </summary>
        public string Guid { get; set; }

        /// <summary>
        /// Gets or sets message.
        /// Message string for both success and failure cases
        /// Refund Initiated : Refund successfully Initiated
        /// PaymentId is not valid for this merchant : When PaymentID is not linked with the merchantID passed
        /// Payment is not allowed for refund as status is: refunding progress : Refund on this sub order is already initiated
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets result.
        /// if Success then it has RefundId.
        /// if Failure then it will be NULL.
        /// </summary>
        public string Result { get; set; }

        /// <summary>
        /// Gets or sets rows.
        /// </summary>
        public string Rows { get; set; }

        /// <summary>
        /// Gets or sets session ID.
        /// </summary>
        public string SessionId { get; set; }

        /// <summary>
        /// Gets or sets status. 
        /// Status will be 0 if API call is a success, Status will be -1 in case of failure you'll get system handled failure reasons in this case.
        /// </summary>
        public int Status { get; set; }
    }
}