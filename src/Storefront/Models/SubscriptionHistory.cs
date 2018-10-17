// -----------------------------------------------------------------------
// <copyright file="SubscriptionHistory.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Storefront.Models
{
    /// <summary>
    /// The subscription history view model.
    /// </summary>
    public class SubscriptionHistory
    {
        /// <summary>
        /// Gets or sets number of seats bought for the subscription.
        /// </summary>
        public string SeatsBought { get; set; }

        /// <summary>
        /// Gets or sets the price at which the subscription was ordered.
        /// </summary>
        public string PricePerSeat { get; set; }

        /// <summary>
        /// Gets or sets the price at which the subscription was ordered.
        /// </summary>
        public string OrderTotal { get; set; }

        /// <summary>
        /// Gets or sets the transaction date.
        /// </summary>
        public string OrderDate { get; set; }

        /// <summary>
        /// Gets or sets the operation type (NewPurchase, AdditionalSeatsPurchase, Renewal) 
        /// </summary>
        public string OperationType { get; set; }
    }
}