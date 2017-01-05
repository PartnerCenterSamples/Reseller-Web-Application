// -----------------------------------------------------------------------
// <copyright file="AdminConsoleController.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.CustomerPortal.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Http;
    using BusinessLogic;
    using BusinessLogic.Commerce.PaymentGateways;
    using BusinessLogic.Exceptions;
    using CustomerPortal;
    using Filters;
    using Filters.WebApi;
    using Models;

    /// <summary>
    /// Serves data to the Admin dashboard pages.
    /// </summary>
    [RoutePrefix("api/AdminConsole")]
    [@Authorize(UserRole = UserRole.Partner)]
    public class AdminConsoleController : BaseController
    {
        /// <summary>
        /// Retrieves the admin console status.
        /// </summary>
        /// <returns>The admin console status.</returns>
        [HttpGet]
        public async Task<AdminConsoleViewModel> GetAdminConsoleStatus()
        {
            AdminConsoleViewModel adminConsoleViewModel = new AdminConsoleViewModel
            {
                IsOffersConfigured = await ApplicationDomain.Instance.OffersRepository.IsConfiguredAsync(),
                IsBrandingConfigured = await ApplicationDomain.Instance.PortalBranding.IsConfiguredAsync(),
                IsPaymentConfigured = await ApplicationDomain.Instance.PaymentConfigurationRepository.IsConfiguredAsync()
            };

            return adminConsoleViewModel;
        }

        /// <summary>
        /// Retrieves the partner's branding configuration.
        /// </summary>
        /// <returns>The partner's branding configuration.</returns>
        [Route("Branding")]
        [HttpGet]
        public async Task<BrandingConfiguration> GetBrandingConfiguration()
        {
            return await ApplicationDomain.Instance.PortalBranding.RetrieveAsync();
        }

        /// <summary>
        /// Updates the website's branding.
        /// </summary>
        /// <returns>The updated branding information.</returns>
        [Route("Branding")]
        [HttpPost]
        public async Task<BrandingConfiguration> UpdateBrandingConfiguration()
        {
            BrandingConfiguration brandingConfiguration = new BrandingConfiguration()
            {
                OrganizationName = HttpContext.Current.Request.Form["OrganizationName"],
                ContactUs = new ContactUsInformation
                {
                    Email = HttpContext.Current.Request.Form["ContactUsEmail"],
                    Phone = HttpContext.Current.Request.Form["ContactUsPhone"],
                },
                ContactSales = new ContactUsInformation
                {
                    Email = HttpContext.Current.Request.Form["ContactSalesEmail"],
                    Phone = HttpContext.Current.Request.Form["ContactSalesPhone"],
                },
            };

            string organizationLogo = HttpContext.Current.Request.Form["OrganizationLogo"];
            var organizationLogoPostedFile = HttpContext.Current.Request.Files["OrganizationLogoFile"];

            if (organizationLogoPostedFile != null && Path.GetFileName(organizationLogoPostedFile.FileName) == organizationLogo)
            {
                // there is a new organization logo to be uploaded
                if (!organizationLogoPostedFile.ContentType.Trim().StartsWith("image/"))
                {
                    throw new PartnerDomainException(ErrorCode.InvalidFileType, Resources.InvalidOrganizationLogoFileTypeMessage).AddDetail("Field", "OrganizationLogoFile");
                }

                brandingConfiguration.OrganizationLogoContent = organizationLogoPostedFile.InputStream;
            }
            else if (!string.IsNullOrWhiteSpace(organizationLogo))
            {
                try
                {
                    // the user either did not specify a logo or he did but changed the organization logo text to point to somewhere else i.e. a URI
                    brandingConfiguration.OrganizationLogo = new Uri(organizationLogo, UriKind.Absolute);
                }
                catch (UriFormatException invalidUri)
                {
                    throw new PartnerDomainException(ErrorCode.InvalidInput, Resources.InvalidOrganizationLogoUriMessage, invalidUri).AddDetail("Field", "OrganizationLogo");
                }
            }

            string headerImage = HttpContext.Current.Request.Form["HeaderImage"];
            var headerImageUploadPostedFile = HttpContext.Current.Request.Files["HeaderImageFile"];

            if (headerImageUploadPostedFile != null && Path.GetFileName(headerImageUploadPostedFile.FileName) == headerImage)
            {
                // there is a new header image to be uploaded
                if (!headerImageUploadPostedFile.ContentType.Trim().StartsWith("image/"))
                {
                    throw new PartnerDomainException(ErrorCode.InvalidFileType, Resources.InvalidHeaderImageMessage).AddDetail("Field", "HeaderImageFile");
                }

                brandingConfiguration.HeaderImageContent = headerImageUploadPostedFile.InputStream;
            }
            else if (!string.IsNullOrWhiteSpace(headerImage))
            {
                try
                {
                    // the user either did not specify a header image or he did but changed the organization logo text to point to somewhere else i.e. a URI
                    brandingConfiguration.HeaderImage = new Uri(headerImage, UriKind.Absolute);
                }
                catch (UriFormatException invalidUri)
                {
                    throw new PartnerDomainException(ErrorCode.InvalidInput, Resources.InvalidHeaderImageUriMessage, invalidUri).AddDetail("Field", "HeaderImage");
                }
            }

            if (!string.IsNullOrWhiteSpace(HttpContext.Current.Request.Form["PrivacyAgreement"]))
            {
                try
                {
                    brandingConfiguration.PrivacyAgreement = new Uri(HttpContext.Current.Request.Form["PrivacyAgreement"], UriKind.Absolute);
                }
                catch (UriFormatException invalidUri)
                {
                    throw new PartnerDomainException(ErrorCode.InvalidInput, Resources.InvalidPrivacyUriMessage, invalidUri).AddDetail("Field", "PrivacyAgreement");
                }
            }

            if (!string.IsNullOrWhiteSpace(HttpContext.Current.Request.Form["InstrumentationKey"]))
            {
                brandingConfiguration.InstrumentationKey = HttpContext.Current.Request.Form["InstrumentationKey"];
            }

            var updatedBrandingConfiguration = await ApplicationDomain.Instance.PortalBranding.UpdateAsync(brandingConfiguration);
            bool isPaymentConfigurationSetup = await ApplicationDomain.Instance.PaymentConfigurationRepository.IsConfiguredAsync();
            if (isPaymentConfigurationSetup)
            {
                // update the web experience profile. 
                var paymentConfiguration = await ApplicationDomain.Instance.PaymentConfigurationRepository.RetrieveAsync();
                paymentConfiguration.WebExperienceProfileId = PayPalGateway.CreateWebExperienceProfile(paymentConfiguration, updatedBrandingConfiguration, ApplicationDomain.Instance.PortalLocalization.CountryIso2Code);
                await ApplicationDomain.Instance.PaymentConfigurationRepository.UpdateAsync(paymentConfiguration);
            }

            return updatedBrandingConfiguration;
        }

        /// <summary>
        /// Retrieves all active offers the partner has configured.
        /// </summary>
        /// <returns>The active partner offers.</returns>
        [Route("Offers")]
        [HttpGet]
        public async Task<IEnumerable<PartnerOffer>> GetOffers()
        {
            return (await ApplicationDomain.Instance.OffersRepository.RetrieveAsync()).Where(offer => offer.IsInactive == false);
        }

        /// <summary>
        /// Adds a new partner offer.
        /// </summary>
        /// <param name="newPartnerOffer">The new partner offer to add.</param>
        /// <returns>The new partner offer details.</returns>
        [Route("Offers")]
        [HttpPost]
        public async Task<PartnerOffer> AddOffer(PartnerOffer newPartnerOffer)
        {
            return await ApplicationDomain.Instance.OffersRepository.AddAsync(newPartnerOffer);
        }

        /// <summary>
        /// Updates a partner offer.
        /// </summary>
        /// <param name="partnerOffer">The partner offer to update.</param>
        /// <returns>The updated partner offer.</returns>
        [Route("Offers")]
        [HttpPut]
        public async Task<PartnerOffer> UpdateOffer(PartnerOffer partnerOffer)
        {
            return await ApplicationDomain.Instance.OffersRepository.UpdateAsync(partnerOffer);
        }

        /// <summary>
        /// Deletes partner offers.
        /// </summary>
        /// <param name="partnerOffersToDelete">The partner offers to delete.</param>
        /// <returns>The updated partner offers after deletion.</returns>
        [Route("Offers/Delete")]
        [HttpPost]
        public async Task<IEnumerable<PartnerOffer>> DeleteOffers(List<PartnerOffer> partnerOffersToDelete)
        {
            return (await ApplicationDomain.Instance.OffersRepository.MarkAsDeletedAsync(partnerOffersToDelete)).Where(offer => offer.IsInactive == false);
        }

        /// <summary>
        /// Retrieves all the Microsoft CSP offers.
        /// </summary>
        /// <returns>A list of Microsoft CSP offers.</returns>
        [HttpGet]
        [Route("MicrosoftOffers")]
        public async Task<IEnumerable<MicrosoftOffer>> GetMicrosoftOffers()
        {
            return await ApplicationDomain.Instance.OffersRepository.RetrieveMicrosoftOffersAsync();
        }

        /// <summary>
        /// Retrieves the portal's payment configuration.
        /// </summary>
        /// <returns>The portal's payment configuration.</returns>
        [HttpGet]
        [Route("Payment")]
        public async Task<PaymentConfiguration> GetPaymentConfiguration()
        {
            return await ApplicationDomain.Instance.PaymentConfigurationRepository.RetrieveAsync();
        }

        /// <summary>
        /// Updates the the portal's payment configuration.
        /// </summary>
        /// <param name="paymentConfiguration">The new portal's payment configuration.</param>
        /// <returns>The updated portal's payment configuration.</returns>
        [Route("Payment")]
        [HttpPut]
        public async Task<PaymentConfiguration> UpdatePaymentConfiguration(PaymentConfiguration paymentConfiguration)
        {
            // validate the payment configuration before saving. 
            PayPalGateway.ValidateConfiguration(paymentConfiguration);

            // check if branding configuration has been setup else don't create web experience profile. 
            bool isBrandingConfigured = await ApplicationDomain.Instance.PortalBranding.IsConfiguredAsync();
            if (isBrandingConfigured)
            {
                // create a web experience profile using the branding for the web store. 
                BrandingConfiguration brandConfig = await ApplicationDomain.Instance.PortalBranding.RetrieveAsync();
                paymentConfiguration.WebExperienceProfileId = PayPalGateway.CreateWebExperienceProfile(paymentConfiguration, brandConfig, ApplicationDomain.Instance.PortalLocalization.CountryIso2Code);
            }

            // Save the validated & complete payment configuration to repository.
            PaymentConfiguration paymentConfig = await ApplicationDomain.Instance.PaymentConfigurationRepository.UpdateAsync(paymentConfiguration);

            return paymentConfig;
        }
    }
}