// -----------------------------------------------------------------------
// <copyright file="CoreSegment.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Storefront.Configuration.WebPortal
{
    /// <summary>
    /// A container for portal core asset segments.
    /// </summary>
    public class CoreSegment
    {
        /// <summary>
        /// The start up assets segment.
        /// </summary>
        private AssetsSegment startup;

        /// <summary>
        /// The non start up assets segment.
        /// </summary>
        private AssetsSegment nonStartup;

        /// <summary>
        /// Gets or sets startup assets.
        /// </summary>
        public AssetsSegment Startup
        {
            get => this.startup;

            set
            {
                this.startup = value;
                this.startup.Name = "Startup";
            }
        }

        /// <summary>
        /// Gets or sets non startup assets.
        /// </summary>
        public AssetsSegment NonStartup
        {
            get => this.nonStartup;

            set
            {
                this.nonStartup = value;
                this.nonStartup.Name = "Nonstartup";
            }
        }
    }
}