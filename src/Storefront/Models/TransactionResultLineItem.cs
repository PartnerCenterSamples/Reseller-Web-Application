// -----------------------------------------------------------------------
// <copyright file="TransactionResultLineItem.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Storefront.Models
{
    /// <summary>
    /// Represents a single transaction result line item which corresponds to a subscription being transacted on.
    /// </summary>
    public class TransactionResultLineItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionResultLineItem"/> class.
        /// </summary>
        /// <param name="subscriptionId">The subscription ID related to the transaction line item.</param>
        /// <param name="partnerOfferId">The partner offer ID related to the transaction line item.</param>
        /// <param name="quantity">The quantity purchased.</param>
        /// <param name="seatPrice">The price charged per seat.</param>
        /// <param name="amountCharged">The total amount charged.</param>
        public TransactionResultLineItem(string subscriptionId, string partnerOfferId, int quantity, decimal seatPrice, decimal amountCharged)
        {
            // we don't validate amount charged since a transaction may result in a negative amount
            subscriptionId.AssertNotEmpty(nameof(subscriptionId));
            partnerOfferId.AssertNotEmpty(nameof(partnerOfferId));
            quantity.AssertPositive(nameof(quantity));
            seatPrice.AssertPositive(nameof(seatPrice));

            this.SubscriptionId = subscriptionId;
            this.PartnerOfferId = partnerOfferId;
            this.Quantity = quantity;
            this.SeatPrice = seatPrice;
            this.AmountCharged = amountCharged;
        }

        /// <summary>
        /// Gets the subscription ID related to the transaction line item.
        /// </summary>
        public string SubscriptionId { get; private set; }

        /// <summary>
        /// Gets the partner offer ID related to the transaction line item.
        /// </summary>
        public string PartnerOfferId { get; private set; }

        /// <summary>
        /// Gets the quantity purchased.
        /// </summary>
        public int Quantity { get; private set; }

        /// <summary>
        /// Gets the seat price that applied to this transaction.
        /// </summary>
        public decimal SeatPrice { get; private set; }

        /// <summary>
        /// Gets the amount charged for the line item.
        /// </summary>
        public decimal AmountCharged { get; private set; }
    }
}