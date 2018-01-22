// -----------------------------------------------------------------------
// <copyright file="WebPortalConfigurationFactory.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.CustomerPortal.Configuration.Manager
{
    /// <summary>
    /// The default web portal configuration factory implementation.
    /// </summary>
    public class WebPortalConfigurationFactory : IWebPortalConfigurationFactory
    {
        /// <summary>
        /// Creates a new web portal configuration manager instance.
        /// </summary>
        /// <param name="configurationFilePath">The web portal configuration file path.</param>
        /// <returns>A new web portal configuration manager instance.</returns>
        public WebPortalConfigurationManager Create(string configurationFilePath)
        {
            return new StandardWebPortalConfigurationManager(configurationFilePath);
        }
    }
}