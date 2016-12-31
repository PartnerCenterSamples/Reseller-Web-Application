// -----------------------------------------------------------------------
// <copyright file="PartnerOfferController.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.CustomerPortal.Controllers
{
    using System;
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
            var startTime = DateTime.Now;
            var isBrandingConfigured = ApplicationDomain.Instance.PortalBranding.IsConfiguredAsync();
            var isOffersConfigured = ApplicationDomain.Instance.OffersRepository.IsConfiguredAsync();
            var isPaymentConfigured = ApplicationDomain.Instance.PaymentConfigurationRepository.IsConfiguredAsync();

            var getMicrosoftOffersTask = ApplicationDomain.Instance.OffersRepository.RetrieveMicrosoftOffersAsync();
            var getPartnerOffersTask = ApplicationDomain.Instance.OffersRepository.RetrieveAsync();

            await Task.WhenAll(isBrandingConfigured, isOffersConfigured, isPaymentConfigured);

            var offerCatalogViewModel = new OfferCatalogViewModel
            {
                IsPortalConfigured =
                    isBrandingConfigured.Result && isOffersConfigured.Result && isPaymentConfigured.Result
            };

            if (offerCatalogViewModel.IsPortalConfigured)
            {
                await Task.WhenAll(getMicrosoftOffersTask, getPartnerOffersTask);

                var microsoftOffers = getMicrosoftOffersTask.Result;
                var partnerOffers = getPartnerOffersTask.Result.Where(offer => offer.IsInactive == false);

                foreach (var offer in partnerOffers)
                {
                    // TODO :: Handle Microsoft offer being pulled back due to EOL. 
                    var microsoftOfferItem = microsoftOffers.FirstOrDefault(msOffer => msOffer.Offer.Id == offer.MicrosoftOfferId);

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

            // Capture the request for the customer summary for analysis.
            var eventProperties = new Dictionary<string, string> { { "IsPortalConfigured", offerCatalogViewModel.IsPortalConfigured.ToString() } };

            // Track the event measurements for analysis.
            var eventMetrics = new Dictionary<string, double> { { "ElapsedMilliseconds", DateTime.Now.Subtract(startTime).TotalMilliseconds }, { "NumberOfOffers", offerCatalogViewModel.Offers.Count() } };

            ApplicationDomain.Instance.TelemetryService.Provider.TrackEvent("api/partnerOffers", eventProperties, eventMetrics);

            return offerCatalogViewModel;
        }
    }
}