// -----------------------------------------------------------------------
// <copyright file="PreApprovedCustomersRepository.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.CustomerPortal.BusinessLogic
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
            var sdkClient = ApplicationDomain.Instance.PartnerCenterClient;            
            List<Customer> allCustomers = new List<Customer>(); 

            // create a customer enumerator which will aid us in traversing the customer pages
            var customersEnumerator = sdkClient.Enumerators.Customers.Create(sdkClient.Customers.Query(QueryFactory.Instance.BuildIndexedQuery(100)));            
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
            PreApprovedCustomersList currentPreApprovedCustomers = await this.RetrieveAsync();
            if (currentPreApprovedCustomers.CustomerIds != null)
            {
                // Find if the all customers approved entry is present. 
                allCustomersPreApproved = currentPreApprovedCustomers.CustomerIds.Where(cid => (cid == Guid.Empty.ToString())).Count() > 0;
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
                        preApprovedCustomerDetails.Where(customer => customer.TenantId == customerId).FirstOrDefault().IsPreApproved = true;
                    }
                    catch (NullReferenceException)
                    {
                    }
                }
            }

            return new PreApprovedCustomersViewModel()
            {
                IsEveryCustomerPreApproved = allCustomersPreApproved,
                CustomerIds = allCustomersPreApproved ? null : currentPreApprovedCustomers.CustomerIds?.ToList(),
                Items = preApprovedCustomerDetails.OrderBy(customer => customer.CompanyName)
            };
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

            var preApprovedCustomersBlob = await this.GetPreApprovedCustomersBlob();
            await preApprovedCustomersBlob.UploadTextAsync(JsonConvert.SerializeObject(customerList));

            // invalidate the cache, we do not update it to avoid race condition between web instances
            await this.ApplicationDomain.CachingService.ClearAsync(PreApprovedCustomersRepository.PreApprovedCustomersCacheKey);

            return await this.RetrieveCustomerDetailsAsync();
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

            PreApprovedCustomersList existingCustomers = await this.RetrieveAsync();
            if (existingCustomers.CustomerIds != null)
            {
                // Find if the all customers approved entry is present. 
                int allCustomersApproved = existingCustomers.CustomerIds.Where(cid => (cid == Guid.Empty.ToString())).Count();
                if (allCustomersApproved > 0)
                {
                    isCustomerPreApproved = true;
                }
                else
                {
                    // check if current customer is in the pre approved list. 
                    int currentCustomerApproved = existingCustomers.CustomerIds.Where(cid => cid == customerId).Count();
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
            var portalAssetsBlobContainer = await this.ApplicationDomain.AzureStorageService.GetPrivateCustomerPortalAssetsBlobContainerAsync();

            return portalAssetsBlobContainer.GetBlockBlobReference(PreApprovedCustomersRepository.PreApprovedCustomersBlobName);
        }

        /// <summary>
        /// Retrieves the PreApproved Customers from persistence.
        /// </summary>
        /// <returns>The PreApproved Customers.</returns>
        private async Task<PreApprovedCustomersList> RetrieveAsync()
        {
            var preApprovedCustomersList = await this.ApplicationDomain.CachingService
                .FetchAsync<PreApprovedCustomersList>(PreApprovedCustomersRepository.PreApprovedCustomersCacheKey);

            if (preApprovedCustomersList == null)
            {
                var preApprovedCustomersBlob = await this.GetPreApprovedCustomersBlob();
                preApprovedCustomersList = new PreApprovedCustomersList();

                if (await preApprovedCustomersBlob.ExistsAsync())
                {
                    preApprovedCustomersList = JsonConvert.DeserializeObject<PreApprovedCustomersList>(await preApprovedCustomersBlob.DownloadTextAsync());

                    // cache the preapproved customers configuration
                    await this.ApplicationDomain.CachingService.StoreAsync<PreApprovedCustomersList>(
                        PreApprovedCustomersRepository.PreApprovedCustomersCacheKey,
                        preApprovedCustomersList);
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