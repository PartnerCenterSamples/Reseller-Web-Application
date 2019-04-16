// -----------------------------------------------------------------------
// <copyright file="PreApprovedCustomersRepository.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Storefront.BusinessLogic
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Models;
    using Newtonsoft.Json;
    using PartnerCenter.Models.Customers;
    using PartnerCenter.Models.Query;
    using WindowsAzure.Storage.Blob;

    /// <summary>
    /// Manages persistence for PreApprovedCustomersRepository configuration options.
    /// </summary>
    public class PreApprovedCustomersRepository : DomainObject
    {
        /// <summary>
        /// The PreApprovedCustomers key in the cache.
        /// </summary>
        private const string PreApprovedCustomersCacheKey = "PreApprovedCustomers";

        /// <summary>
        /// The Azure BLOB name for the portal PreApprovedCustomers configuration.
        /// </summary>
        private const string PreApprovedCustomersBlobName = "preapprovedcustomers";

        /// <summary>
        /// Initializes a new instance of the <see cref="PreApprovedCustomersRepository"/> class.
        /// </summary>
        /// <param name="applicationDomain">An application domain instance.</param>
        public PreApprovedCustomersRepository(ApplicationDomain applicationDomain) : base(applicationDomain)
        {
        }

        /// <summary>
        /// Retrieves the PreApproved Customers for rendering in UX.
        /// </summary>
        /// <returns>The PreApproved Customers.</returns>
        public async Task<PreApprovedCustomersViewModel> RetrieveCustomerDetailsAsync()
        {
            // retrieve the list of customers from Partner Center.             
            IAggregatePartner sdkClient = ApplicationDomain.Instance.PartnerCenterClient;
            List<Customer> allCustomers = new List<Customer>();

            // create a customer enumerator which will aid us in traversing the customer pages
            Enumerators.IResourceCollectionEnumerator<PartnerCenter.Models.SeekBasedResourceCollection<Customer>> customersEnumerator = sdkClient.Enumerators.Customers.Create(sdkClient.Customers.Query(QueryFactory.Instance.BuildIndexedQuery(100)));
            while (customersEnumerator.HasValue)
            {
                foreach (Customer c in customersEnumerator.Current.Items)
                {
                    allCustomers.Add(c);
                }

                customersEnumerator.Next();
            }

            // if all customers are preapproved then every customer's IsPreApproved is true. 
            bool allCustomersPreApproved = false;
            PreApprovedCustomersList currentPreApprovedCustomers = await RetrieveAsync().ConfigureAwait(false);
            if (currentPreApprovedCustomers.CustomerIds != null)
            {
                // Find if the all customers approved entry is present. 
                allCustomersPreApproved = currentPreApprovedCustomers.CustomerIds.Any(cid => cid == Guid.Empty.ToString());
            }

            // populate portal customer list. 
            List<PortalCustomer> preApprovedCustomerDetails = (from customer in allCustomers
                                                               select new PortalCustomer()
                                                               {
                                                                   TenantId = customer.Id,
                                                                   CompanyName = customer.CompanyProfile.CompanyName,
                                                                   Domain = customer.CompanyProfile.Domain,
                                                                   IsPreApproved = false
                                                               }).ToList();

            // identify the customers who are preapproved and update them. 
            if (!allCustomersPreApproved && (currentPreApprovedCustomers.CustomerIds != null))
            {
                foreach (string customerId in currentPreApprovedCustomers.CustomerIds)
                {
                    try
                    {
                        // can raise an exception if a customer has been removed from PartnerCenter although preapproved in the portal.                         
                        preApprovedCustomerDetails.FirstOrDefault(customer => customer.TenantId == customerId).IsPreApproved = true;
                    }
                    catch (NullReferenceException)
                    {
                        // This has been intentionally left empty.
                    }
                }
            }

            PreApprovedCustomersViewModel viewModel = new PreApprovedCustomersViewModel
            {
                IsEveryCustomerPreApproved = allCustomersPreApproved,
                Items = preApprovedCustomerDetails.OrderBy(customer => customer.CompanyName)
            };

            if (!allCustomersPreApproved && currentPreApprovedCustomers.CustomerIds != null)
            {
                viewModel.CustomerIds.AddRange(currentPreApprovedCustomers.CustomerIds.ToList());
            }

            return viewModel;
        }

        /// <summary>
        /// Updates the PreApproved Customers configuration.
        /// </summary>
        /// <param name="preApprovedCustomers">The new list of PreApproved Customers.</param>
        /// <returns>The updated PreApprovedCustomers configuration.</returns>
        public async Task<PreApprovedCustomersViewModel> UpdateAsync(PreApprovedCustomersViewModel preApprovedCustomers)
        {
            preApprovedCustomers.AssertNotNull(nameof(preApprovedCustomers));

            PreApprovedCustomersList customerList = new PreApprovedCustomersList();
            if (preApprovedCustomers.IsEveryCustomerPreApproved)
            {
                string[] ids = new string[] { Guid.Empty.ToString() };
                customerList.CustomerIds = ids.ToList();
            }
            else
            {
                customerList.CustomerIds = preApprovedCustomers.CustomerIds;
            }

            CloudBlockBlob preApprovedCustomersBlob = await GetPreApprovedCustomersBlob().ConfigureAwait(false);
            await preApprovedCustomersBlob.UploadTextAsync(JsonConvert.SerializeObject(customerList)).ConfigureAwait(false);

            // invalidate the cache, we do not update it to avoid race condition between web instances
            await ApplicationDomain.CachingService.ClearAsync(PreApprovedCustomersCacheKey).ConfigureAwait(false);

            return await RetrieveCustomerDetailsAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Checks whether given customer id is of a pre approved customer or not. 
        /// </summary>        
        /// <param name="customerId">The customer who is transacting.</param>
        /// <returns>True if customer is pre approved else false.</returns>
        public async Task<bool> IsCustomerPreApprovedAsync(string customerId)
        {
            customerId.AssertNotEmpty(nameof(customerId));

            bool isCustomerPreApproved = false;

            PreApprovedCustomersList existingCustomers = await RetrieveAsync().ConfigureAwait(false);
            if (existingCustomers.CustomerIds != null)
            {
                // Find if the all customers approved entry is present. 
                int allCustomersApproved = existingCustomers.CustomerIds.Count(cid => cid == Guid.Empty.ToString());
                if (allCustomersApproved > 0)
                {
                    isCustomerPreApproved = true;
                }
                else
                {
                    // check if current customer is in the pre approved list. 
                    int currentCustomerApproved = existingCustomers.CustomerIds.Count(cid => cid == customerId);
                    isCustomerPreApproved = currentCustomerApproved > 0;
                }
            }

            return isCustomerPreApproved;
        }

        /// <summary>
        /// Retrieves the portal PreApproved Customers configuration BLOB reference.
        /// </summary>
        /// <returns>The portal PreApproved Customers BLOB.</returns>
        private async Task<CloudBlockBlob> GetPreApprovedCustomersBlob()
        {
            CloudBlobContainer portalAssetsBlobContainer = await ApplicationDomain.AzureStorageService.GetPrivateCustomerPortalAssetsBlobContainerAsync().ConfigureAwait(false);

            return portalAssetsBlobContainer.GetBlockBlobReference(PreApprovedCustomersBlobName);
        }

        /// <summary>
        /// Retrieves the PreApproved Customers from persistence.
        /// </summary>
        /// <returns>The PreApproved Customers.</returns>
        private async Task<PreApprovedCustomersList> RetrieveAsync()
        {
            PreApprovedCustomersList preApprovedCustomersList = await ApplicationDomain.CachingService
                .FetchAsync<PreApprovedCustomersList>(PreApprovedCustomersCacheKey).ConfigureAwait(false);

            if (preApprovedCustomersList == null)
            {
                CloudBlockBlob preApprovedCustomersBlob = await GetPreApprovedCustomersBlob().ConfigureAwait(false);
                preApprovedCustomersList = new PreApprovedCustomersList();

                if (await preApprovedCustomersBlob.ExistsAsync().ConfigureAwait(false))
                {
                    preApprovedCustomersList = JsonConvert.DeserializeObject<PreApprovedCustomersList>(await preApprovedCustomersBlob.DownloadTextAsync().ConfigureAwait(false));

                    // cache the preapproved customers configuration
                    await ApplicationDomain.CachingService.StoreAsync(
                        PreApprovedCustomersCacheKey,
                        preApprovedCustomersList).ConfigureAwait(false);
                }
            }

            return preApprovedCustomersList;
        }

        /// <summary>
        /// The Pre Approved customers list model.
        /// </summary>
        private class PreApprovedCustomersList
        {
            /// <summary>
            /// Gets or sets the customer ids who are preapproved.
            /// </summary>        
            public IEnumerable<string> CustomerIds { get; set; }
        }
    }
}