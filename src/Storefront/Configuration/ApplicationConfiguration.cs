// -----------------------------------------------------------------------
// <copyright file="ApplicationConfiguration.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Storefront.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.IO;
    using System.Web;
    using System.Web.Configuration;
    using BusinessLogic.Commerce.PaymentGateways;
    using Manager;

    /// <summary>
    /// Abstracts the Web server configuration stored in different places such as web.config.
    /// </summary>
    public static class ApplicationConfiguration
    {
        /// <summary>
        /// The AAD endpoint configuration key.
        /// </summary>
        private const string ActiveDirectoryEndPointKey = "aadEndpoint";

        /// <summary>
        /// The AD Graph endpoint configuration key.
        /// </summary>
        private const string ActiveDirectoryGraphEndPointKey = "aadGraphEndpoint";

        /// <summary>
        /// The web portal AD client ID configuration key.
        /// </summary>
        private const string WebPortalADClientIDKey = "webPortal.clientId";

        /// <summary>
        /// The web portal AD client secret configuration key.
        /// </summary>
        private const string WebPortalADClientSecretKey = "webPortal.clientSecret";

        /// <summary>
        /// The web portal AD tenant ID.
        /// </summary>        
        private const string WebPortalAadTenantID = "webPortal.AadTenantId";

        /// <summary>
        /// The web portal configuration file path configuration key.
        /// </summary>
        private const string WebPortalConfigurationFilePathKey = "webPortal.configurationPath";

        /// <summary>
        /// The Azure storage connection string configuration key.
        /// </summary>
        private const string AzureStorageConnectionStringKey = "webPortal.azureStorageConnectionString";

        /// <summary>
        /// The Azure storage connection endpoint suffix key.
        /// </summary>
        private const string AzureStorageConnectionEndpointSuffixKey = "webPortal.azureStorageConnectionEndpointSuffix";

        /// <summary>
        /// The cache connection string configuration key.
        /// </summary>
        private const string CacheConnectionStringKey = "webPortal.cacheConnectionString";

        /// <summary>
        /// The web portal configuration manager configuration key.
        /// </summary>
        private const string WebPortalConfigurationManagerKey = "WebPortalConfigurationManager";

        /// <summary>
        /// A lazy reference to client configuration.
        /// </summary>
        private static Lazy<IDictionary<string, dynamic>> clientConfiguration = new Lazy<IDictionary<string, dynamic>>(
            () => WebPortalConfigurationManager.GenerateConfigurationDictionary());

        /// <summary>
        /// Gets the web portal configuration file path.
        /// </summary>
        public static string WebPortalConfigurationFilePath => Path.Combine(
            HttpRuntime.AppDomainAppPath,
            $"{WebConfigurationManager.AppSettings[WebPortalConfigurationFilePathKey]}{PaymentGatewayConfig.GetWebConfigPath()}");

        /// <summary>
        /// Gets the client configuration.
        /// </summary>
        public static IDictionary<string, dynamic> ClientConfiguration => clientConfiguration.Value;

        /// <summary>
        /// Gets or sets the web portal configuration manager instance.
        /// </summary>
        public static WebPortalConfigurationManager WebPortalConfigurationManager
        {
            get => HttpContext.Current.Application[WebPortalConfigurationManagerKey] as WebPortalConfigurationManager;

            set => HttpContext.Current.Application[WebPortalConfigurationManagerKey] = value;
        }

        /// <summary>
        /// Gets the Azure Active Directory endpoint used by the web portal.
        /// </summary>
        public static string ActiveDirectoryEndPoint => ConfigurationManager.AppSettings[ActiveDirectoryEndPointKey];

        /// <summary>
        /// Gets the Azure Active Directory Graph endpoint used by the web portal.
        /// </summary>
        public static string ActiveDirectoryGraphEndPoint => ConfigurationManager.AppSettings[ActiveDirectoryGraphEndPointKey];

        /// <summary>
        /// Gets the Azure Active Directory client ID of the web portal.
        /// </summary>
        public static string ActiveDirectoryClientID => ConfigurationManager.AppSettings[WebPortalADClientIDKey];

        /// <summary>
        /// Gets the Azure Active Directory client secret of the web portal.
        /// </summary>
        public static string ActiveDirectoryClientSecret => ConfigurationManager.AppSettings[WebPortalADClientSecretKey];

        /// <summary>
        /// Gets the Azure Active Directory ID of the web portal.
        /// </summary>
        public static string ActiveDirectoryTenantId => ConfigurationManager.AppSettings[WebPortalAadTenantID];

        /// <summary>
        /// Gets the Azure storage connection string.
        /// </summary>
        public static string AzureStorageConnectionString => ConfigurationManager.AppSettings[AzureStorageConnectionStringKey];

        /// <summary>
        /// Gets the Azure Azure storage endpoint suffix.
        /// </summary>
        public static string AzureStorageConnectionEndpointSuffix => ConfigurationManager.AppSettings[AzureStorageConnectionEndpointSuffixKey];

        /// <summary>        
        /// Gets the cache connection string.
        /// </summary>
        public static string CacheConnectionString => ConfigurationManager.AppSettings[CacheConnectionStringKey];
    }
}