// -----------------------------------------------------------------------
// <copyright file="TransactionStatusResponse.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Storefront.BusinessLogic.Commerce.PaymentGateways.PayUMoney
{
    using System.Collections.Generic;

    /// <summary>
    /// Transaction status response class.
    /// </summary>
    public class TransactionStatusResponse
    {
        /// <summary>
        /// Gets or sets status.
        /// Status will be 0 if API call is a success, Status will be -1 in case of failure you'll get system handled failure reasons in this case.
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// Gets or sets message.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets transaction result.
        /// </summary>
        public List<TransactionResult> Result { get; set; }

        /// <summary>
        /// Gets or sets error code. Always Null from PayUMoney documentation.
        /// </summary>
        public object ErrorCode { get; set; }

        /// <summary>
        /// Gets or sets response code.
        /// </summary>
        public object ResponseCode { get; set; }
    }
}