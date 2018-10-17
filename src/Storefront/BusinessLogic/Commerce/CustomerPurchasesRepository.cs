// -----------------------------------------------------------------------
// <copyright file="CustomerPurchasesRepository.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Storefront.BusinessLogic.Commerce
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using Exceptions;
    using Models;
    using WindowsAzure.Storage.Table;

    /// <summary>
    /// Encapsulates persistence for customer purchases.
    /// </summary>
    public class CustomerPurchasesRepository : DomainObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CustomerPurchasesRepository"/> class.
        /// </summary>
        /// <param name="applicationDomain">An instance of the application domain.</param>
        public CustomerPurchasesRepository(ApplicationDomain applicationDomain) : base(applicationDomain)
        {
        }

        /// <summary>
        /// Adds a new customer purchase into persistence.
        /// </summary>
        /// <param name="newCustomerPurchase">The new customer purchase persistence to add.</param>
        /// <returns>The resulting customer purchase that got added.</returns>
        public async Task<CustomerPurchaseEntity> AddAsync(CustomerPurchaseEntity newCustomerPurchase)
        {
            newCustomerPurchase.AssertNotNull(nameof(newCustomerPurchase));

            CustomerPurchaseTableEntity customerPurchaseTableEntity = new CustomerPurchaseTableEntity(newCustomerPurchase.CustomerId, newCustomerPurchase.SubscriptionId)
            {
                SeatPrice = newCustomerPurchase.SeatPrice.ToString(CultureInfo.InvariantCulture),
                SeatsBought = newCustomerPurchase.SeatsBought,
                TransactionDate = newCustomerPurchase.TransactionDate,
                PurchaseType = newCustomerPurchase.PurchaseType.ToString()
            };

            CloudTable customerPurchasesTable = await ApplicationDomain.AzureStorageService.GetCustomerPurchasesTableAsync().ConfigureAwait(false);

            TableResult insertionResult = await customerPurchasesTable.ExecuteAsync(TableOperation.Insert(customerPurchaseTableEntity)).ConfigureAwait(false);
            insertionResult.HttpStatusCode.AssertHttpResponseSuccess(ErrorCode.PersistenceFailure, "Failed to add customer purchase", insertionResult.Result);

            newCustomerPurchase = new CustomerPurchaseEntity(
                newCustomerPurchase.PurchaseType,
                customerPurchaseTableEntity.RowKey,
                newCustomerPurchase.CustomerId,
                newCustomerPurchase.SubscriptionId,
                newCustomerPurchase.SeatsBought,
                newCustomerPurchase.SeatPrice,
                newCustomerPurchase.TransactionDate);

            return newCustomerPurchase;
        }

        /// <summary>
        /// Removes a purchase entity from persistence.
        /// </summary>
        /// <param name="customerPurchaseToRemove">The customer purchase to remove.</param>
        /// <returns>A task.</returns>
        public async Task DeleteAsync(CustomerPurchaseEntity customerPurchaseToRemove)
        {
            customerPurchaseToRemove.AssertNotNull(nameof(customerPurchaseToRemove));

            CloudTable customerPurchasesTable = await ApplicationDomain.AzureStorageService.GetCustomerPurchasesTableAsync().ConfigureAwait(false);

            TableResult deletionResult = await customerPurchasesTable.ExecuteAsync(
                TableOperation.Delete(new CustomerPurchaseTableEntity(customerPurchaseToRemove.CustomerId, customerPurchaseToRemove.SubscriptionId) { RowKey = customerPurchaseToRemove.Id, ETag = "*" })).ConfigureAwait(false);

            deletionResult.HttpStatusCode.AssertHttpResponseSuccess(ErrorCode.PersistenceFailure, "Failed to delete customer purchase", deletionResult.Result);
        }

        /// <summary>
        /// Retrieves all purchases made by a customer from persistence.
        /// </summary>
        /// <param name="customerId">The customer ID.</param>
        /// <returns>The customer's purchases.</returns>
        public async Task<IEnumerable<CustomerPurchaseEntity>> RetrieveAsync(string customerId)
        {
            customerId.AssertNotEmpty(nameof(customerId));

            CloudTable customerPurchasesTable = await ApplicationDomain.AzureStorageService.GetCustomerPurchasesTableAsync().ConfigureAwait(false);
            TableQuery<CustomerPurchaseTableEntity> getCustomerPurchasesQuery = new TableQuery<CustomerPurchaseTableEntity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, customerId));

            TableQuerySegment<CustomerPurchaseTableEntity> resultSegment = null;

            ICollection<CustomerPurchaseEntity> customerPurchases = new List<CustomerPurchaseEntity>();

            do
            {
                resultSegment = await customerPurchasesTable.ExecuteQuerySegmentedAsync(getCustomerPurchasesQuery, resultSegment?.ContinuationToken).ConfigureAwait(false);

                foreach (CustomerPurchaseTableEntity customerPurchaseResult in resultSegment.AsEnumerable())
                {
                    customerPurchases.Add(new CustomerPurchaseEntity(
                        (CommerceOperationType)Enum.Parse(typeof(CommerceOperationType), customerPurchaseResult.PurchaseType, true),
                        customerPurchaseResult.RowKey,
                        customerPurchaseResult.PartitionKey,
                        customerPurchaseResult.SubscriptionId,
                        customerPurchaseResult.SeatsBought,
                        decimal.Parse(customerPurchaseResult.SeatPrice, CultureInfo.CurrentCulture),
                        customerPurchaseResult.TransactionDate));
                }
            }
            while (resultSegment.ContinuationToken != null);

            return customerPurchases;
        }

        /// <summary>
        /// An azure table entity that describes a customer purchase.
        /// </summary>
        private class CustomerPurchaseTableEntity : TableEntity
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="CustomerPurchaseTableEntity"/> class.
            /// </summary>
            public CustomerPurchaseTableEntity()
            {
                RowKey = Guid.NewGuid().ToString();
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="CustomerPurchaseTableEntity"/> class.
            /// </summary>
            /// <param name="customerId">The customer ID.</param>
            /// <param name="subscriptionId">The subscription ID.</param>
            public CustomerPurchaseTableEntity(string customerId, string subscriptionId)
            {
                PartitionKey = customerId;
                RowKey = Guid.NewGuid().ToString();
                SubscriptionId = subscriptionId;
            }

            /// <summary>
            /// Gets or sets the subscription ID.
            /// </summary>
            public string SubscriptionId { get; set; }

            /// <summary>
            /// Gets or sets the commerce purchase type for this purchase item.
            /// </summary>
            public string PurchaseType { get; set; }

            /// <summary>
            /// Gets or sets the seat price.
            /// </summary>
            public string SeatPrice { get; set; }

            /// <summary>
            /// Gets or sets the number of seats bought.
            /// </summary>
            public int SeatsBought { get; set; }

            /// <summary>
            /// Gets or sets the transaction date.
            /// </summary>
            public DateTime TransactionDate { get; set; }
        }
    }
}