// -----------------------------------------------------------------------
// <copyright file="RenewSubscription.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Storefront.BusinessLogic.Commerce.Transactions
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Threading.Tasks;
    using Exceptions;
    using Infrastructure;
    using PartnerCenter.Exceptions;
    using PartnerCenter.Models.Subscriptions;
    using Subscriptions;

    /// <summary>
    /// Renews a partner center subscription.
    /// </summary>
    public class RenewSubscription : IBusinessTransactionWithOutput<Subscription>
    {
        /// <summary>
        /// The existing subscription.
        /// </summary>
        private Subscription existingSubscription;

        /// <summary>
        /// Initializes a new instance of the <see cref="RenewSubscription"/> class.
        /// </summary>
        /// <param name="subscriptionOperations">A Partner Center subscription operations instance.</param>
        /// <param name="existingSubscription">An existing subscription to update.</param>
        public RenewSubscription(ISubscription subscriptionOperations, Subscription existingSubscription)
        {
            subscriptionOperations.AssertNotNull(nameof(subscriptionOperations));
            existingSubscription.AssertNotNull(nameof(existingSubscription));

            this.SubscriptionOperations = subscriptionOperations;
            this.existingSubscription = existingSubscription;
        }

        /// <summary>
        /// Gets the subscription operations used to manipulate the subscription.
        /// </summary>
        public ISubscription SubscriptionOperations { get; private set; }

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
                // activate the subscription (in case it was suspended)
                Result = await SubscriptionOperations.PatchAsync(new Subscription()
                {
                    Status = SubscriptionStatus.Active
                }).ConfigureAwait(false);
            }
            catch (PartnerException subscriptionUpdateProblem)
            {
                string exceptionMessage = string.Format(
                    CultureInfo.InvariantCulture,
                    Resources.RenewSubscriptionFailedMessage,
                    subscriptionUpdateProblem,
                    existingSubscription.Id);

                if (subscriptionUpdateProblem.ErrorCategory == PartnerErrorCategory.NotFound)
                {
                    throw new PartnerDomainException(ErrorCode.SubscriptionNotFound, exceptionMessage, subscriptionUpdateProblem);
                }
                else
                {
                    throw new PartnerDomainException(ErrorCode.SubscriptionUpdateFailure, exceptionMessage, subscriptionUpdateProblem);
                }
            }
        }

        /// <summary>
        /// Reverts the subscription renewal.
        /// </summary>
        /// <returns>A task.</returns>
        public async Task RollbackAsync()
        {
            if (Result != null)
            {
                try
                {
                    // restore the original subscription state
                    await SubscriptionOperations.PatchAsync(existingSubscription).ConfigureAwait(false);
                }
                catch (Exception rollbackProblem)
                {
                    if (rollbackProblem.IsFatal())
                    {
                        throw;
                    }

                    Trace.TraceError(
                        "RenewSubscription.RollbackAsync failed: {0}, Customer ID: {1}, Subscription ID: {2}, Subscription: {3}",
                        rollbackProblem,
                        SubscriptionOperations.Context.Item1,
                        SubscriptionOperations.Context.Item2,
                        existingSubscription);

                    // TODO: Notify the system integrity recovery component
                }
            }

            Result = null;
        }
    }
}