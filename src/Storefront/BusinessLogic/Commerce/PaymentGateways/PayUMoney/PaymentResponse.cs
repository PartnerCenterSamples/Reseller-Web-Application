// -----------------------------------------------------------------------
// <copyright file="PaymentResponse.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Storefront.BusinessLogic.Commerce.PaymentGateways.PayUMoney
{
    using System.Collections.Generic;

    /// <summary>
    /// PayUMoneyPaymentResponse class
    /// </summary>
    public class PaymentResponse
    {
        /// <summary>
        /// Gets or sets error code
        /// </summary>
        public string ErrorCode { get; set; }

        /// <summary>
        /// Gets or sets message code
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets response code
        /// </summary>
        public string ResponseCode { get; set; }

        /// <summary>
        /// Gets or sets result
        /// </summary>
        public List<PaymentResponseResult> Result { get; set; }

        /// <summary>
        /// Gets or sets status
        /// </summary>
        public string Status { get; set; }
    }
}