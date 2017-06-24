// -----------------------------------------------------------------------
// <copyright file="PartnerSubscriptionModel.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.CustomerPortal.Models
{
    /// <summary>
    /// The partner subscription model
    /// </summary>
    public class PartnerSubscriptionModel
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
    }
}