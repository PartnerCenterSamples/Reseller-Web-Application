// -----------------------------------------------------------------------
// <copyright file="CustomerLicensesModel.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Storefront.Models
{
    /// <summary>
    /// The customer licenses view model.
    /// </summary>
    public class CustomerLicensesModel
    {
        /// <summary>
        /// Gets or sets the customer license Id.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the offer name.
        /// </summary>
        public string OfferName { get; set; }

        /// <summary>
        /// Gets or sets the customer license status like None, Active, Suspended or Deleted
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets the total number of licenses for this customer license.
        /// </summary>
        public string Quantity { get; set; }

        /// <summary>
        /// Gets or sets the license creation date. 
        /// </summary>
        public string CreationDate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether subscription is renewable. 
        /// </summary>
        public bool IsRenewable { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether subscription is editable. 
        /// </summary>
        public bool IsEditable { get; set; }

        /// <summary>
        /// Gets or sets the subscription's portal offer Id.
        /// </summary>
        public string PortalOfferId { get; set; }

        /// <summary>
        /// Gets or sets the remaining days to expiry for this subscription.
        /// </summary>
        public decimal SubscriptionProRatedPrice { get; set; }
    }
}