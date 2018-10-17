// -----------------------------------------------------------------------
// <copyright file="MicrosoftOfferLogoIndexer.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Storefront.BusinessLogic.Offers
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using PartnerCenter.Models.Offers;
    using RequestContext;

    /// <summary>
    /// Indexes Microsoft offers and associated them with logo images.
    /// </summary>
    public class MicrosoftOfferLogoIndexer : DomainObject
    {
        /// <summary>
        /// The default logo URI to use.
        /// </summary>
        private const string DefaultLogo = "/Content/Images/Plugins/ProductLogos/microsoft-logo.png";

        /// <summary>
        /// A collection of registered offer logo matchers.
        /// </summary>
        private readonly ICollection<IOfferLogoMatcher> offerLogoMatchers = new List<IOfferLogoMatcher>();

        /// <summary>
        /// A hash table mapping offer product IDs to their respective logo images.
        /// </summary>
        private readonly IDictionary<string, string> offerLogosIndex = new Dictionary<string, string>();

        /// <summary>
        /// Indicates whether offers have been indexed or not.
        /// </summary>
        private bool isIndexed = false;

        /// <summary>
        /// The time the index was last built.
        /// </summary>
        private DateTime lastIndexedTime;

        /// <summary>
        /// Initializes a new instance of the <see cref="MicrosoftOfferLogoIndexer"/> class.
        /// </summary>
        /// <param name="applicationDomain">An application domain instance.</param>
        public MicrosoftOfferLogoIndexer(ApplicationDomain applicationDomain) : base(applicationDomain)
        {
            // register offer logo matchers
            offerLogoMatchers.Add(new OfferLogoMatcher(new string[] { "azure", "active directory" }, "/Content/Images/Plugins/ProductLogos/azure-logo.png"));
            offerLogoMatchers.Add(new OfferLogoMatcher(new string[] { "dynamics", "crm" }, "/Content/Images/Plugins/ProductLogos/dynamics-logo.png"));
            offerLogoMatchers.Add(new OfferLogoMatcher(new string[] { "exchange" }, "/Content/Images/Plugins/ProductLogos/exchange-logo.png"));
            offerLogoMatchers.Add(new OfferLogoMatcher(new string[] { "intune" }, "/Content/Images/Plugins/ProductLogos/intune-logo.png"));
            offerLogoMatchers.Add(new OfferLogoMatcher(new string[] { "onedrive" }, "/Content/Images/Plugins/ProductLogos/onedrive-logo.png"));
            offerLogoMatchers.Add(new OfferLogoMatcher(new string[] { "project" }, "/Content/Images/Plugins/ProductLogos/project-logo.png"));
            offerLogoMatchers.Add(new OfferLogoMatcher(new string[] { "sharepoint" }, "/Content/Images/Plugins/ProductLogos/sharepoint-logo.png"));
            offerLogoMatchers.Add(new OfferLogoMatcher(new string[] { "skype" }, "/Content/Images/Plugins/ProductLogos/skype-logo.png"));
            offerLogoMatchers.Add(new OfferLogoMatcher(new string[] { "visio" }, "/Content/Images/Plugins/ProductLogos/visio-logo.png"));
            offerLogoMatchers.Add(new OfferLogoMatcher(new string[] { "office", "365" }, "/Content/Images/Plugins/ProductLogos/office-logo.png"));
            offerLogoMatchers.Add(new OfferLogoMatcher(new string[] { "yammer" }, "/Content/Images/Plugins/ProductLogos/yammer-logo.png"));

            // we will default the logo if all the above matchers fail to match the given offer
            offerLogoMatchers.Add(new DefaultLogoMatcher());
        }

        /// <summary>
        /// The contract for an offer logo matcher.
        /// </summary>
        private interface IOfferLogoMatcher
        {
            /// <summary>
            /// Attempts to match and offer with a logo URI.
            /// </summary>
            /// <param name="offer">The Microsoft offer to find its logo.</param>
            /// <returns>The logo URI if matched. Empty string is could not match.</returns>
            string Match(Offer offer);
        }

        /// <summary>
        /// Returns a logo URI for the given offer.
        /// </summary>
        /// <param name="offer">The Microsoft offer to retrieve its logo.</param>
        /// <returns>The offer's logo URI.</returns>
        public async Task<string> GetOfferLogoUriAsync(Offer offer)
        {
            if (!isIndexed)
            {
                await IndexOffersAsync().ConfigureAwait(false);
            }
            else
            {
                if (DateTime.Now - lastIndexedTime > TimeSpan.FromDays(1))
                {
                    // it has been more than a day since we last indexed, reindex the next time this is called
                    isIndexed = false;
                }
            }

            return offer?.Product?.Id != null && offerLogosIndex.ContainsKey(offer.Product.Id) ? offerLogosIndex[offer.Product.Id] : DefaultLogo;
        }

        /// <summary>
        /// Indexes offers with their respective logos.
        /// </summary>
        /// <returns>A task.</returns>
        private async Task IndexOffersAsync()
        {
            // Need to manage this based on the partner's country locale to retrieve localized offers for the store front.             
            IPartner localeSpecificPartnerCenterClient = ApplicationDomain.PartnerCenterClient.With(RequestContextFactory.Instance.Create(ApplicationDomain.PortalLocalization.OfferLocale));

            // retrieve the offers for this country
            PartnerCenter.Models.ResourceCollection<Offer> localizedOffers = await localeSpecificPartnerCenterClient.Offers.ByCountry(ApplicationDomain.PortalLocalization.CountryIso2Code).GetAsync().ConfigureAwait(false);

            foreach (Offer offer in localizedOffers.Items)
            {
                if (offer?.Product?.Id != null && offerLogosIndex.ContainsKey(offer.Product.Id))
                {
                    // this offer product has already been indexed, skip it
                    continue;
                }

                foreach (IOfferLogoMatcher offerLogoMatcher in offerLogoMatchers)
                {
                    string logo = offerLogoMatcher.Match(offer);

                    if (!string.IsNullOrWhiteSpace(logo))
                    {
                        // logo matched, add it to the index
                        offerLogosIndex.Add(offer.Product.Id, logo);
                        break;
                    }
                }
            }

            isIndexed = true;
            lastIndexedTime = DateTime.Now;
        }

        /// <summary>
        /// An offer logo matcher implementation that matches the offer product name against a set of keywords.
        /// </summary>
        private class OfferLogoMatcher : IOfferLogoMatcher
        {
            /// <summary>
            /// The logo to use in case the offer was matched.
            /// </summary>
            private readonly string logo;

            /// <summary>
            /// The list of keywords to match against.
            /// </summary>
            private readonly IReadOnlyList<string> keywords;

            /// <summary>
            /// Initializes a new instance of the <see cref="OfferLogoMatcher"/> class.
            /// </summary>
            /// <param name="keywords">The keywords to match the offer product name against.</param>
            /// <param name="logo">The offer logo to use in case of a match.</param>
            public OfferLogoMatcher(IEnumerable<string> keywords, string logo)
            {
                keywords.AssertNotNull(nameof(keywords));
                logo.AssertNotEmpty("logo URI can't be empty");

                this.logo = logo;
                this.keywords = new List<string>(keywords);
            }

            /// <summary>
            /// Matches the given offer against the configured keywords.
            /// </summary>
            /// <param name="offer">The offer to match.</param>
            /// <returns>The logo image if matched. Empty string if not.</returns>
            public string Match(Offer offer)
            {
                offer.AssertNotNull(nameof(offer));

                string offerName = offer.Name?.ToUpperInvariant();

                if (!string.IsNullOrWhiteSpace(offerName))
                {
                    foreach (string keyword in keywords)
                    {
                        if (offerName.Contains(keyword.ToUpperInvariant()))
                        {
                            return logo;
                        }
                    }
                }

                return string.Empty;
            }
        }

        /// <summary>
        /// An implementation that always returns a default logo for any given offer.
        /// </summary>
        private class DefaultLogoMatcher : IOfferLogoMatcher
        {
            /// <summary>
            /// Matches an offer with a logo.
            /// </summary>
            /// <param name="offer">The offer to find its logo.</param>
            /// <returns>The default logo</returns>
            public string Match(Offer offer)
            {
                return DefaultLogo;
            }
        }
    }
}