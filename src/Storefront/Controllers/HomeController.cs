// -----------------------------------------------------------------------
// <copyright file="HomeController.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Storefront.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
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
                PluginsSegment clientVisiblePlugins = ApplicationConfiguration.WebPortalConfigurationManager.GeneratePlugins();
                IDictionary<string, dynamic> clientConfiguration = new Dictionary<string, dynamic>(ApplicationConfiguration.ClientConfiguration);

                // configure the tiles to show and hide based on the logged in user role
                CustomerPortalPrincipal principal = HttpContext.User as CustomerPortalPrincipal;

                clientVisiblePlugins.Plugins.First(x => x.Name == "CustomerAccount").Hidden = !principal.IsPartnerCenterCustomer;
                clientVisiblePlugins.Plugins.First(x => x.Name == "CustomerSubscriptions").Hidden = !principal.IsPartnerCenterCustomer;
                clientVisiblePlugins.Plugins.First(x => x.Name == "AdminConsole").Hidden = !principal.IsPortalAdmin;
                clientVisiblePlugins.Plugins.First(x => x.Name == "PartnerOffersSetup").Hidden = !principal.IsPortalAdmin;
                clientVisiblePlugins.Plugins.First(x => x.Name == "BrandingSetup").Hidden = !principal.IsPortalAdmin;
                clientVisiblePlugins.Plugins.First(x => x.Name == "PaymentSetup").Hidden = !principal.IsPortalAdmin;
                clientVisiblePlugins.Plugins.First(x => x.Name == "CustomerManagementSetup").Hidden = !principal.IsPortalAdmin;

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

                ViewBag.Templates = ApplicationConfiguration.WebPortalConfigurationManager.AggregateStartupAssets().Templates;
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

                if (!Resources.Culture.TwoLetterISOLanguageName.Equals("en", StringComparison.InvariantCultureIgnoreCase))
                {
                    ViewBag.ValidatorMessagesSrc = string.Format(CultureInfo.InvariantCulture, "https://ajax.aspnetcdn.com/ajax/jquery.validate/1.15.0/localization/messages_{0}.js", Resources.Culture.TwoLetterISOLanguageName);
                }

                if (form.Count > 0)
                {
                    ViewBag.paymentId = form["txnid"];
                    ViewBag.txnId = form["payuMoneyId"];
                }

                return View();
            }
            catch (Exception exception)
            {
                ViewBag.ErrorMessage = Resources.PortalStartupFailure;
                ViewBag.ErrorDetails = exception.Message;
                return View("Error");
            }
        }

        /// <summary>
        /// Displays an error page.
        /// </summary>
        /// <param name="errorMessage">The error message to display.</param>
        /// <returns>The error view.</returns>
        public async Task<ActionResult> Error(string errorMessage)
        {
            Models.BrandingConfiguration portalBranding = await ApplicationDomain.Instance.PortalBranding.RetrieveAsync().ConfigureAwait(false);

            ViewBag.ErrorMessage = errorMessage;
            ViewBag.OrganizationName = portalBranding.OrganizationName;

            return View();
        }
    }
}