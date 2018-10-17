// -----------------------------------------------------------------------
// <copyright file="PortalLocalization.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Storefront.BusinessLogic
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// Encapsulates locale information for the portal based on the partner's region.
    /// </summary>
    public class PortalLocalization : DomainObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PortalLocalization"/> class.
        /// </summary>
        /// <param name="applicationDomain">An application domain instance.</param>
        public PortalLocalization(ApplicationDomain applicationDomain) : base(applicationDomain)
        {
        }

        /// <summary>
        /// Gets the portal's country ISO2 code. E.g. US
        /// </summary>
        public string CountryIso2Code { get; private set; }

        /// <summary>
        /// Gets the portal's locale. E.g. En-US
        /// </summary>
        public string Locale { get; private set; }

        /// <summary>
        /// Gets the portal's ISO currency code. E.g. USD
        /// </summary>
        public string CurrencyCode { get; private set; }

        /// <summary>
        /// Gets the portal's currency symbol. E.g. $
        /// </summary>
        public string CurrencySymbol { get; private set; }

        /// <summary>
        /// Gets the portal's locale which is applied for offer API calls to partner center. 
        /// </summary>
        public string OfferLocale { get; private set;  }

        /// <summary>
        /// Initializes state and ensures the object is ready to be consumed.
        /// </summary>
        /// <returns>A task.</returns>
        public async Task InitializeAsync()
        {
            var partnerLegalBusinessProfile = await this.ApplicationDomain.PartnerCenterClient.Profiles.LegalBusinessProfile.GetAsync().ConfigureAwait(false);
            this.CountryIso2Code = partnerLegalBusinessProfile.Address.Country;

            RegionInfo partnerRegion = null;
            
            try
            {   
                // Get the default locale using the Country Validation rules infrastructure.  
                var partnerCountryValidationRules = await ApplicationDomain.Instance.PartnerCenterClient.CountryValidationRules.ByCountry(CountryIso2Code).GetAsync().ConfigureAwait(false);

                this.Locale = partnerCountryValidationRules.DefaultCulture;
                partnerRegion = new RegionInfo(new CultureInfo(this.Locale, false).LCID);
            }
            catch
            {
                // we will default region to en-US so that currency is USD. 
                this.Locale = "en-US";
                partnerRegion = new RegionInfo(new CultureInfo(this.Locale, false).LCID);                
            }

            this.OfferLocale = this.ResolveOfferLocale(this.Locale);
            
            // figure out the currency             
            this.CurrencyCode = partnerRegion.ISOCurrencySymbol;
            this.CurrencySymbol = partnerRegion.CurrencySymbol;

            // set culture to partner locale.
            Resources.Culture = new CultureInfo(this.Locale);
        }

        /// <summary>
        /// Resolver to identify the right locale settings which can be used for calling offer APIs. 
        /// </summary>
        /// <param name="locale">Partner Locale</param>
        /// <returns>Offer Locale</returns>
        private string ResolveOfferLocale(string locale)
        {
            List<string> portalSupportedLocales = new List<string>
            { 
                "de",
                "en",
                "es",
                "nl",
                "fr",
                "ja"
            };

            List<string> portalOfferLocaleDefaults = new List<string>
            {
                "de-DE",
                "en-US",
                "es-ES",
                "nl-NL",
                "fr-FR",
                "ja-JP"
            };
                        
            //// Examples [en-US, en-GB, en-CA] -> en-US ==> en ==> (en-US), en-GB ==> en ==> (en-US), en-CA ==> en ==> (en-US).            
            //// Examples [fr-CH, de-CH, it-CH] -> fr-CH ==> fr ==> (fr-FR), de-CH ==> de ==> (de-DE), it-CH ==> it ==> (en-US).

            // if language is not supported by Portal then default it to en-US since the portal runs on english. 
            string offerSpecificLocale = "en-US";

            // if language is supported by Portal then default it to the portalOfferLocaleDefault 
            string languageInLocale = new CultureInfo(locale).TwoLetterISOLanguageName;

            // check the language part and see if we can default to one of the top locales. 
            int localeIndex = portalSupportedLocales.IndexOf(languageInLocale);
            if (localeIndex > -1)
            {
                offerSpecificLocale = portalOfferLocaleDefaults.ElementAt(localeIndex);
            }
                        
            return offerSpecificLocale;
        }
    }
}