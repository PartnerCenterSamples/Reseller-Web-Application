// -----------------------------------------------------------------------
// <copyright file="PartnerOffersRepository.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Storefront.BusinessLogic.Offers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Exceptions;
    using Models;
    using Newtonsoft.Json;
    using RequestContext;
    using WindowsAzure.Storage.Blob;

    /// <summary>
    /// Encapsulates the behavior of offers a partner has configured to sell to their customers.
    /// </summary>
    public class PartnerOffersRepository : DomainObject
    {
        /// <summary>
        /// The Microsoft offers key in the cache.
        /// </summary>
        private const string MicrosoftOffersCacheKey = "MicrosoftOffers";

        /// <summary>
        /// The partner offers key in the cache.
        /// </summary>
        private const string PartnerOffersCacheKey = "PartnerOffers";

        /// <summary>
        /// The Azure BLOB name for the partner offers.
        /// </summary>
        private const string PartnerOffersBlobName = "partneroffers";

        /// <summary>
        /// Initializes a new instance of the <see cref="PartnerOffersRepository"/> class.
        /// </summary>
        /// <param name="applicationDomain">An application domain instance.</param>
        public PartnerOffersRepository(ApplicationDomain applicationDomain) : base(applicationDomain)
        {
        }

        /// <summary>
        /// Checks if the partner offers had been configured or not.
        /// </summary>
        /// <returns>True if the offers were configured and stored, false otherwise.</returns>
        public async Task<bool> IsConfiguredAsync()
        {
            return (await RetrieveAsync().ConfigureAwait(false)).Any(offer => !offer.IsInactive);
        }

        /// <summary>
        /// Fetches all Microsoft CSP offers.
        /// </summary>
        /// <returns>A list of all Microsoft CSP offers.</returns>
        public async Task<IEnumerable<MicrosoftOffer>> RetrieveMicrosoftOffersAsync()
        {
            List<MicrosoftOffer> microsoftOffers = await ApplicationDomain.CachingService
                .FetchAsync<List<MicrosoftOffer>>(MicrosoftOffersCacheKey).ConfigureAwait(false);

            if (microsoftOffers == null)
            {
                // Need to manage this based on the offer locale supported by the Offer API. Either its english or using one of the supported offer locale to retrieve localized offers for the store front.
                IPartner localeSpecificPartnerCenterClient = ApplicationDomain.PartnerCenterClient.With(RequestContextFactory.Instance.Create(ApplicationDomain.PortalLocalization.OfferLocale));

                // Offers.ByCountry is required to pull country / region specific offers. 
                PartnerCenter.Models.ResourceCollection<PartnerCenter.Models.Offers.Offer> partnerCenterOffers = await localeSpecificPartnerCenterClient.Offers.ByCountry(ApplicationDomain.PortalLocalization.CountryIso2Code).GetAsync().ConfigureAwait(false);

                // Get the current billing cycle to filter down the eligible offers
                BrandingConfiguration portalBranding = await ApplicationDomain.Instance.PortalBranding.RetrieveAsync().ConfigureAwait(false);
                PartnerCenter.Models.Offers.BillingCycleType offerBillingCycle = PartnerCenter.Models.Offers.BillingCycleType.Unknown;

                switch (portalBranding.BillingCycle)
                {
                    case BillingCycleType.Annual:
                        offerBillingCycle = PartnerCenter.Models.Offers.BillingCycleType.Monthly;
                        break;
                    case BillingCycleType.Monthly:
                        offerBillingCycle = PartnerCenter.Models.Offers.BillingCycleType.Annual;
                        break;
                    default:
                        throw new NotImplementedException($"Billing cycle {portalBranding.BillingCycle} is not implemented");
                }

                // Get offers which support the desired billing cycle
                IEnumerable<PartnerCenter.Models.Offers.Offer> eligibleOffers = partnerCenterOffers?.Items.Where(offer =>
                    !offer.IsAddOn &&
                    (offer.PrerequisiteOffers == null || !offer.PrerequisiteOffers.Any())
                    && offer.IsAvailableForPurchase
                    && offer.SupportedBillingCycles.Contains(offerBillingCycle));

                microsoftOffers = new List<MicrosoftOffer>();

                if (eligibleOffers != null)
                {
                    foreach (PartnerCenter.Models.Offers.Offer partnerCenterOffer in eligibleOffers)
                    {
                        microsoftOffers.Add(new MicrosoftOffer()
                        {
                            Offer = partnerCenterOffer,
                            ThumbnailUri = new Uri(await ApplicationDomain.MicrosoftOfferLogoIndexer.GetOfferLogoUriAsync(partnerCenterOffer).ConfigureAwait(false), UriKind.Relative)
                        });
                    }
                }

                // cache the Microsoft offers for one day
                await ApplicationDomain.CachingService.StoreAsync(
                    MicrosoftOffersCacheKey,
                    microsoftOffers,
                    TimeSpan.FromDays(1)).ConfigureAwait(false);
            }

            return microsoftOffers;
        }

        /// <summary>
        /// Retrieves a specific partner offer using its ID.
        /// </summary>
        /// <param name="partnerOfferId">The ID of the partner offer to look for.</param>
        /// <returns>The matching partner offer.</returns>
        public async Task<PartnerOffer> RetrieveAsync(string partnerOfferId)
        {
            partnerOfferId.AssertNotEmpty(nameof(partnerOfferId));
            PartnerOffer matchingPartnerOffer = (await RetrieveAsync().ConfigureAwait(false)).FirstOrDefault(offer => offer.Id == partnerOfferId);

            if (matchingPartnerOffer != null)
            {
                return matchingPartnerOffer;
            }
            else
            {
                throw new PartnerDomainException(ErrorCode.PartnerOfferNotFound, Resources.OfferNotFound);
            }
        }

        /// <summary>
        /// Retrieves all the partner offers from persistence.
        /// </summary>
        /// <returns>The partner offers.</returns>
        public async Task<IEnumerable<PartnerOffer>> RetrieveAsync()
        {
            List<PartnerOffer> partnerOffers = await ApplicationDomain.CachingService
                .FetchAsync<List<PartnerOffer>>(PartnerOffersCacheKey).ConfigureAwait(false);

            if (partnerOffers == null)
            {
                CloudBlockBlob partnerOffersBlob = await GetPartnerOffersBlobAsync().ConfigureAwait(false);

                if (await partnerOffersBlob.ExistsAsync().ConfigureAwait(false))
                {
                    // download the partner offer BLOB
                    MemoryStream partnerOffersStream = new MemoryStream();
                    await partnerOffersBlob.DownloadToStreamAsync(partnerOffersStream).ConfigureAwait(false);
                    partnerOffersStream.Seek(0, SeekOrigin.Begin);

                    // deserialize the BLOB into a list of Partner offer objects
                    partnerOffers =
                        JsonConvert.DeserializeObject<List<PartnerOffer>>(await new StreamReader(partnerOffersStream).ReadToEndAsync().ConfigureAwait(false));

                    if (partnerOffers != null && partnerOffers.Count > 0)
                    {
                        // apply business rules to the offers
                        PartnerOfferNormalizer offerNormalizer = new PartnerOfferNormalizer();

                        foreach (PartnerOffer partnerOffer in partnerOffers)
                        {
                            offerNormalizer.Normalize(partnerOffer);
                        }
                    }
                }

                partnerOffers = partnerOffers ?? new List<PartnerOffer>();

                // cache the partner offers
                await ApplicationDomain.CachingService.StoreAsync(
                    PartnerOffersCacheKey,
                    partnerOffers).ConfigureAwait(false);
            }

            return partnerOffers;
        }

        /// <summary>
        /// Adds a new partner offer to the repository.
        /// </summary>
        /// <param name="newPartnerOffer">The partner offer to add.</param>
        /// <returns>The added partner offer.</returns>
        public async Task<PartnerOffer> AddAsync(PartnerOffer newPartnerOffer)
        {
            newPartnerOffer.AssertNotNull(nameof(newPartnerOffer));

            newPartnerOffer.Id = Guid.NewGuid().ToString();

            ICollection<PartnerOffer> allPartnerOffers = new List<PartnerOffer>(await RetrieveAsync().ConfigureAwait(false));
            new PartnerOfferNormalizer().Normalize(newPartnerOffer);
            allPartnerOffers.Add(newPartnerOffer);

            await UpdateAsync(allPartnerOffers).ConfigureAwait(false);

            return newPartnerOffer;
        }

        /// <summary>
        /// Updates an existing partner offer.
        /// </summary>
        /// <param name="partnerOfferUpdate">The partner offer to update.</param>
        /// <returns>The updated partner offer.</returns>
        public async Task<PartnerOffer> UpdateAsync(PartnerOffer partnerOfferUpdate)
        {
            partnerOfferUpdate.AssertNotNull(nameof(partnerOfferUpdate));

            IList<PartnerOffer> allPartnerOffers = new List<PartnerOffer>(await RetrieveAsync().ConfigureAwait(false));
            new PartnerOfferNormalizer().Normalize(partnerOfferUpdate);

            PartnerOffer existingPartnerOffer = allPartnerOffers.FirstOrDefault(offer => offer.Id == partnerOfferUpdate.Id);

            if (existingPartnerOffer == null)
            {
                throw new PartnerDomainException(ErrorCode.PartnerOfferNotFound, Resources.OfferNotFound);
            }

            if (existingPartnerOffer.MicrosoftOfferId != partnerOfferUpdate.MicrosoftOfferId)
            {
                // we do not allow changing the Microsoft offer association since there may be existing purchases that purchased the original Microsoft offer
                throw new PartnerDomainException(ErrorCode.MicrosoftOfferImmutable, Resources.MicrosoftOfferImmutableErrorMessage);
            }

            allPartnerOffers[allPartnerOffers.IndexOf(existingPartnerOffer)] = partnerOfferUpdate;

            await UpdateAsync(allPartnerOffers).ConfigureAwait(false);

            return partnerOfferUpdate;
        }

        /// <summary>
        /// Marks the passed partner offers as deleted.
        /// </summary>
        /// <param name="partnerOffersToDelete">The partner offers to mark as deleted.</param>
        /// <returns>The updated partner offers.</returns>
        public async Task<IEnumerable<PartnerOffer>> MarkAsDeletedAsync(IEnumerable<PartnerOffer> partnerOffersToDelete)
        {
            partnerOffersToDelete.AssertNotNull(nameof(partnerOffersToDelete));

            ICollection<PartnerOffer> allPartnerOffers = new List<PartnerOffer>(await RetrieveAsync().ConfigureAwait(false));

            // mark the provided offers are deleted
            IEnumerable<PartnerOffer> matchedOffers = allPartnerOffers.Where(offer => partnerOffersToDelete.FirstOrDefault(offerToDelete => offerToDelete.Id == offer.Id) != null);

            foreach (PartnerOffer offerToDelete in matchedOffers)
            {
                offerToDelete.IsInactive = true;
            }

            return await UpdateAsync(allPartnerOffers.ToList()).ConfigureAwait(false);
        }

        /// <summary>
        /// Updates persistence with new partner offers. Existing offers will be wiped.
        /// </summary>
        /// <param name="partnerOffers">A collection of new partner offers.</param>
        /// <returns>The resulting partner offers.</returns>
        private async Task<IEnumerable<PartnerOffer>> UpdateAsync(ICollection<PartnerOffer> partnerOffers)
        {
            partnerOffers.AssertNotNull(nameof(partnerOffers));

            try
            {
                // overwrite the partner offers BLOB
                CloudBlockBlob partnerOffersBlob = await GetPartnerOffersBlobAsync().ConfigureAwait(false);
                await partnerOffersBlob.UploadTextAsync(JsonConvert.SerializeObject(partnerOffers)).ConfigureAwait(false);

                // invalidate the cache, we do not update it to avoid race condition between web instances
                await ApplicationDomain.CachingService.ClearAsync(PartnerOffersCacheKey).ConfigureAwait(false);
            }
            catch (Exception blobAccessProblem)
            {
                if (blobAccessProblem.IsFatal())
                {
                    throw;
                }

                throw new PartnerDomainException(ErrorCode.PersistenceFailure, Resources.FailedToUpdatePartnerOffersStore, blobAccessProblem);
            }

            // return the normalized offers
            return partnerOffers;
        }

        /// <summary>
        /// Retrieves the partner offers BLOB reference.
        /// </summary>
        /// <returns>The partner offers BLOB.</returns>
        private async Task<CloudBlockBlob> GetPartnerOffersBlobAsync()
        {
            CloudBlobContainer portalAssetsBlobContainer = await ApplicationDomain.AzureStorageService.GetPrivateCustomerPortalAssetsBlobContainerAsync().ConfigureAwait(false);

            return portalAssetsBlobContainer.GetBlockBlobReference(PartnerOffersBlobName);
        }
    }
}