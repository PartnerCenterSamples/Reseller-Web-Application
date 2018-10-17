// -----------------------------------------------------------------------
// <copyright file="WebPortalConfiguration.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Storefront.Configuration.WebPortal
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Holds the Web portal configuration.
    /// </summary>
    public class WebPortalConfiguration
    {
        /// <summary>
        /// The dependencies assets segment.
        /// </summary>
        private AssetsSegment dependencies;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebPortalConfiguration"/> class.
        /// </summary>
        public WebPortalConfiguration()
        {
            Configuration = new Dictionary<string, dynamic>();
        }

        /// <summary>
        /// Gets or sets the portal dependencies.
        /// </summary>
        public AssetsSegment Dependencies
        {
            get => this.dependencies;

            set
            {
                this.dependencies = value;
                this.dependencies.Name = "Dependencies";
            }
        }

        /// <summary>
        /// Gets or sets the portal core assets.
        /// </summary>
        public CoreSegment Core { get; set; }

        /// <summary>
        /// Gets or sets the portal services assets.
        /// </summary>
        public IEnumerable<AssetsSegment> Services { get; set; }

        /// <summary>
        /// Gets or sets the portal views assets.
        /// </summary>
        public IEnumerable<AssetsSegment> Views { get; set; }

        /// <summary>
        /// Gets or sets the portal plugins assets.
        /// </summary>
        public PluginsSegment Plugins { get; set; }

        /// <summary>
        /// Gets or sets the portal configuration.
        /// </summary>
        public Dictionary<string, dynamic> Configuration { get; }

        /// <summary>
        /// Processes the configuration and ensures it is valid.
        /// </summary>
        /// <exception cref="InvalidOperationException">If the configuration is invalid.</exception>
        public void Process()
        {
            if (this.Dependencies != null)
            {
                this.Dependencies.Validate();
            }

            if (this.Core == null || (this.Core.Startup == null && this.Core.NonStartup == null))
            {
                throw new InvalidOperationException("Portal core not present.");
            }

            if (this.Core.Startup != null)
            {
                this.Core.Startup.Validate();
            }

            if (this.Core.NonStartup != null)
            {
                this.Core.NonStartup.Validate();
            }

            if (this.Services != null)
            {
                foreach (AssetsSegment service in this.Services)
                {
                    service.Validate();
                }
            }

            if (this.Views != null)
            {
                foreach (AssetsSegment view in this.Views)
                {
                    view.Validate();
                }
            }

            this.Plugins.Validate();
        }
    }
}