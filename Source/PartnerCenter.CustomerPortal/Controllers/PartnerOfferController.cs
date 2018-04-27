// -----------------------------------------------------------------------
// <copyright file="PartnerOfferController.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.CustomerPortal.Controllers
{
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
            var isBrandingConfigured = ApplicationDomain.Instance.PortalBranding.IsConfiguredAsync();
            var isOffersConfigured = ApplicationDomain.Instance.OffersRepository.IsConfiguredAsync();
            var isPaymentConfigured = ApplicationDomain.Instance.PaymentConfigurationRepository.IsConfiguredAsync();

            var getMicrosoftOffersTask = ApplicationDomain.Instance.OffersRepository.RetrieveMicrosoftOffersAsync();
            var getPartnerOffersTask = ApplicationDomain.Instance.OffersRepository.RetrieveAsync();

            await Task.WhenAll(isBrandingConfigured, isOffersConfigured, isPaymentConfigured).ConfigureAwait(false);

            var offerCatalogViewModel = new OfferCatalogViewModel
            {
                IsPortalConfigured = isBrandingConfigured.Result && isOffersConfigured.Result && isPaymentConfigured.Result
            };

            if (offerCatalogViewModel.IsPortalConfigured)
            {
                await Task.WhenAll(getMicrosoftOffersTask, getPartnerOffersTask).ConfigureAwait(false);

                var microsoftOffers = getMicrosoftOffersTask.Result;
                var partnerOffers = getPartnerOffersTask.Result.Where(offer => offer.IsInactive == false);

                foreach (var offer in partnerOffers)
                {
                    // TODO :: Handle Microsoft offer being pulled back due to EOL. 
                    var microsoftOfferItem = microsoftOffers.Where(msOffer => msOffer.Offer.Id == offer.MicrosoftOfferId).FirstOrDefault();

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

                offerCatalogViewModel.Offers = partnerOffers.Where(offer => offer.IsInactive == false);
            }

            return offerCatalogViewModel;
        }
    }
}