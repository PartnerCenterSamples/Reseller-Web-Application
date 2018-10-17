// -----------------------------------------------------------------------
// <copyright file="CustomerPurchaseEntity.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Storefront.Models
{
    using System;

    /// <summary>
    /// Represents a purchase performed by a customer.
    /// </summary>
    public class CustomerPurchaseEntity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CustomerPurchaseEntity"/> class.
        /// </summary>
        /// <param name="purchaseType">The purchase type.</param>
        /// <param name="id">Purchase ID.</param>
        /// <param name="customerId">The ID  of the customer who made the purchase.</param>
        /// <param name="subscriptionId">The subscription ID which the purchase applied to.</param>
        /// <param name="seatsBought">The number of seats bought in the purchase.</param>
        /// <param name="seatPrice">The seat price charged for the purchase.</param>
        /// <param name="transactionDate">The transaction date of the purchase.</param>
        public CustomerPurchaseEntity(CommerceOperationType purchaseType, string id, string customerId, string subscriptionId, int seatsBought, decimal seatPrice, DateTime transactionDate)
        {
            id.AssertNotEmpty(nameof(id));
            customerId.AssertNotEmpty(nameof(customerId));
            subscriptionId.AssertNotEmpty(nameof(subscriptionId));
            seatsBought.AssertPositive(nameof(seatsBought));
            seatPrice.AssertPositive(nameof(seatPrice));

            this.PurchaseType = purchaseType;
            this.Id = id;
            this.CustomerId = customerId;
            this.SubscriptionId = subscriptionId;
            this.SeatsBought = seatsBought;
            this.SeatPrice = seatPrice;
            this.TransactionDate = transactionDate;
        }

        /// <summary>
        /// Gets the commerce purchase type for this purchase item.
        /// </summary>
        public CommerceOperationType PurchaseType { get; private set; }

        /// <summary>
        /// Gets the unique purchase ID.
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// Gets the ID  of the customer who made the purchase.
        /// </summary>
        public string CustomerId { get; private set; }

        /// <summary>
        /// Gets the subscription ID which the purchase applied to.
        /// </summary>
        public string SubscriptionId { get; private set; }

        /// <summary>
        /// Gets the seat price charged for the purchase.
        /// </summary>
        public decimal SeatPrice { get; private set; }

        /// <summary>
        /// Gets the number of seats bought in the purchase.
        /// </summary>
        public int SeatsBought { get; private set; }

        /// <summary>
        /// Gets the transaction date of the purchase.
        /// </summary>
        public DateTime TransactionDate { get; private set; }
    }
}