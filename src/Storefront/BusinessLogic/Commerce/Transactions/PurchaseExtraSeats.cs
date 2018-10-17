// -----------------------------------------------------------------------
// <copyright file="PurchaseExtraSeats.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Storefront.BusinessLogic.Commerce.Transactions
{
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using Exceptions;
    using Infrastructure;
    using PartnerCenter.Exceptions;
    using PartnerCenter.Models.Subscriptions;
    using Subscriptions;

    /// <summary>
    /// Purchases additional seats for a subscription.
    /// </summary>
    public class PurchaseExtraSeats : IBusinessTransactionWithOutput<Subscription>
    {
        /// <summary>
        /// The subscription's seat count before the update.
        /// </summary>
        private int originalSeatCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="PurchaseExtraSeats"/> class.
        /// </summary>
        /// <param name="subscriptionOperations">A Partner Center subscription operations instance.</param>
        /// <param name="seatsToPurchase">The number of seats to purchase.</param>
        public PurchaseExtraSeats(ISubscription subscriptionOperations, int seatsToPurchase)
        {
            subscriptionOperations.AssertNotNull(nameof(subscriptionOperations));
            seatsToPurchase.AssertPositive(nameof(seatsToPurchase));

            SubscriptionOperations = subscriptionOperations;
            SeatsToPurchase = seatsToPurchase;
        }

        /// <summary>
        /// Gets the subscription operations used to manipulate the subscription.
        /// </summary>
        public ISubscription SubscriptionOperations { get; private set; }

        /// <summary>
        /// Gets the number of seats to purchase.
        /// </summary>
        public int SeatsToPurchase { get; private set; }

        /// <summary>
        /// Gets the updated subscription.
        /// </summary>
        public Subscription Result { get; private set; }

        /// <summary>
        /// Purchases additional seats for the subscription.
        /// </summary>
        /// <returns>A task.</returns>
        public async Task ExecuteAsync()
        {
            try
            {
                Subscription partnerCenterSubscription = await SubscriptionOperations.GetAsync().ConfigureAwait(false);

                originalSeatCount = partnerCenterSubscription.Quantity;
                partnerCenterSubscription.Quantity += SeatsToPurchase;

                Result = await SubscriptionOperations.PatchAsync(partnerCenterSubscription).ConfigureAwait(false);
            }
            catch (PartnerException subscriptionUpdateProblem)
            {
                if (subscriptionUpdateProblem.ErrorCategory == PartnerErrorCategory.NotFound)
                {
                    throw new PartnerDomainException(ErrorCode.SubscriptionNotFound, "PurchaseExtraSeats.ExecuteAsync() Failed", subscriptionUpdateProblem);
                }
                else
                {
                    throw new PartnerDomainException(ErrorCode.SubscriptionUpdateFailure, "PurchaseExtraSeats.ExecuteAsync() Failed", subscriptionUpdateProblem);
                }
            }
        }

        /// <summary>
        /// Reverts the seat change.
        /// </summary>
        /// <returns>A task.</returns>
        public async Task RollbackAsync()
        {
            if (Result != null)
            {
                try
                {
                    // restore the original seat count for the subscription
                    Result.Quantity = originalSeatCount;
                    await SubscriptionOperations.PatchAsync(Result).ConfigureAwait(false);
                }
                catch (Exception subscriptionUpdateProblem)
                {
                    if (subscriptionUpdateProblem.IsFatal())
                    {
                        throw;
                    }

                    Trace.TraceError("PurchaseExtraSeats.RollbackAsync failed: {0}, ID: {1}, Quantity: {2}.", subscriptionUpdateProblem, this.Result.Id, this.Result.Quantity);

                    // TODO: Notify the system integrity recovery component
                }
            }

            this.Result = null;
        }
    }
}