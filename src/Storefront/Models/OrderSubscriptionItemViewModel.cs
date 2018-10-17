// -----------------------------------------------------------------------
// <copyright file="OrderSubscriptionItemViewModel.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Storefront.Models
{
    using System;

    /// <summary>
    /// The order view model.
    /// </summary>
    public class OrderSubscriptionItemViewModel
    {
        /// <summary>
        /// Gets or sets the offer Id tied to the order.
        /// </summary>        
        public string OfferId { get; set; }

        /// <summary>
        /// Gets or sets the subscription Id. 
        /// </summary>
        public string SubscriptionId { get; set; }

        /// <summary>
        /// Gets or sets the partner offer Id. 
        /// </summary>
        public string PartnerOfferId { get; set; }

        /// <summary>
        /// Gets or sets the subscription name of the offer being ordered. 
        /// </summary>
        public string SubscriptionName { get; set; }

        /// <summary>
        /// Gets or sets the subscription expiry date of the offer being ordered. 
        /// </summary>
        public DateTime SubscriptionExpiryDate { get; set; }

        /// <summary>
        /// Gets or sets the quantity of the offer being ordered.
        /// </summary>                
        public int Quantity { get; set; }

        /// <summary>
        /// Gets or sets the price of the offer being ordered.
        /// </summary>
        public decimal SeatPrice { get; set; }
    }
}