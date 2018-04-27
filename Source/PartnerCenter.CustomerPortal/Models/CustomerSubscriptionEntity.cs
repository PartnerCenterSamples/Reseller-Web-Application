// -----------------------------------------------------------------------
// <copyright file="CustomerSubscriptionEntity.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.CustomerPortal.Models
{
    using System;

    /// <summary>
    /// Represents a unique subscription purchased by a customer.
    /// </summary>
    public class CustomerSubscriptionEntity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CustomerSubscriptionEntity"/> class.
        /// </summary>
        /// <param name="customerId">The ID of the customer who owns the subscription.</param>
        /// <param name="subscriptionId">The subscription ID.</param>
        /// <param name="partnerOfferId">The partner offer ID associated with the subscription.</param>
        /// <param name="expiryDate">The subscription's expiry date.</param>
        public CustomerSubscriptionEntity(string customerId, string subscriptionId, string partnerOfferId, DateTime expiryDate)
        {
            customerId.AssertNotEmpty(nameof(customerId));
            subscriptionId.AssertNotEmpty(nameof(subscriptionId));
            partnerOfferId.AssertNotEmpty(nameof(partnerOfferId));

            this.CustomerId = customerId;
            this.SubscriptionId = subscriptionId;
            this.PartnerOfferId = partnerOfferId;
            this.ExpiryDate = expiryDate;
        }

        /// <summary>
        /// Gets the customer ID who owns the subscription.
        /// </summary>
        public string CustomerId { get; private set; }

        /// <summary>
        /// Gets the subscription ID.
        /// </summary>
        public string SubscriptionId { get; private set; }

        /// <summary>
        /// Gets the partner offer ID associated with the subscription.
        /// </summary>
        public string PartnerOfferId { get; private set; }

        /// <summary>
        /// Gets the subscription's expiry date.
        /// </summary>
        public DateTime ExpiryDate { get; private set; }
    }
}