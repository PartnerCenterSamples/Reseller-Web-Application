// -----------------------------------------------------------------------
// <copyright file="PartnerOfferController.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Storefront.Controllers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web.Http;
    using BusinessLogic;
    using Models;

    /// <summary>
    /// Serves partner offers to callers.
    /// </summary>
    [RoutePrefix("api/partnerOffers")]
    public class PartnerOfferController : BaseController
    {
        /// <summary>
        /// Retrieves all the active offers the partner has configured.
        /// </summary>
        /// <returns>The active partner offers.</returns>
        [Route("")]
        [HttpGet]
        public async Task<OfferCatalogViewModel> GetOffersCatalog()
        {
            bool isBrandingConfigured = await ApplicationDomain.Instance.PortalBranding.IsConfiguredAsync().ConfigureAwait(false);
            bool isOffersConfigured = await ApplicationDomain.Instance.OffersRepository.IsConfiguredAsync().ConfigureAwait(false);
            bool isPaymentConfigured = await ApplicationDomain.Instance.PaymentConfigurationRepository.IsConfiguredAsync().ConfigureAwait(false);

            IEnumerable<MicrosoftOffer> microsoftOffers = await ApplicationDomain.Instance.OffersRepository.RetrieveMicrosoftOffersAsync().ConfigureAwait(false);
            IEnumerable<PartnerOffer> partnerOffers = await ApplicationDomain.Instance.OffersRepository.RetrieveAsync().ConfigureAwait(false);


            OfferCatalogViewModel offerCatalogViewModel = new OfferCatalogViewModel
            {
                IsPortalConfigured = isBrandingConfigured && isOffersConfigured && isPaymentConfigured
            };

            if (offerCatalogViewModel.IsPortalConfigured)
            {
                foreach (PartnerOffer offer in partnerOffers)
                {
                    // TODO :: Handle Microsoft offer being pulled back due to EOL. 
                    MicrosoftOffer microsoftOfferItem = microsoftOffers.FirstOrDefault(msOffer => msOffer.Offer.Id == offer.MicrosoftOfferId);

                    // temporarily remove the partner offer from catalog display if the corresponding Microsoft offer does not exist. 
                    if (microsoftOfferItem != null)
                    {
                        offer.Thumbnail = microsoftOfferItem.ThumbnailUri;
                    }
                    else
                    {
                        // temporary fix - remove the items from the collection by marking it as Inactive.
                        offer.IsInactive = true;
                    }
                }

                offerCatalogViewModel.Offers = partnerOffers.Where(offer => !offer.IsInactive);
            }

            return offerCatalogViewModel;
        }
    }
}