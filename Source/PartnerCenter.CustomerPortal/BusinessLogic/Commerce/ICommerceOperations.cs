// -----------------------------------------------------------------------
// <copyright file="ICommerceOperations.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.CustomerPortal.BusinessLogic.Commerce
{
    using System.Threading.Tasks;
    using Models;

    /// <summary>
    /// A contract for components implementing commerce operations.
    /// </summary>
    public interface ICommerceOperations
    {
        /// <summary>
        /// Gets the customer ID who owns the transaction.
        /// </summary>
        string CustomerId { get; }

        /// <summary>
        /// Gets the payment gateway used to process payments.
        /// </summary>
        IPaymentGateway PaymentGateway { get; }

        /// <summary>
        /// Purchases one or more partner offers.
        /// </summary>
        /// <param name="order">The order to execute.</param>
        /// <returns>A transaction result which summarizes its outcome.</returns>
        Task<TransactionResult> PurchaseAsync(OrderViewModel order);

        /// <summary>
        /// Purchases additional seats for an existing subscription the customer has already bought.
        /// </summary>
        /// <param name="order">The order to execute.</param>
        /// <returns>A transaction result which summarizes its outcome.</returns>
        Task<TransactionResult> PurchaseAdditionalSeatsAsync(OrderViewModel order);

        /// <summary>
        /// Renews an existing subscription for a customer.
        /// </summary>
        /// <param name="order">The order to execute.</param>
        /// <returns>A transaction result which summarizes its outcome.</returns>
        Task<TransactionResult> RenewSubscriptionAsync(OrderViewModel order);
    }
}