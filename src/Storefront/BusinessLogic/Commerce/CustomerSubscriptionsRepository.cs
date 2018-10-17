// -----------------------------------------------------------------------
// <copyright file="CustomerSubscriptionsRepository.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Storefront.BusinessLogic.Commerce
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Exceptions;
    using Models;
    using WindowsAzure.Storage.Table;

    /// <summary>
    /// Manages persisting customer subscriptions.
    /// </summary>
    public class CustomerSubscriptionsRepository : DomainObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CustomerSubscriptionsRepository"/> class.
        /// </summary>
        /// <param name="applicationDomain">An application domain instance.</param>
        public CustomerSubscriptionsRepository(ApplicationDomain applicationDomain) : base(applicationDomain)
        {
        }

        /// <summary>
        /// Adds a new customer subscription.
        /// </summary>
        /// <param name="newCustomerSubscription">The new subscription information.</param>
        /// <returns>The just added customer subscription.</returns>
        public async Task<CustomerSubscriptionEntity> AddAsync(CustomerSubscriptionEntity newCustomerSubscription)
        {
            newCustomerSubscription.AssertNotNull(nameof(newCustomerSubscription));

            CustomerSubscriptionTableEntity customerSubscriptionTableEntity = new CustomerSubscriptionTableEntity(newCustomerSubscription.CustomerId, newCustomerSubscription.SubscriptionId)
            {
                PartnerOfferId = newCustomerSubscription.PartnerOfferId,
                ExpiryDate = newCustomerSubscription.ExpiryDate
            };

            CloudTable customerSubscriptionsTable = await ApplicationDomain.AzureStorageService.GetCustomerSubscriptionsTableAsync().ConfigureAwait(false);

            TableResult insertionResult = await customerSubscriptionsTable.ExecuteAsync(TableOperation.Insert(customerSubscriptionTableEntity)).ConfigureAwait(false);
            insertionResult.HttpStatusCode.AssertHttpResponseSuccess(ErrorCode.PersistenceFailure, "Failed to add customer subscription", insertionResult.Result);

            return newCustomerSubscription;
        }

        /// <summary>
        /// Removes a customer subscription.
        /// </summary>
        /// <param name="customerSubscriptionToRemove">The customer subscription to remove.</param>
        /// <returns>A task.</returns>
        public async Task DeleteAsync(CustomerSubscriptionEntity customerSubscriptionToRemove)
        {
            customerSubscriptionToRemove.AssertNotNull(nameof(customerSubscriptionToRemove));

            CloudTable customerSubscriptionsTable = await ApplicationDomain.AzureStorageService.GetCustomerSubscriptionsTableAsync().ConfigureAwait(false);

            TableResult deletionResult = await customerSubscriptionsTable.ExecuteAsync(TableOperation.Delete(new CustomerSubscriptionTableEntity(customerSubscriptionToRemove.CustomerId, customerSubscriptionToRemove.SubscriptionId) { ETag = "*" })).ConfigureAwait(false);
            deletionResult.HttpStatusCode.AssertHttpResponseSuccess(ErrorCode.PersistenceFailure, "Failed to remove customer subscription", deletionResult.Result);
        }

        /// <summary>
        /// Updates a customer subscription.
        /// </summary>
        /// <param name="customerSubscriptionUpdates">The customer subscription updates.</param>
        /// <returns>The updated customer subscription.</returns>
        public async Task<CustomerSubscriptionEntity> UpdateAsync(CustomerSubscriptionEntity customerSubscriptionUpdates)
        {
            customerSubscriptionUpdates.AssertNotNull(nameof(customerSubscriptionUpdates));

            TableOperation updateSubscriptionOperation = TableOperation.Replace(new CustomerSubscriptionTableEntity(customerSubscriptionUpdates.CustomerId, customerSubscriptionUpdates.SubscriptionId)
            {
                ExpiryDate = customerSubscriptionUpdates.ExpiryDate,
                PartnerOfferId = customerSubscriptionUpdates.PartnerOfferId,
                ETag = "*"
            });

            CloudTable customerSubscriptionsTable = await ApplicationDomain.AzureStorageService.GetCustomerSubscriptionsTableAsync().ConfigureAwait(false);
            TableResult updateResult = await customerSubscriptionsTable.ExecuteAsync(updateSubscriptionOperation).ConfigureAwait(false);

            updateResult.HttpStatusCode.AssertHttpResponseSuccess(ErrorCode.PersistenceFailure, "Failed to update customer subscription", updateResult.Result);

            return customerSubscriptionUpdates;
        }

        /// <summary>
        /// Retrieves all customer subscriptions.
        /// </summary>
        /// <param name="customerId">The ID of the customer who owns the subscriptions.</param>
        /// <returns>A list of customer subscriptions.</returns>
        public async Task<IEnumerable<CustomerSubscriptionEntity>> RetrieveAsync(string customerId)
        {
            customerId.AssertNotEmpty(nameof(customerId));

            CloudTable customerSubscriptionsTable = await ApplicationDomain.AzureStorageService.GetCustomerSubscriptionsTableAsync().ConfigureAwait(false);
            TableQuery<CustomerSubscriptionTableEntity> getCustomerSubscriptionsQuery = new TableQuery<CustomerSubscriptionTableEntity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, customerId));

            TableQuerySegment<CustomerSubscriptionTableEntity> resultSegment = null;

            ICollection<CustomerSubscriptionEntity> customerSubscriptions = new List<CustomerSubscriptionEntity>();

            do
            {
                resultSegment = await customerSubscriptionsTable.ExecuteQuerySegmentedAsync(getCustomerSubscriptionsQuery, resultSegment?.ContinuationToken).ConfigureAwait(false);

                foreach (CustomerSubscriptionTableEntity customerSubscriptionResult in resultSegment.AsEnumerable())
                {
                    customerSubscriptions.Add(new CustomerSubscriptionEntity(
                        customerSubscriptionResult.PartitionKey,
                        customerSubscriptionResult.RowKey,
                        customerSubscriptionResult.PartnerOfferId,
                        customerSubscriptionResult.ExpiryDate));
                }
            }
            while (resultSegment.ContinuationToken != null);

            return customerSubscriptions;
        }

        /// <summary>
        /// A azure table entity for customer subscriptions.
        /// </summary>
        private class CustomerSubscriptionTableEntity : TableEntity
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="CustomerSubscriptionTableEntity"/> class.
            /// </summary>
            public CustomerSubscriptionTableEntity()
            {
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="CustomerSubscriptionTableEntity"/> class.
            /// </summary>
            /// <param name="customerId">The customer ID.</param>
            /// <param name="subscriptionId">The subscription ID.</param>
            public CustomerSubscriptionTableEntity(string customerId, string subscriptionId)
            {
                PartitionKey = customerId;
                RowKey = subscriptionId;
            }

            /// <summary>
            /// Gets or sets the partner offer ID.
            /// </summary>
            public string PartnerOfferId { get; set; }

            /// <summary>
            /// Gets or sets the expiry date.
            /// </summary>
            public DateTime ExpiryDate { get; set; }
        }
    }
}