// -----------------------------------------------------------------------
// <copyright file="RouteConfig.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.CustomerPortal
{
    using System.Web.Mvc;
    using System.Web.Routing;

    /// <summary>
    /// Holds routing configuration.
    /// </summary>
    public class RouteConfig
    {
        /// <summary>
        /// Registers routes.
        /// </summary>
        /// <param name="routes">A collection of routes.</param>
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional });
        }
    }
}
