// -----------------------------------------------------------------------
// <copyright file="Global.asax.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.CustomerPortal
{
    using System;
    using System.Configuration;
    using System.Diagnostics;
    using System.Web;
    using System.Web.Http;
    using System.Web.Mvc;
    using System.Web.Routing;
    using BusinessLogic;
    using Configuration;
    using Configuration.Bundling;
    using Configuration.Manager;

    /// <summary>
    /// The web application.
    /// </summary>
    public class MvcApplication : HttpApplication
    {
        /// <summary>
        /// Called when the application starts.
        /// </summary>
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            FilterConfig.RegisterGlobalMvcFilters(GlobalFilters.Filters);
            FilterConfig.RegisterWebApiFilters(GlobalConfiguration.Configuration.Filters);

            // configure the web portal client application
            string portalConfigurationPath = ApplicationConfiguration.WebPortalConfigurationFilePath;

            if (string.IsNullOrWhiteSpace(portalConfigurationPath))
            {
                throw new ConfigurationErrorsException("WebPortalConfigurationPath setting not found in web.config");
            }

            // intialize our application domain
            ApplicationDomain.InitializeAsync().Wait();

            // create the web portal configuration manager
            IWebPortalConfigurationFactory webPortalConfigFactory = new WebPortalConfigurationFactory();
            ApplicationConfiguration.WebPortalConfigurationManager = webPortalConfigFactory.Create(portalConfigurationPath);

            // setup the application assets bundles
            ApplicationConfiguration.WebPortalConfigurationManager.UpdateBundles(Bundler.Instance).Wait();
        }

        /// <summary>
        /// Fired when an uncaught exception escapes the pipeline.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">Event arguments.</param>
        protected void Application_Error(object sender, EventArgs e)
        {
            Exception exception = Server.GetLastError();

            if (exception != null)
            {
                Trace.TraceError("Application_Error: Uncaught exception: {0}", exception);
            }
        }
    }
}
