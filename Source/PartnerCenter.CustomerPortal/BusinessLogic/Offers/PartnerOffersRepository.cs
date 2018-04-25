// -----------------------------------------------------------------------
// <copyright file="PartnerOffersRepository.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.CustomerPortal.BusinessLogic.Offers
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
            return (await this.RetrieveAsync()).Where(offer => offer.IsInactive == false).Count() > 0;
        }

        /// <summary>
        /// Fetches all Microsoft CSP offers.
        /// </summary>
        /// <returns>A list of all Microsoft CSP offers.</returns>
        public async Task<IEnumerable<MicrosoftOffer>> RetrieveMicrosoftOffersAsync()
        {
            var microsoftOffers = await this.ApplicationDomain.CachingService
                .FetchAsync<List<MicrosoftOffer>>(PartnerOffersRepository.MicrosoftOffersCacheKey);

            if (microsoftOffers == null)
            {
                // Need to manage this based on the offer locale supported by the Offer API. Either its english or using one of the supported offer locale to retrieve localized offers for the store front.
                var localeSpecificPartnerCenterClient = this.ApplicationDomain.PartnerCenterClient.With(RequestContextFactory.Instance.Create(this.ApplicationDomain.PortalLocalization.OfferLocale));

                // Offers.ByCountry is required to pull country / region specific offers. 
                var partnerCenterOffers = await localeSpecificPartnerCenterClient.Offers.ByCountry(this.ApplicationDomain.PortalLocalization.CountryIso2Code).GetAsync();

                var eligibleOffers = partnerCenterOffers?.Items.Where(offer =>
                    !offer.IsAddOn &&
                    (offer.PrerequisiteOffers == null || offer.PrerequisiteOffers.Count() <= 0)
                    && offer.IsAvailableForPurchase == true);

                microsoftOffers = new List<MicrosoftOffer>();

                foreach (var partnerCenterOffer in eligibleOffers)
                {
                    microsoftOffers.Add(new MicrosoftOffer()
                    {
                        Offer = partnerCenterOffer,
                        ThumbnailUri = new Uri(await this.ApplicationDomain.MicrosoftOfferLogoIndexer.GetOfferLogoUriAsync(partnerCenterOffer), UriKind.Relative)
                    });
                }

                // cache the Microsoft offers for one day
                await this.ApplicationDomain.CachingService.StoreAsync<List<MicrosoftOffer>>(
                    PartnerOffersRepository.MicrosoftOffersCacheKey,
                    microsoftOffers,
                    TimeSpan.FromDays(1));
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
            PartnerOffer matchingPartnerOffer = (await this.RetrieveAsync()).Where(offer => offer.Id == partnerOfferId).FirstOrDefault();

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
            var partnerOffers = await this.ApplicationDomain.CachingService
                .FetchAsync<List<PartnerOffer>>(PartnerOffersRepository.PartnerOffersCacheKey);

            if (partnerOffers == null)
            {
                var partnerOffersBlob = await this.GetPartnerOffersBlobAsync();

                if (await partnerOffersBlob.ExistsAsync())
                {
                    // download the partner offer BLOB
                    MemoryStream partnerOffersStream = new MemoryStream();
                    await partnerOffersBlob.DownloadToStreamAsync(partnerOffersStream);
                    partnerOffersStream.Seek(0, SeekOrigin.Begin);

                    // deserialize the BLOB into a list of Partner offer objects
                    partnerOffers =
                        JsonConvert.DeserializeObject<List<PartnerOffer>>(await new StreamReader(partnerOffersStream).ReadToEndAsync());

                    if (partnerOffers != null && partnerOffers.Count > 0)
                    {
                        // apply business rules to the offers
                        PartnerOfferNormalizer offerNormalizer = new PartnerOfferNormalizer();

                        foreach (var partnerOffer in partnerOffers)
                        {
                            offerNormalizer.Normalize(partnerOffer);
                        }
                    }
                }

                partnerOffers = partnerOffers ?? new List<PartnerOffer>();

                // cache the partner offers
                await this.ApplicationDomain.CachingService.StoreAsync<List<PartnerOffer>>(
                    PartnerOffersRepository.PartnerOffersCacheKey,
                    partnerOffers);
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
            if (newPartnerOffer == null)
            {
                throw new ArgumentNullException(nameof(newPartnerOffer));
            }

            newPartnerOffer.Id = Guid.NewGuid().ToString();

            ICollection<PartnerOffer> allPartnerOffers = new List<PartnerOffer>(await this.RetrieveAsync());
            new PartnerOfferNormalizer().Normalize(newPartnerOffer);
            allPartnerOffers.Add(newPartnerOffer);

            await this.UpdateAsync(allPartnerOffers);

            return newPartnerOffer;
        }

        /// <summary>
        /// Updates an existing partner offer.
        /// </summary>
        /// <param name="partnerOfferUpdate">The partner offer to update.</param>
        /// <returns>The updated partner offer.</returns>
        public async Task<PartnerOffer> UpdateAsync(PartnerOffer partnerOfferUpdate)
        {
            if (partnerOfferUpdate == null)
            {
                throw new ArgumentNullException(nameof(partnerOfferUpdate));
            }

            IList<PartnerOffer> allPartnerOffers = new List<PartnerOffer>(await this.RetrieveAsync());
            new PartnerOfferNormalizer().Normalize(partnerOfferUpdate);

            var existingPartnerOffer = allPartnerOffers.Where(offer => offer.Id == partnerOfferUpdate.Id).FirstOrDefault();

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

            await this.UpdateAsync(allPartnerOffers);

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
                        
            ICollection<PartnerOffer> allPartnerOffers = new List<PartnerOffer>(await this.RetrieveAsync());

            // mark the provided offers are deleted
            var matchedOffers = allPartnerOffers.Where(offer => partnerOffersToDelete.Where(offerToDelete => offerToDelete.Id == offer.Id).FirstOrDefault() != null);

            foreach (var offerToDelete in matchedOffers)
            {
                offerToDelete.IsInactive = true;
            }

            return await this.UpdateAsync(allPartnerOffers.ToList());
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
                var partnerOffersBlob = await this.GetPartnerOffersBlobAsync();
                await partnerOffersBlob.UploadTextAsync(JsonConvert.SerializeObject(partnerOffers));

                // invalidate the cache, we do not update it to avoid race condition between web instances
                await this.ApplicationDomain.CachingService.ClearAsync(PartnerOffersRepository.PartnerOffersCacheKey);
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
            var portalAssetsBlobContainer = await this.ApplicationDomain.AzureStorageService.GetPrivateCustomerPortalAssetsBlobContainerAsync();

            return portalAssetsBlobContainer.GetBlockBlobReference(PartnerOffersRepository.PartnerOffersBlobName);
        }
    }
}