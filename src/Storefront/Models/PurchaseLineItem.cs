// -----------------------------------------------------------------------
// <copyright file="PurchaseLineItem.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Storefront.Models
{
    /// <summary>
    /// Holds the information for a new purchase line item.
    /// </summary>
    public class PurchaseLineItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PurchaseLineItem"/> class.
        /// </summary>
        /// <param name="partnerOfferId">The partner's offer ID to purchase.</param>
        /// <param name="quantity">The quantity to purchase.</param>
        public PurchaseLineItem(string partnerOfferId, int quantity)
        {
            partnerOfferId.AssertNotEmpty(nameof(partnerOfferId));
            quantity.AssertPositive(nameof(quantity));

            this.PartnerOfferId = partnerOfferId;
            this.Quantity = quantity;
        }

        /// <summary>
        /// Gets the partner's offer ID to purchase.
        /// </summary>
        public string PartnerOfferId { get; private set; }

        /// <summary>
        /// Gets the quantity to purchase.
        /// </summary>
        public int Quantity { get; private set; }
    }
}