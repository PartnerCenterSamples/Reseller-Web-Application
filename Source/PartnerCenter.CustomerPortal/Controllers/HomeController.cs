// -----------------------------------------------------------------------
// <copyright file="HomeController.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.CustomerPortal.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using BusinessLogic;
    using Configuration;
    using Configuration.WebPortal;
    using Newtonsoft.Json;

    /// <summary>
    /// Manages the application home page requests.
    /// </summary>
    public class HomeController : Controller
    {
        /// <summary>
        /// Serves the single page application to the browser.
        /// </summary>
        /// <param name="form">form data</param>
        /// <returns>The SPA markup.</returns>
        public async Task<ActionResult> Index(FormCollection form)
        {
            try
            {
                // get a copy of the plugins and the client configuration
                PluginsSegment clientVisiblePlugins = await ApplicationConfiguration.WebPortalConfigurationManager.GeneratePlugins().ConfigureAwait(false);
                IDictionary<string, dynamic> clientConfiguration = new Dictionary<string, dynamic>(ApplicationConfiguration.ClientConfiguration);

                // configure the tiles to show and hide based on the logged in user role
                var principal = this.HttpContext.User as CustomerPortalPrincipal;

                clientVisiblePlugins.Plugins.Where(x => x.Name == "CustomerAccount").First().Hidden = !principal.IsPartnerCenterCustomer;
                clientVisiblePlugins.Plugins.Where(x => x.Name == "CustomerSubscriptions").First().Hidden = !principal.IsPartnerCenterCustomer;
                clientVisiblePlugins.Plugins.Where(x => x.Name == "AdminConsole").First().Hidden = !principal.IsPortalAdmin;
                clientVisiblePlugins.Plugins.Where(x => x.Name == "PartnerOffersSetup").First().Hidden = !principal.IsPortalAdmin;
                clientVisiblePlugins.Plugins.Where(x => x.Name == "BrandingSetup").First().Hidden = !principal.IsPortalAdmin;
                clientVisiblePlugins.Plugins.Where(x => x.Name == "PaymentSetup").First().Hidden = !principal.IsPortalAdmin;
                clientVisiblePlugins.Plugins.Where(x => x.Name == "CustomerManagementSetup").First().Hidden = !principal.IsPortalAdmin;

                if (principal.IsPortalAdmin)
                {
                    clientVisiblePlugins.DefaultPlugin = "AdminConsole";
                }
                else
                {
                    clientVisiblePlugins.DefaultPlugin = "Home";
                }

                clientConfiguration["DefaultTile"] = clientVisiblePlugins.DefaultPlugin;
                clientConfiguration["Tiles"] = clientVisiblePlugins.Plugins;

                ViewBag.Templates = (await ApplicationConfiguration.WebPortalConfigurationManager.AggregateStartupAssets().ConfigureAwait(false)).Templates;
                ViewBag.OrganizationName = (await ApplicationDomain.Instance.PortalBranding.RetrieveAsync().ConfigureAwait(false)).OrganizationName;
                ViewBag.IsAuthenticated = Request.IsAuthenticated ? "true" : "false";

                if (Request.IsAuthenticated)
                {
                    ViewBag.UserName = ((ClaimsIdentity)HttpContext.User.Identity).FindFirst("name").Value ?? "Unknown";
                    ViewBag.Email = ((ClaimsIdentity)HttpContext.User.Identity).FindFirst(ClaimTypes.Name)?.Value ??
                        ((ClaimsIdentity)HttpContext.User.Identity).FindFirst(ClaimTypes.Email)?.Value;
                }

                ViewBag.Configuratrion = JsonConvert.SerializeObject(
                    clientConfiguration,
                    new JsonSerializerSettings() { StringEscapeHandling = StringEscapeHandling.Default });

                if (Resources.Culture.TwoLetterISOLanguageName.ToLowerInvariant() != "en")
                {
                    ViewBag.ValidatorMessagesSrc = string.Format("https://ajax.aspnetcdn.com/ajax/jquery.validate/1.15.0/localization/messages_{0}.js", Resources.Culture.TwoLetterISOLanguageName);
                }

                if (form.Count > 0)
                {
                    ViewBag.paymentId = form["txnid"];
                    ViewBag.txnId = form["payuMoneyId"];
                }

                return this.View();
            }
            catch (Exception exception)
            {
                ViewBag.ErrorMessage = Resources.PortalStartupFailure;
                ViewBag.ErrorDetails = exception.Message;
                return this.View("Error");
            }
        }

        /// <summary>
        /// Displays an error page.
        /// </summary>
        /// <param name="errorMessage">The error message to display.</param>
        /// <returns>The error view.</returns>
        public async Task<ActionResult> Error(string errorMessage)
        {
            var portalBranding = await ApplicationDomain.Instance.PortalBranding.RetrieveAsync().ConfigureAwait(false);

            ViewBag.ErrorMessage = errorMessage;
            ViewBag.OrganizationName = portalBranding.OrganizationName;

            return this.View();
        }
    }
}