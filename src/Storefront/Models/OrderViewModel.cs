// -----------------------------------------------------------------------
// <copyright file="OrderViewModel.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Storefront.Models
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// The Order view model (orders contain subscriptions, CreditCard and optionally Customer Id).
    /// </summary>
    public class OrderViewModel
    {
        /// <summary>
        /// Gets or sets the Order Id. 
        /// </summary>
        public string OrderId { get; set; }

        /// <summary>
        /// Gets or sets the Customer Id. 
        /// </summary>
        public string CustomerId { get; set; }

        /// <summary>
        /// Gets or sets the subscriptions the customer ordered.
        /// </summary>
        [Required]
        public IEnumerable<OrderSubscriptionItemViewModel> Subscriptions { get; set; }

        /// <summary>
        /// Gets or sets the Operation Type for the order. 
        /// </summary>
        [Required]
        public CommerceOperationType OperationType { get; set; }
    }
}