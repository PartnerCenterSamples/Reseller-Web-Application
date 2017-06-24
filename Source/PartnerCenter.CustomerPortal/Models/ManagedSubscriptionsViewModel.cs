// -----------------------------------------------------------------------
// <copyright file="ManagedSubscriptionsViewModel.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.CustomerPortal.Models
{
    using System.Collections.Generic;

    /// <summary>
    /// The managed subscriptions view model.
    /// </summary>
    public class ManagedSubscriptionsViewModel
    {
        /// <summary>
        /// Gets or sets the customers managed subscriptions
        /// </summary>
        public IEnumerable<CustomerSubscriptionModel> CustomerManagedSubscriptions { get; set; }

        /// <summary>
        /// Gets or sets the partner managed subscriptions
        /// </summary>
        public IEnumerable<PartnerSubscriptionModel> PartnerManagedSubscriptions { get; set; }
    }
}