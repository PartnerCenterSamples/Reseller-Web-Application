// -----------------------------------------------------------------------
// <copyright file="UpdatePersistedSubscription.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Storefront.BusinessLogic.Commerce.Transactions
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using Exceptions;
    using Infrastructure;
    using Models;

    /// <summary>
    /// Updates a subscription in persistence.
    /// </summary>
    public class UpdatePersistedSubscription : IBusinessTransactionWithOutput<CustomerSubscriptionEntity>
    {
        /// <summary>
        /// The customer subscriptions repository used for accessing persistence.
        /// </summary>
        private readonly CustomerSubscriptionsRepository repository;

        /// <summary>
        /// The required updates to the subscription.
        /// </summary>
        private readonly CustomerSubscriptionEntity desiredSubscriptionUpdates;

        /// <summary>
        /// The subscription state before our update.
        /// </summary>
        private CustomerSubscriptionEntity originalSubscriptionState;

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdatePersistedSubscription"/> class.
        /// </summary>
        /// <param name="repository">The payment gateway to use for authorization.</param>
        /// <param name="updatedSubscriptionInformation">The updates to apply to the subscription in persistence.</param>
        public UpdatePersistedSubscription(CustomerSubscriptionsRepository repository, CustomerSubscriptionEntity updatedSubscriptionInformation)
        {
            repository.AssertNotNull(nameof(repository));
            updatedSubscriptionInformation.AssertNotNull(nameof(updatedSubscriptionInformation));

            this.repository = repository;
            desiredSubscriptionUpdates = updatedSubscriptionInformation;
        }

        /// <summary>
        /// Gets the updated subscription entity.
        /// </summary>
        public CustomerSubscriptionEntity Result { get; private set; }

        /// <summary>
        /// Authorizes the payment amount.
        /// </summary>
        /// <returns>A task.</returns>
        public async Task ExecuteAsync()
        {
            // retrieve the subscription
            System.Collections.Generic.IEnumerable<CustomerSubscriptionEntity> customerSubscriptions = await repository.RetrieveAsync(desiredSubscriptionUpdates.CustomerId).ConfigureAwait(false);
            originalSubscriptionState = customerSubscriptions.FirstOrDefault(subscription => subscription.SubscriptionId == desiredSubscriptionUpdates.SubscriptionId);

            if (originalSubscriptionState == null)
            {
                throw new PartnerDomainException(ErrorCode.SubscriptionNotFound);
            }

            // update the subscription
            Result = await repository.UpdateAsync(desiredSubscriptionUpdates).ConfigureAwait(false);
        }

        /// <summary>
        /// Rolls back the authorization.
        /// </summary>
        /// <returns>A task.</returns>
        public async Task RollbackAsync()
        {
            if (Result != null)
            {
                try
                {
                    // restore the subscription to what it was before
                    await repository.UpdateAsync(originalSubscriptionState).ConfigureAwait(false);
                }
                catch (Exception restoreProblem)
                {
                    if (restoreProblem.IsFatal())
                    {
                        throw;
                    }

                    Trace.TraceError(
                        "UpdatePersistedSubscription.RollbackAsync failed: {0}, Customer ID: {1}, ExpiryDate: {2}, PartnerOfferId: {3}, SubscriptionId: {4}",
                        restoreProblem,
                        originalSubscriptionState.CustomerId,
                        originalSubscriptionState.ExpiryDate,
                        originalSubscriptionState.PartnerOfferId,
                        originalSubscriptionState.SubscriptionId);

                    // TODO: Notify the system integrity recovery component
                }
            }

            Result = null;
        }
    }
}