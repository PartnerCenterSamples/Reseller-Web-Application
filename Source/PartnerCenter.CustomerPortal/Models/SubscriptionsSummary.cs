// -----------------------------------------------------------------------
// <copyright file="SubscriptionsSummary.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.CustomerPortal.Models
{
    using System.Collections.Generic;

    /// <summary>
    /// The subscription summary view model.
    /// </summary>
    public class SubscriptionsSummary
    {
        /// <summary>
        /// Gets or sets the subscription's in this summary.
        /// </summary>
        public IEnumerable<SubscriptionViewModel> Subscriptions { get; set; }

        /// <summary>
        /// Gets or sets the total amount for this subscription summary.
        /// </summary>
        public string SummaryTotal { get; set; }

        /// <summary>
        /// Gets or sets Customer view model
        /// </summary>
        public CustomerViewModel CustomerViewModel { get; set; }
    }
}
