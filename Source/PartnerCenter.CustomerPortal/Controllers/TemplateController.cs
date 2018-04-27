// -----------------------------------------------------------------------
// <copyright file="TemplateController.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.CustomerPortal.Controllers
{
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using BusinessLogic;
    using BusinessLogic.Commerce.PaymentGateways;
    using Configuration;
    using Configuration.Manager;
    using Filters;
    using Filters.Mvc;

    /// <summary>
    /// Serves HTML templates to the browser.
    /// </summary>
    public class TemplateController : Controller
    {
        /// <summary>
        /// Serves the HTML template for the homepage presenter.
        /// </summary>
        /// <returns>The HTML template for the homepage presenter.</returns>
        [HttpGet]
        [OutputCache(NoStore = true, Duration = 0)]
        public ActionResult HomePage()
        {
            return this.PartialView();
        }

        /// <summary>
        /// Serves the HTML template for the offers presenter.
        /// </summary>
        /// <returns>The HTML template for the offers presenter.</returns>
        [HttpGet]
        [OutputCache(NoStore = true, Duration = 0)]
        public async Task<ActionResult> Home()
        {
            var portalBranding = await ApplicationDomain.Instance.PortalBranding.RetrieveAsync().ConfigureAwait(false);

            if (portalBranding.HeaderImage != null)
            {
                ViewBag.HeaderImage = portalBranding.HeaderImage.ToString();
            }

            if (portalBranding.PrivacyAgreement != null)
            {
                ViewBag.PrivacyAgreement = portalBranding.PrivacyAgreement.ToString();
            }

            var principal = this.HttpContext.User as CustomerPortalPrincipal;

            ViewBag.OrganizationName = portalBranding.OrganizationName;
            ViewBag.IsPortalAdmin = principal.IsPortalAdmin;            

            return this.PartialView();
        }

        /// <summary>
        /// Serves the HTML template for the customer registration presenter.
        /// </summary>
        /// <returns>The HTML template for the customer registration presenter.</returns>
        [HttpGet]
        [@Authorize(UserRole = UserRole.None)]
        [OutputCache(NoStore = true, Duration = 0)]
        public ActionResult CustomerRegistration()
        {
            return this.PartialView();
        }

        /// <summary>
        /// Serves the HTML template for the registration confirmation presenter.
        /// </summary>
        /// <returns>The HTML template for the registration confirmation presenter.</returns>
        [HttpGet]
        [@Authorize(UserRole = UserRole.None)]
        [OutputCache(NoStore = true, Duration = 0)]
        public ActionResult RegistrationConfirmation()
        {
            return this.PartialView();
        }

        /// <summary>
        /// Serves the HTML template for the process order presenter.
        /// </summary>
        /// <returns>The HTML template for the process order presenter.</returns>
        [HttpGet]        
        [OutputCache(NoStore = true, Duration = 0)]
        public ActionResult ProcessOrder()
        {            
            return this.PartialView();
        }

        /// <summary>
        /// Serves the HTML template for the customer account presenter.
        /// </summary>
        /// <returns>The HTML template for the customer account presenter.</returns>
        [HttpGet]
        [@Authorize(UserRole = UserRole.Customer)]
        [OutputCache(NoStore = true, Duration = 0)]
        public ActionResult CustomerAccount()
        {
            return this.PartialView();
        }
        
        /// <summary>
        /// Serves the HTML template for the subscriptions presenter.
        /// </summary>
        /// <returns>The HTML template for the subscriptions presenter.</returns>
        [HttpGet]
        [@Authorize(UserRole = UserRole.Customer)]
        [OutputCache(NoStore = true, Duration = 0)]
        public ActionResult Subscriptions()
        {
            return this.PartialView();
        }

        /// <summary>
        /// Serves the HTML template for the add subscriptions presenter.
        /// </summary>
        /// <returns>The HTML template for the add subscriptions presenter.</returns>
        [HttpGet]
        [@Authorize(UserRole = UserRole.Customer)]
        [OutputCache(NoStore = true, Duration = 0)]
        public ActionResult AddSubscriptions()
        {
            return this.PartialView();
        }

        /// <summary>
        /// Serves the HTML template for the update subscriptions presenter.
        /// </summary>
        /// <returns>The HTML template for the update subscriptions presenter.</returns>
        [HttpGet]
        [@Authorize(UserRole = UserRole.Customer)]
        [OutputCache(NoStore = true, Duration = 0)]
        public ActionResult UpdateSubscriptions()
        {
            ViewBag.CurrencySymbol = ApplicationDomain.Instance.PortalLocalization.CurrencySymbol;
            return this.PartialView();
        }

        /// <summary>
        /// Serves the HTML template for the update contact information presenter.
        /// </summary>
        /// <returns>The HTML template for the update contact information presenter.</returns>
        [HttpGet]
        [@Authorize(UserRole = UserRole.Customer)]
        [OutputCache(NoStore = true, Duration = 0)]
        public ActionResult UpdateContactInformation()
        {
            return this.PartialView();
        }

        /// <summary>
        /// Serves the HTML template for the update company information presenter.
        /// </summary>
        /// <returns>The HTML template for the update company information presenter.</returns>
        [HttpGet]
        [@Authorize(UserRole = UserRole.Customer)]
        [OutputCache(NoStore = true, Duration = 0)]
        public ActionResult UpdateCompanyInformation()
        {
            return this.PartialView();
        }

        /// <summary>
        /// Serves the HTML template for the admin console presenter.
        /// </summary>
        /// <returns>The HTML template for the admin console presenter.</returns>
        [HttpGet]
        [@Authorize(UserRole = UserRole.Partner)]
        [OutputCache(NoStore = true, Duration = 0)]
        public ActionResult AdminConsole()
        {
            return this.PartialView();
        }

        /// <summary>
        /// Serves the HTML template for the adding or updating offers.
        /// </summary>
        /// <returns>The HTML template for the add or update offer presenter.</returns>
        [@Authorize(UserRole = UserRole.Partner)]
        [OutputCache(NoStore = true, Duration = 0)]
        public ActionResult AddOrUpdateOffer()
        {
            return this.PartialView();
        }

        /// <summary>
        /// Serves the HTML template for the partner offer list.
        /// </summary>
        /// <returns>The HTML template for the partner offer list presenter.</returns>
        [@Authorize(UserRole = UserRole.Partner)]
        [OutputCache(NoStore = true, Duration = 0)]
        public ActionResult OfferList()
        {
            ViewBag.CurrencySymbol = ApplicationDomain.Instance.PortalLocalization.CurrencySymbol;
            return this.PartialView();
        }

        /// <summary>
        /// Serves the HTML templates for the branding setup page.
        /// </summary>
        /// <returns>The HTML templates for the branding setup page.</returns>
        [@Authorize(UserRole = UserRole.Partner)]
        [OutputCache(NoStore = true, Duration = 0)]
        public ActionResult BrandingSetup()
        {
            return this.PartialView();
        }

        /// <summary>
        /// Serves the HTML templates for the payment setup page.
        /// </summary>
        /// <returns>The HTML templates for the payment setup page.</returns>
        [@Authorize(UserRole = UserRole.Partner)]
        [OutputCache(NoStore = true, Duration = 0)]
        public ActionResult PaymentSetup()
        {
            return this.PartialView(PaymentGatewayConfig.GetPaymentConfigView());
        }

        /// <summary>
        /// Serves the HTML templates for the customer management setup page.
        /// </summary>
        /// <returns>The HTML templates for the customer management setup page.</returns>
        [@Authorize(UserRole = UserRole.Partner)]
        [OutputCache(NoStore = true, Duration = 0)]
        public ActionResult CustomerManagementSetup()
        {
            return this.PartialView();
        }

        /// <summary>
        /// Serves the HTML templates for the framework controls and services.
        /// </summary>
        /// <returns>The HTML template for the framework controls and services.</returns>
        [HttpGet]
        [OutputCache(NoStore = true, Duration = 0)]
        public async Task<ActionResult> FrameworkFragments()
        {
            WebPortalConfigurationManager builder = ApplicationConfiguration.WebPortalConfigurationManager;
            ViewBag.Templates = (await builder.AggregateNonStartupAssets().ConfigureAwait(false)).Templates;

            var portalBranding = await ApplicationDomain.Instance.PortalBranding.RetrieveAsync().ConfigureAwait(false);

            ViewBag.OrganizationName = portalBranding.OrganizationName;

            if (portalBranding.OrganizationLogo != null)
            {
                ViewBag.OrganizationLogo = portalBranding.OrganizationLogo.ToString();
            }

            ViewBag.ContactUs = portalBranding.ContactUs;
            ViewBag.ContactSales = portalBranding.ContactSales;

            ViewBag.CurrencySymbol = ApplicationDomain.Instance.PortalLocalization.CurrencySymbol;

            return this.PartialView();
        }
    }
}