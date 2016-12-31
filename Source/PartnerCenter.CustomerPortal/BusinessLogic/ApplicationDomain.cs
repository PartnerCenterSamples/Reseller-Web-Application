// -----------------------------------------------------------------------
// <copyright file="ApplicationDomain.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.CustomerPortal.BusinessLogic
{
    using System.Configuration;
    using System.Threading.Tasks;
    using Commerce;
    using Configuration;
    using Customers;
    using Offers;
    using PartnerCenter.Extensions;

    /// <summary>
    /// The application domain holds key domain objects needed across the application.
    /// </summary>
    public class ApplicationDomain
    {
        /// <summary>
        /// Prevents a default instance of the <see cref="ApplicationDomain"/> class from being created.
        /// </summary>
        private ApplicationDomain()
        {
        }

        /// <summary>
        /// Gets an instance of the application domain.
        /// </summary>
        public static ApplicationDomain Instance { get; private set; }

        /// <summary>
        /// Gets a Partner Center SDK client.
        /// </summary>
        public IAggregatePartner PartnerCenterClient { get; private set; }

        /// <summary>
        /// Gets a Partner Center customer repository.
        /// </summary>
        public PartnerCenterCustomersRepository CustomersRepository { get; private set; }

        /// <summary>
        /// Gets the partner offers repository.
        /// </summary>
        public PartnerOffersRepository OffersRepository { get; private set; }

        /// <summary>
        /// Gets the Azure storage service.
        /// </summary>
        public AzureStorageService AzureStorageService { get; private set; }

        /// <summary>
        /// Gets the caching service.
        /// </summary>
        public CachingService CachingService { get; private set; }

        /// <summary>
        /// Gets the Microsoft offer logo indexer.
        /// </summary>
        public MicrosoftOfferLogoIndexer MicrosoftOfferLogoIndexer { get; private set; }

        /// <summary>
        /// Gets the portal branding service.
        /// </summary>
        public PortalBranding PortalBranding { get; private set; }

        /// <summary>
        /// Gets the portal localization service.
        /// </summary>
        public PortalLocalization PortalLocalization { get; private set; }

        /// <summary>
        /// Gets the portal payment configuration repository.
        /// </summary>
        public PaymentConfigurationRepository PaymentConfigurationRepository { get; private set; }

        /// <summary>
        /// Gets the customer subscriptions repository.
        /// </summary>
        public CustomerSubscriptionsRepository CustomerSubscriptionsRepository { get; private set; }

        /// <summary>
        /// Gets the customer purchases repository.
        /// </summary>
        public CustomerPurchasesRepository CustomerPurchasesRepository { get; private set; }

        /// <summary>
        /// Gets the portal telemetry service.
        /// </summary>
        public TelemetryService TelemetryService { get; private set; }

        /// <summary>
        /// Initializes the application domain objects.
        /// </summary>
        /// <returns>A task.</returns>
        public static async Task InitializeAsync()
        {
            if (Instance == null)
            {
                Instance = new ApplicationDomain();

                Instance.AzureStorageService = new AzureStorageService(ApplicationConfiguration.AzureStorageConnectionString, ApplicationConfiguration.AzureStorageConnectionEndpointSuffix);
                Instance.CachingService = new CachingService(Instance, ApplicationConfiguration.CacheConnectionString);
                Instance.PartnerCenterClient = await AcquirePartnerCenterAccessAsync();
                Instance.CustomersRepository = new PartnerCenterCustomersRepository(Instance);
                Instance.OffersRepository = new PartnerOffersRepository(Instance);
                Instance.MicrosoftOfferLogoIndexer = new MicrosoftOfferLogoIndexer(Instance);
                Instance.PortalBranding = new PortalBranding(Instance);
                Instance.PaymentConfigurationRepository = new PaymentConfigurationRepository(Instance);
                Instance.PortalLocalization = new PortalLocalization(Instance);
                Instance.CustomerSubscriptionsRepository = new CustomerSubscriptionsRepository(Instance);
                Instance.CustomerPurchasesRepository = new CustomerPurchasesRepository(Instance);
                Instance.TelemetryService = new TelemetryService(Instance);

                await Instance.PortalLocalization.InitializeAsync();
                await Instance.TelemetryService.InitializeAsync();
            }
        }

        /// <summary>
        /// Authenticates with the Partner Center APIs.
        /// </summary>
        /// <returns>A Partner Center API client.</returns>
        private static async Task<IAggregatePartner> AcquirePartnerCenterAccessAsync()
        {
            PartnerService.Instance.ApiRootUrl = ConfigurationManager.AppSettings["partnerCenter.apiEndPoint"];
            PartnerService.Instance.ApplicationName = "Web Store Front V1.1";

            var credentials = await PartnerCredentials.Instance.GenerateByApplicationCredentialsAsync(
                ConfigurationManager.AppSettings["partnercenter.applicationId"],
                ConfigurationManager.AppSettings["partnercenter.applicationSecret"],
                ConfigurationManager.AppSettings["partnercenter.AadTenantId"],
                ConfigurationManager.AppSettings["aadEndpoint"],
                ConfigurationManager.AppSettings["aadGraphEndpoint"]);

            return PartnerService.Instance.CreatePartnerOperations(credentials);
        }
    }
}