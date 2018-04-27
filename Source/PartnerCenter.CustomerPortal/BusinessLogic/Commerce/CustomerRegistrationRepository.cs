// -----------------------------------------------------------------------
// <copyright file="CustomerRegistrationRepository.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.CustomerPortal.BusinessLogic.Commerce
{
    using System.Linq;
    using System.Threading.Tasks;
    using Exceptions;
    using Models;
    using Newtonsoft.Json;
    using WindowsAzure.Storage.Table;

    /// <summary>
    /// Encapsulates persistence for customer registrations
    /// </summary>
    public class CustomerRegistrationRepository : DomainObject
    {
        /// <summary>
        /// The partner customer key in the cache.
        /// </summary>
        private const string PartnerCustomerCacheKey = "PartnerCustomers";

        /// <summary>
        /// The Azure BLOB name for the partner customer details.
        /// </summary>
        private const string PartnerCustomerBlobName = "partnercustomers";

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomerRegistrationRepository"/> class.
        /// </summary>
        /// <param name="applicationDomain">An instance of the application domain.</param>
        public CustomerRegistrationRepository(ApplicationDomain applicationDomain) : base(applicationDomain)
        {
        }

        /// <summary>
        /// Adds a new customer registration details into persistence.
        /// </summary>
        /// <param name="customerRegistrationInfo">The new customer registration details to add.</param>
        /// <returns>The resulting customer registration details that get added.</returns>
        public async Task<CustomerViewModel> AddAsync(CustomerViewModel customerRegistrationInfo)
        {
            customerRegistrationInfo.AssertNotNull(nameof(customerRegistrationInfo));

            var customerRegistrationTable = await this.ApplicationDomain.AzureStorageService.GetCustomerRegistrationTableAsync().ConfigureAwait(false);
            CustomerRegistrationTableEntity customerRegistrationTableEntity = new CustomerRegistrationTableEntity(customerRegistrationInfo);

            var insertionResult = await customerRegistrationTable.ExecuteAsync(TableOperation.Insert(customerRegistrationTableEntity)).ConfigureAwait(false);
            insertionResult.HttpStatusCode.AssertHttpResponseSuccess(ErrorCode.PersistenceFailure, "Failed to add customer registration details", insertionResult.Result);

            return customerRegistrationInfo;
        }

        /// <summary>
        /// Removes customer from persistence.
        /// </summary>
        /// <param name="customerId">Id of the customer to remove.</param>
        /// <returns>A task.</returns>
        public async Task DeleteAsync(string customerId)
        {
            customerId.AssertNotEmpty(nameof(customerId));

            var customerRegistrationTable = await this.ApplicationDomain.AzureStorageService.GetCustomerRegistrationTableAsync().ConfigureAwait(false);

            var deletionResult = customerRegistrationTable.Execute(
                TableOperation.Delete(new CustomerRegistrationTableEntity() { PartitionKey = customerId, RowKey = customerId, ETag = "*" }));

            deletionResult.HttpStatusCode.AssertHttpResponseSuccess(ErrorCode.PersistenceFailure, "Failed to delete persisted customer registration info", deletionResult.Result);
        }

        /// <summary>
        /// Retrieves customer registration details from persistence.
        /// </summary>
        /// <param name="customerGuid">The customer ID.</param>
        /// <returns>The customer's registration details.</returns>
        public async Task<CustomerViewModel> RetrieveAsync(string customerGuid)
        {
            customerGuid.AssertNotEmpty(nameof(customerGuid));

            var customerRegistrationTable = await this.ApplicationDomain.AzureStorageService.GetCustomerRegistrationTableAsync().ConfigureAwait(false);

            string tableQueryFilter = TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, customerGuid);

            var getCustomerOrdersQuery = new TableQuery<CustomerRegistrationTableEntity>().Where(tableQueryFilter);

            TableQuerySegment<CustomerRegistrationTableEntity> resultSegment = null;
            CustomerViewModel customerRegistrationInfo = null;

            resultSegment = await customerRegistrationTable.ExecuteQuerySegmentedAsync(getCustomerOrdersQuery, resultSegment?.ContinuationToken).ConfigureAwait(false);

            do
            {
                resultSegment = await customerRegistrationTable.ExecuteQuerySegmentedAsync(getCustomerOrdersQuery, resultSegment?.ContinuationToken).ConfigureAwait(false);

                foreach (var customerResult in resultSegment.AsEnumerable())
                {
                    if (customerResult.RowKey == customerGuid)
                    {
                        customerRegistrationInfo = JsonConvert.DeserializeObject<CustomerViewModel>(customerResult.CustomerRegistrationBlob);
                    }
                }
            }
            while (resultSegment.ContinuationToken != null);

            return customerRegistrationInfo;
        }

        /// <summary>
        /// A azure table entity for customer registrations.
        /// </summary>
        private class CustomerRegistrationTableEntity : TableEntity
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="CustomerRegistrationTableEntity"/> class.
            /// </summary>
            public CustomerRegistrationTableEntity()
            {
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="CustomerRegistrationTableEntity"/> class.
            /// </summary>
            /// <param name="customerRegistrationInfo">The customer registration entity</param>
            public CustomerRegistrationTableEntity(CustomerViewModel customerRegistrationInfo)
            {
                this.RowKey = customerRegistrationInfo.MicrosoftId;
                this.PartitionKey = customerRegistrationInfo.MicrosoftId;
                this.CustomerRegistrationBlob = JsonConvert.SerializeObject(customerRegistrationInfo, Formatting.None);
            }

            /// <summary>
            /// Gets or sets the blob which contains the customer registration details. 
            /// </summary>
            public string CustomerRegistrationBlob { get; set; }
        }
    }
}