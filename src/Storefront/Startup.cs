// -----------------------------------------------------------------------
// <copyright file="Startup.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

[assembly: Microsoft.Owin.OwinStartupAttribute(typeof(Microsoft.Store.PartnerCenter.Storefront.Startup))]

namespace Microsoft.Store.PartnerCenter.Storefront
{
    using global::Owin;

    /// <summary>
    /// Manages the application start up.
    /// </summary>
    public partial class Startup
    {
        /// <summary>
        /// Configured the application.
        /// </summary>
        /// <param name="application">The application.</param>
        public void Configuration(IAppBuilder application)
        {
            this.ConfigureAuth(application);
        }
    }
}
