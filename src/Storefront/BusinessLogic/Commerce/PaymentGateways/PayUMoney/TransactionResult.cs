// -----------------------------------------------------------------------
// <copyright file="TransactionResult.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Storefront.BusinessLogic.Commerce.PaymentGateways.PayUMoney
{
    /// <summary>
    /// Transaction result class.
    /// </summary>
    public class TransactionResult
    {
        /// <summary>
        /// Gets or sets merchant transaction ID.
        /// </summary>
        public string MerchantTransactionId { get; set; }

        /// <summary>
        /// Gets or sets payment ID.
        /// </summary>
        public int PaymentId { get; set; }

        /// <summary>
        /// Gets or sets status.
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets amount.
        /// </summary>
        public double Amount { get; set; }
    }
}