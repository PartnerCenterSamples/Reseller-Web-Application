// -----------------------------------------------------------------------
// <copyright file="RecordNewCustomerSubscription.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.CustomerPortal.BusinessLogic.Commerce.Transactions
{
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using Infrastructure;
    using Models;

    /// <summary>
    /// Records a new subscription which a custom purchased.
    /// </summary>
    public class RecordNewCustomerSubscription : IBusinessTransactionWithOutput<CustomerSubscriptionEntity>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RecordNewCustomerSubscription"/> class.
        /// </summary>
        /// <param name="repository">A customer subscriptions repository which manages customer subscriptions persistence.</param>
        /// <param name="newSubscription">The new customer subscription to record.</param>
        public RecordNewCustomerSubscription(CustomerSubscriptionsRepository repository, CustomerSubscriptionEntity newSubscription)
        {
            repository.AssertNotNull(nameof(repository));
            newSubscription.AssertNotNull(nameof(newSubscription));

            this.CustomerSubscriptionsRepository = repository;
            this.CustomerSubscriptionToPersist = newSubscription;
        }

        /// <summary>
        /// Gets the customer subscription repository used to persist the subscription.
        /// </summary>
        public CustomerSubscriptionsRepository CustomerSubscriptionsRepository { get; private set; }

        /// <summary>
        /// Gets the customer subscription entity to persist.
        /// </summary>
        public CustomerSubscriptionEntity CustomerSubscriptionToPersist { get; private set; }

        /// <summary>
        /// Gets the resulting customer subscription entity.
        /// </summary>
        public CustomerSubscriptionEntity Result { get; private set; }

        /// <summary>
        /// Persists the subscription.
        /// </summary>
        /// <returns>A task.</returns>
        public async Task ExecuteAsync()
        {
            this.Result = await this.CustomerSubscriptionsRepository.AddAsync(this.CustomerSubscriptionToPersist);
        }

        /// <summary>
        /// Removes the subscription from persistence.
        /// </summary>
        /// <returns>A task.</returns>
        public async Task RollbackAsync()
        {
            if (this.Result != null)
            {
                try
                {
                    // delete the inserted row
                    await this.CustomerSubscriptionsRepository.DeleteAsync(this.Result);
                }
                catch (Exception deletionProblem)
                {
                    if (deletionProblem.IsFatal())
                    {
                        throw;
                    }

                    Trace.TraceError(
                        "RecordNewCustomerSubscription.RollbackAsync failed: {0}, Customer ID: {1}, Subscription ID: {2}",
                        deletionProblem,
                        this.Result.CustomerId,
                        this.Result.SubscriptionId);

                    // TODO: Notify the system integrity recovery component
                }

                this.Result = null;
            }
        }
    }
}