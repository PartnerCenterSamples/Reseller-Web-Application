// -----------------------------------------------------------------------
// <copyright file="PortalBranding.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.CustomerPortal.BusinessLogic
{
    using System;
    using System.IO;
    using System.Net.Mail;
    using System.Threading.Tasks;
    using Exceptions;
    using Models;
    using Newtonsoft.Json;
    using WindowsAzure.Storage.Blob;

    /// <summary>
    /// Implements business behavior for the portal branding configuration. Enables clients to read and update the branding.
    /// </summary>
    public class PortalBranding : DomainObject
    {
        /// <summary>
        /// The portal branding key in the cache.
        /// </summary>
        private const string PortalBrandingCacheKey = "PortalBranding";

        /// <summary>
        /// The Azure BLOB name for the portal branding.
        /// </summary>
        private const string PortalBrandingBlobName = "portalbranding";

        /// <summary>
        /// Initializes a new instance of the <see cref="PortalBranding"/> class.
        /// </summary>
        /// <param name="applicationDomain">An application domain instance.</param>
        public PortalBranding(ApplicationDomain applicationDomain) : base(applicationDomain)
        {
        }

        /// <summary>
        /// Checks if the portal branding had been configured or not.
        /// </summary>
        /// <returns>True if the branding was configured and stored, false otherwise.</returns>
        public async Task<bool> IsConfiguredAsync()
        {
            var portalBrandingBlob = await this.GetPortalBrandingBlobAsync();
            return await portalBrandingBlob.ExistsAsync();
        }

        /// <summary>
        /// Retrieves the portal branding.
        /// </summary>
        /// <returns>The portal branding information.</returns>
        public async Task<BrandingConfiguration> RetrieveAsync()
        {
            var portalBranding = await this.ApplicationDomain.CachingService
                .FetchAsync<BrandingConfiguration>(PortalBranding.PortalBrandingCacheKey);

            if (portalBranding == null)
            {
                var portalBrandingBlob = await this.GetPortalBrandingBlobAsync();
                portalBranding = new BrandingConfiguration();

                if (await portalBrandingBlob.ExistsAsync())
                {
                    portalBranding = JsonConvert.DeserializeObject<BrandingConfiguration>(await portalBrandingBlob.DownloadTextAsync());
                    await this.NormalizeAsync(portalBranding);
                }
                else
                {
                    // portal branding has not been configured yet
                    portalBranding.OrganizationName = Resources.DefaultOrganizationName;
                }

                // cache the portal branding
                await this.ApplicationDomain.CachingService.StoreAsync<BrandingConfiguration>(
                    PortalBranding.PortalBrandingCacheKey,
                    portalBranding);
            }

            return portalBranding;
        }

        /// <summary>
        /// Updates the portal branding.
        /// </summary>
        /// <param name="updatedBrandingConfiguration">The new portal branding.</param>
        /// <returns>The updated portal branding.</returns>
        public async Task<BrandingConfiguration> UpdateAsync(BrandingConfiguration updatedBrandingConfiguration)
        {
            updatedBrandingConfiguration.AssertNotNull(nameof(updatedBrandingConfiguration));

            await this.NormalizeAsync(updatedBrandingConfiguration);

            var portalBrandingBlob = await this.GetPortalBrandingBlobAsync();
            await portalBrandingBlob.UploadTextAsync(JsonConvert.SerializeObject(updatedBrandingConfiguration));

            // invalidate the cache, we do not update it to avoid race condition between web instances
            await this.ApplicationDomain.CachingService.ClearAsync(PortalBranding.PortalBrandingCacheKey);

            // re-initialize the telemetry service because the configuration might have changed.
            await this.ApplicationDomain.TelemetryService.InitializeAsync();

            return updatedBrandingConfiguration;
        }

        /// <summary>
        /// Applies business rules to <see cref="BrandingConfiguration"/> instances.
        /// </summary>
        /// <param name="brandingConfiguration">A branding configuration instance.</param>
        /// <returns>A task.</returns>
        private async Task NormalizeAsync(BrandingConfiguration brandingConfiguration)
        {
            brandingConfiguration.AssertNotNull(nameof(brandingConfiguration));

            brandingConfiguration.OrganizationName.AssertNotEmpty("OrganizationName");

            if (brandingConfiguration.ContactUs == null)
            {
                throw new PartnerDomainException(ErrorCode.InvalidInput, Resources.ContactUsSectionNotFound).AddDetail("Field", "ContactUs");
            }

            brandingConfiguration.ContactUs.Email.AssertNotEmpty("Email");

            try
            {
                new MailAddress(brandingConfiguration.ContactUs.Email);
            }
            catch (FormatException)
            {
                throw new PartnerDomainException(ErrorCode.InvalidInput, Resources.InvalidContactUsEmailAddress).AddDetail("Field", "ContactUs.Email");
            }

            try
            {
                brandingConfiguration.ContactUs.Phone.AssertNotEmpty("ContactUs.Phone");
            }
            catch (ArgumentException)
            {
                throw new PartnerDomainException(ErrorCode.InvalidInput, Resources.InvalidContactUsPhoneExceptionMessage).AddDetail("Field", "ContactUs.Phone");
            }

            if (brandingConfiguration.ContactSales == null)
            {
                // default the contact sales to the contact us information
                brandingConfiguration.ContactSales = new ContactUsInformation()
                {
                    Email = brandingConfiguration.ContactUs.Email,
                    Phone = brandingConfiguration.ContactUs.Phone
                };
            }
            else
            {
                if (string.IsNullOrWhiteSpace(brandingConfiguration.ContactSales.Email))
                {
                    brandingConfiguration.ContactSales.Email = brandingConfiguration.ContactUs.Email;
                }
                else
                {
                    try
                    {
                        new MailAddress(brandingConfiguration.ContactSales.Email);
                    }
                    catch (FormatException)
                    {
                        throw new PartnerDomainException(ErrorCode.InvalidInput, Resources.InvalidContactSalesEmailAddress).AddDetail("Field", "ContactSales.Email");
                    }
                }

                if (string.IsNullOrWhiteSpace(brandingConfiguration.ContactSales.Phone))
                {
                    brandingConfiguration.ContactSales.Phone = brandingConfiguration.ContactUs.Phone;
                }
                else
                {
                    try
                    {
                        brandingConfiguration.ContactSales.Phone.AssertNotEmpty("ContactSales.Phone");
                    }
                    catch (ArgumentException)
                    {
                        throw new PartnerDomainException(ErrorCode.InvalidInput, Resources.InvalidContactSalesPhoneExceptionMessage).AddDetail("Field", "ContactSales.Phone");
                    }
                }
            }

            if (brandingConfiguration.OrganizationLogoContent != null)
            {
                // there is an logo image specified, upload it to BLOB storage and setup the URI property to point to it
                brandingConfiguration.OrganizationLogo = await this.UploadStreamToBlobStorageAsync(
                    "OrganizationLogo",
                    brandingConfiguration.OrganizationLogoContent);

                brandingConfiguration.OrganizationLogoContent = null;
            }

            if (brandingConfiguration.HeaderImageContent != null)
            {
                // there is a header image specified, upload it to BLOB storage and setup the URI property to point to it
                brandingConfiguration.HeaderImage = await this.UploadStreamToBlobStorageAsync(
                    "HeaderImage",
                    brandingConfiguration.HeaderImageContent);

                brandingConfiguration.HeaderImageContent = null;
            }
        }

        /// <summary>
        /// Uploads a stream to a new asset BLOB.
        /// </summary>
        /// <param name="blobNamePrefix">The BLOB name prefix to use.</param>
        /// <param name="streamToUpload">The stream to be uploaded.</param>
        /// <returns>The uploaded BLOB's URI.</returns>
        private async Task<Uri> UploadStreamToBlobStorageAsync(string blobNamePrefix, Stream streamToUpload)
        {
            streamToUpload.AssertNotNull(nameof(streamToUpload));

            var blob = await this.ApplicationDomain.AzureStorageService.GenerateNewBlobReferenceAsync(
                await this.ApplicationDomain.AzureStorageService.GetPublicCustomerPortalAssetsBlobContainerAsync(),
                blobNamePrefix);

            await blob.UploadFromStreamAsync(streamToUpload);

            return blob.Uri;
        }

        /// <summary>
        /// Retrieves the portal branding BLOB reference.
        /// </summary>
        /// <returns>The portal branding BLOB.</returns>
        private async Task<CloudBlockBlob> GetPortalBrandingBlobAsync()
        {
            var portalAssetsBlobContainer = await this.ApplicationDomain.AzureStorageService.GetPrivateCustomerPortalAssetsBlobContainerAsync();

            return portalAssetsBlobContainer.GetBlockBlobReference(PortalBranding.PortalBrandingBlobName);
        }
    }
}