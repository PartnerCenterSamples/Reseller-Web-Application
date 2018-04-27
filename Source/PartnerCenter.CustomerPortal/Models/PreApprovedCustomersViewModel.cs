// -----------------------------------------------------------------------
// <copyright file="PreApprovedCustomersViewModel.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.CustomerPortal.Models
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents PreApproved Customers view model.
    /// </summary>
    public class PreApprovedCustomersViewModel
    {
        /// <summary>
        /// Gets or sets a list of customers managed in the portal for pre approval. 
        /// </summary>
        public IEnumerable<PortalCustomer> Items { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether All customers in the portal are preapproved. 
        /// </summary>
        public bool IsEveryCustomerPreApproved { get; set; }

        /// <summary>
        /// Gets or sets a list of Customer Ids which need to be preapproved. 
        /// </summary>
        public List<string> CustomerIds { get; set; }
    }
}