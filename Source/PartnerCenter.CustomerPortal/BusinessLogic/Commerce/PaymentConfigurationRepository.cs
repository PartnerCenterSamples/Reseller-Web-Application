// -----------------------------------------------------------------------
// <copyright file="PaymentConfigurationRepository.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.CustomerPortal.BusinessLogic
{
    using System.Linq;
    using System.Threading.Tasks;
    using Exceptions;
    using Models;
    using Newtonsoft.Json;
    using WindowsAzure.Storage.Blob;

    /// <summary>
    /// Manages persistence for payment configuration options.
    /// </summary>
    public class PaymentConfigurationRepository : DomainObject
    {
        /// <summary>
        /// The payment configuration key in the cache.
        /// </summary>
        private const string PaymentConfigurationCacheKey = "PaymentConfiguration";

        /// <summary>
        /// The Azure BLOB name for the portal payment configuration.
        /// </summary>
        private const string PaymentConfigurationBlobName = "paymentconfiguration";

        /// <summary>
        /// The supported payment modes. We can move this to web.config is needed later.
        /// </summary>
        private readonly string[] supportedPaymentModes = { "sandbox", "live" };

        /// <summary>
        /// Initializes a new instance of the <see cref="PaymentConfigurationRepository"/> class.
        /// </summary>
        /// <param name="applicationDomain">An application domain instance.</param>
        public PaymentConfigurationRepository(ApplicationDomain applicationDomain) : base(applicationDomain)
        {
        }

        /// <summary>
        /// Indicates whether the payment configuration has been set or not.
        /// </summary>
        /// <returns>True if configured, false otherwise.</returns>
        public async Task<bool> IsConfiguredAsync()
        {
            var paymentConfigurationBlob = await this.GetPaymentConfigurationBlob();
            return await paymentConfigurationBlob.ExistsAsync();
        }

        /// <summary>
        /// Retrieves the payment configuration from persistence.
        /// </summary>
        /// <returns>The payment configuration.</returns>
        public async Task<PaymentConfiguration> RetrieveAsync()
        {
            var paymentConfiguration = await this.ApplicationDomain.CachingService
                .FetchAsync<PaymentConfiguration>(PaymentConfigurationRepository.PaymentConfigurationCacheKey);

            if (paymentConfiguration == null)
            {
                var paymentConfigurationBlob = await this.GetPaymentConfigurationBlob();
                paymentConfiguration = new PaymentConfiguration();

                if (await paymentConfigurationBlob.ExistsAsync())
                {
                    paymentConfiguration = JsonConvert.DeserializeObject<PaymentConfiguration>(await paymentConfigurationBlob.DownloadTextAsync());
                    await this.NormalizeAsync(paymentConfiguration);

                    // cache the payment configuration
                    await this.ApplicationDomain.CachingService.StoreAsync<PaymentConfiguration>(
                        PaymentConfigurationRepository.PaymentConfigurationCacheKey,
                        paymentConfiguration);
                }
            }
            
            return paymentConfiguration;
        }

        /// <summary>
        /// Updates the payment configuration.
        /// </summary>
        /// <param name="newPaymentConfiguration">The new payment configuration.</param>
        /// <returns>The updated payment configuration.</returns>
        public async Task<PaymentConfiguration> UpdateAsync(PaymentConfiguration newPaymentConfiguration)
        {
            newPaymentConfiguration.AssertNotNull(nameof(newPaymentConfiguration));
           
            await this.NormalizeAsync(newPaymentConfiguration);

            var paymentConfigurationBlob = await this.GetPaymentConfigurationBlob();
            await paymentConfigurationBlob.UploadTextAsync(JsonConvert.SerializeObject(newPaymentConfiguration));

            // invalidate the cache, we do not update it to avoid race condition between web instances
            await this.ApplicationDomain.CachingService.ClearAsync(PaymentConfigurationRepository.PaymentConfigurationCacheKey);

            return newPaymentConfiguration;
        }
        
        /// <summary>
        /// Applies business rules to <see cref="PaymentConfiguration"/> instances.
        /// </summary>
        /// <param name="paymentConfiguration">A payment configuration instance.</param>
        /// <returns>A task.</returns>
        private async Task NormalizeAsync(PaymentConfiguration paymentConfiguration)
        {
            paymentConfiguration.AssertNotNull(nameof(paymentConfiguration));

            // Dont validate WebExperienceProfileId since it will break upgrade as existing deployments dont have this configuration. 
            paymentConfiguration.ClientId.AssertNotEmpty("ClientId");
            paymentConfiguration.ClientSecret.AssertNotEmpty("ClientSecret");
            paymentConfiguration.AccountType.AssertNotEmpty("Mode");

            if (!this.supportedPaymentModes.Contains(paymentConfiguration.AccountType))
            {
                throw new PartnerDomainException(Resources.InvalidPaymentModeErrorMessage);
            }

            await Task.FromResult(0);
        }

        /// <summary>
        /// Retrieves the portal payment configuration BLOB reference.
        /// </summary>
        /// <returns>The portal payment configuration BLOB.</returns>
        private async Task<CloudBlockBlob> GetPaymentConfigurationBlob()
        {
            var portalAssetsBlobContainer = await this.ApplicationDomain.AzureStorageService.GetPrivateCustomerPortalAssetsBlobContainerAsync();

            return portalAssetsBlobContainer.GetBlockBlobReference(PaymentConfigurationRepository.PaymentConfigurationBlobName);
        }
    }
}