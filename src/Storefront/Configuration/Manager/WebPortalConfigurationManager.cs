// -----------------------------------------------------------------------
// <copyright file="WebPortalConfigurationManager.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Storefront.Configuration.Manager
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using Bundling;
    using Newtonsoft.Json;
    using WebPortal;

    /// <summary>
    /// The configuration manager generates the web portal client side configuration settings and is also responsible for generating and aggregating
    /// client side files, plugins configuration and other settings.
    /// </summary>
    public abstract class WebPortalConfigurationManager
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WebPortalConfigurationManager"/> class.
        /// </summary>
        /// <param name="configurationFilePath">The web portal configuration file path.</param>
        protected WebPortalConfigurationManager(string configurationFilePath)
        {
            if (string.IsNullOrWhiteSpace(configurationFilePath))
            {
                throw new ArgumentException("configurationFilePath not set", nameof(configurationFilePath));
            }

            using (StreamReader configReader = new StreamReader(configurationFilePath))
            {
                // read and process the configuration
                this.Configuration = JsonConvert.DeserializeObject<WebPortalConfiguration>(configReader.ReadToEnd());
                this.Configuration.Process();
            }
        }

        /// <summary>
        /// Gets or sets the web portal configuration.
        /// </summary>
        protected WebPortalConfiguration Configuration { get; set; }

        /// <summary>
        /// Aggregates client side files which are needed during the portal start up.
        /// </summary>
        /// <returns>The aggregated startup assets.</returns>
        public abstract Task<Assets> AggregateStartupAssets();

        /// <summary>
        /// Aggregates client side files which are needed once the portal is up and running.
        /// </summary>
        /// <returns>The aggregated non startup assets.</returns>
        public abstract Task<Assets> AggregateNonStartupAssets();

        /// <summary>
        /// Generates the plugins which will be sent down to the client.
        /// </summary>
        /// <returns>The plugins configuration.</returns>
        public abstract Task<PluginsSegment> GeneratePlugins();

        /// <summary>
        /// Generates the configuration settings which will be sent down to the client.
        /// </summary>
        /// <returns>A dictionary of configuration settings.</returns>
        public abstract Task<Dictionary<string, dynamic>> GenerateConfigurationDictionary();

        /// <summary>
        /// Updates the web portal bundle files. This controls which files are bundled and sent to the client browser.
        /// </summary>
        /// <param name="bundler">The bundler instance.</param>
        /// <returns>A task which is complete when the bundles are updated.</returns>
        public async virtual Task UpdateBundles(Bundler bundler)
        {
            if (bundler == null)
            {
                throw new ArgumentNullException(nameof(bundler), "null bundler passed in");
            }

            bundler.Clear();

            Assets startUpAssets = await AggregateStartupAssets().ConfigureAwait(false);
            Assets nonStartUpAssets = await AggregateNonStartupAssets().ConfigureAwait(false);

            // build the start up javascript and css files and bundle them
            List<string> startupClasses = new List<string>(startUpAssets.JavaScript);
            List<string> startupStyles = new List<string>(startUpAssets.Css);
            bundler.BundleStartupAssets(startupClasses.ToArray(), startupStyles.ToArray());

            // build the non startup files and bundle them
            List<string> nonStartupClasses = new List<string>(nonStartUpAssets.JavaScript);
            List<string> nonStartupStyles = new List<string>(nonStartUpAssets.Css);
            bundler.BundleNonStartupAssets(nonStartupClasses.ToArray(), nonStartupStyles.ToArray());
        }
    }
}