// -----------------------------------------------------------------------
// <copyright file="PurchaseLineItemWithOffer.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Storefront.Models
{
    /// <summary>
    /// Associates a purchase line item with a partner offer.
    /// </summary>
    public class PurchaseLineItemWithOffer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PurchaseLineItemWithOffer"/> class.
        /// </summary>
        public PurchaseLineItemWithOffer()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PurchaseLineItemWithOffer"/> class.
        /// </summary>
        /// <param name="purchaseLineItem">The purchase line item.</param>
        /// <param name="partnerOffer">The partner offer.</param>
        public PurchaseLineItemWithOffer(PurchaseLineItem purchaseLineItem, PartnerOffer partnerOffer)
        {
            purchaseLineItem.AssertNotNull(nameof(purchaseLineItem));
            partnerOffer.AssertNotNull(nameof(partnerOffer));

            this.PurchaseLineItem = purchaseLineItem;
            this.PartnerOffer = partnerOffer;
        }

        /// <summary>
        /// Gets the purchase line item.
        /// </summary>
        public PurchaseLineItem PurchaseLineItem { get; private set; }

        /// <summary>
        /// Gets the partner offer.
        /// </summary>
        public PartnerOffer PartnerOffer { get; private set; }
    }
}