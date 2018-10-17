// -----------------------------------------------------------------------
// <copyright file="PlaceOrder.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Storefront.BusinessLogic.Commerce.Transactions
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Exceptions;
    using Infrastructure;
    using PartnerCenter.Customers;
    using PartnerCenter.Exceptions;
    using PartnerCenter.Models.Orders;
    using PartnerCenter.Models.Subscriptions;

    /// <summary>
    /// A transaction that places an order with partner center and knows how to roll it back.
    /// </summary>
    public class PlaceOrder : IBusinessTransactionWithOutput<Order>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PlaceOrder"/> class.
        /// </summary>
        /// <param name="customerOperations">A customer operations used to place the order.</param>
        /// <param name="orderToPlace">A preconfigured order to place with Partner Center.</param>
        public PlaceOrder(ICustomer customerOperations, Order orderToPlace)
        {
            customerOperations.AssertNotNull(nameof(customerOperations));
            orderToPlace.AssertNotNull(nameof(orderToPlace));

            Customer = customerOperations;
            Order = orderToPlace;
        }

        /// <summary>
        /// Gets the customer operations used to place the order.
        /// </summary>
        public ICustomer Customer { get; private set; }

        /// <summary>
        /// Gets the order to place.
        /// </summary>
        public Order Order { get; private set; }

        /// <summary>
        /// Gets the resulting order from the transaction.
        /// </summary>
        public Order Result { get; private set; }

        /// <summary>
        /// Places the order with Partner Center.
        /// </summary>
        /// <returns>A task.</returns>
        public async Task ExecuteAsync()
        {
            try
            {
                // place the order
                Result = await Customer.Orders.CreateAsync(Order).ConfigureAwait(false);
            }
            catch (PartnerException orderPlacingProblem)
            {
                switch (orderPlacingProblem.ErrorCategory)
                {
                    case PartnerErrorCategory.BadInput:
                        throw new PartnerDomainException(ErrorCode.InvalidInput, "PlaceOrder.ExecuteAsync() Failed", orderPlacingProblem);
                    case PartnerErrorCategory.AlreadyExists:
                        throw new PartnerDomainException(ErrorCode.AlreadyExists, "PlaceOrder.ExecuteAsync() Failed", orderPlacingProblem);
                    default:
                        throw new PartnerDomainException(ErrorCode.DownstreamServiceError, "PlaceOrder.ExecuteAsync() Failed", orderPlacingProblem);
                }
            }
        }

        /// <summary>
        /// Rolls back the order that was placed.
        /// </summary>
        /// <returns>A task.</returns>
        public async Task RollbackAsync()
        {
            if (Result != null)
            {
                // suspend all subscriptions that resulted from placing the order
                IEnumerable<Task> suspensionTasks = Result.LineItems.Select<OrderLineItem, Task>(orderLineItem => new TaskFactory().StartNew(async () =>
                {
                    try
                    {
                        Subscriptions.ISubscription subscriptionOperations = Customer.Subscriptions.ById(orderLineItem.SubscriptionId);
                        Subscription subscriptionToSuspend = await subscriptionOperations.GetAsync().ConfigureAwait(false);

                        subscriptionToSuspend.Status = SubscriptionStatus.Suspended;
                        subscriptionToSuspend.FriendlyName = subscriptionToSuspend.FriendlyName.Replace(Resources.UnpaidSubscriptionSuffix, string.Empty) + Resources.UnpaidSubscriptionSuffix;

                        Subscription patchedSubscription = await subscriptionOperations.PatchAsync(subscriptionToSuspend).ConfigureAwait(false);

                        Trace.TraceInformation("Suspended subscription: {0}", orderLineItem.SubscriptionId);
                    }
                    catch (Exception suspensionProblem)
                    {
                        if (suspensionProblem.IsFatal())
                        {
                            throw;
                        }

                        Trace.TraceError("PlaceOrder.RollbackAsync: failed to suspend a subscription: {0}, ID: {1}", suspensionProblem, orderLineItem.SubscriptionId);

                        // TODO: Notify the system integrity recovery component
                    }
                }, CancellationToken.None, TaskCreationOptions.DenyChildAttach, TaskScheduler.FromCurrentSynchronizationContext()));

                try
                {
                    await Task.WhenAll(suspensionTasks).ConfigureAwait(false);
                }
                catch (Exception exception)
                {
                    if (exception.IsFatal())
                    {
                        throw;
                    }

                    Trace.TraceError("PlaceOrder.RollbackAsync: awaiting all suspension tasks failed: {0}", exception);
                }

                Result = null;
            }
        }
    }
}