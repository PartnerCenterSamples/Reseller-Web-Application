// -----------------------------------------------------------------------
// <copyright file="CustomerSubscriptionModel.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.CustomerPortal.Models
{
    /// <summary>
    /// The customer subscription model
    /// </summary>
    public class CustomerSubscriptionModel
    {
        /// <summary>
        /// Gets or sets the customer license Id.
        /// </summary>
        public string SubscriptionId { get; set; }

        /// <summary>
        /// Gets or sets the offer name.
        /// </summary>
        public string FriendlyName { get; set; }

        /// <summary>
        /// Gets or sets the customer license status like None, Active, Suspended or Deleted
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets the total number of licenses for this customer license.
        /// </summary>
        public string LicensesTotal { get; set; }

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