// -----------------------------------------------------------------------
// <copyright file="IWebPortalConfigurationFactory.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Storefront.Configuration.Manager
{
    /// <summary>
    /// Creates <see cref="WebPortalConfigurationManager"/> instances.
    /// </summary>
    public interface IWebPortalConfigurationFactory
    {
        /// <summary>
        /// Creates a new <see cref="WebPortalConfigurationManager"/> instance.
        /// </summary>
        /// <param name="configurationFilePath">The web portal configuration file path.</param>
        /// <returns>A new web portal configuration manager instance.</returns>
        WebPortalConfigurationManager Create(string configurationFilePath);
    }
}