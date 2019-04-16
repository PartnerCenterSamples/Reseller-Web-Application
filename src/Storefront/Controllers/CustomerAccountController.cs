// -----------------------------------------------------------------------
// <copyright file="CustomerAccountController.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Storefront.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web.Http;
    using BusinessLogic;
    using BusinessLogic.Commerce;
    using BusinessLogic.Exceptions;
    using Filters;
    using Models;
    using Newtonsoft.Json;
    using PartnerCenter.Models;
    using PartnerCenter.Models.Subscriptions;
    using RequestContext;

    /// <summary>
    /// Customer Account API Controller.
    /// </summary>
    [RoutePrefix("api/CustomerAccounts")]
    public class CustomerAccountController : BaseController
    {
        /// <summary>
        /// Gets the subscriptions managed by Customer and as well by Partner.
        /// </summary>
        /// <returns>The Subscriptions details from PC Customer Profile.</returns>
        [Route("Subscriptions")]
        [Filters.WebApi.PortalAuthorize(UserRole = UserRole.Customer)]
        [HttpGet]
        public async Task<ManagedSubscriptionsViewModel> GetCustomerSubscriptions()
        {
            return await GetManagedSubscriptions().ConfigureAwait(false);
        }

        /// <summary>
        /// Persists a new customer information and returns customer temporary guid.
        /// </summary>
        /// <param name="customerViewModel">The customer's registration information.</param>
        /// <returns>A registered customer object.</returns>
        [Route("")]
        [HttpPost]
        [Filters.WebApi.PortalAuthorize(UserRole = UserRole.None)]
        public async Task<string> Register([FromBody] CustomerViewModel customerViewModel)
        {
            if (!ModelState.IsValid)
            {
                List<string> errorList = (from item in ModelState.Values
                                          from error in item.Errors
                                          select error.ErrorMessage).ToList();
                string errorMessage = JsonConvert.SerializeObject(errorList);
                throw new PartnerDomainException(ErrorCode.InvalidInput).AddDetail("ErrorMessage", errorMessage);
            }

            // TODO :: Loc. may need special handling for national clouds deployments (China).
            string domainName = string.Format(CultureInfo.InvariantCulture, "{0}.onmicrosoft.com", customerViewModel.DomainPrefix);

            // check domain available.

            bool isDomainTaken = await ApplicationDomain.Instance.PartnerCenterClient.Domains.ByDomain(domainName).ExistsAsync().ConfigureAwait(false);
            if (isDomainTaken)
            {
                throw new PartnerDomainException(ErrorCode.DomainNotAvailable).AddDetail("DomainPrefix", domainName);
            }

            // get the locale, we default to the first locale used in a country for now.
            PartnerCenter.Models.CountryValidationRules.CountryValidationRules customerCountryValidationRules = await ApplicationDomain.Instance.PartnerCenterClient.CountryValidationRules.ByCountry(customerViewModel.Country).GetAsync().ConfigureAwait(false);
            string billingCulture = customerCountryValidationRules.SupportedCulturesList.FirstOrDefault();      // default billing culture is the first supported culture for the customer's selected country. 
            string billingLanguage = customerCountryValidationRules.SupportedLanguagesList.FirstOrDefault();    // default billing culture is the first supported language for the customer's selected country. 

            CustomerViewModel customerRegistrationInfoToPersist = new CustomerViewModel()
            {
                AddressLine1 = customerViewModel.AddressLine1,
                AddressLine2 = customerViewModel.AddressLine2,
                City = customerViewModel.City,
                State = customerViewModel.State,
                ZipCode = customerViewModel.ZipCode,
                Country = customerViewModel.Country,
                Phone = customerViewModel.Phone,
                Language = customerViewModel.Language,
                FirstName = customerViewModel.FirstName,
                LastName = customerViewModel.LastName,
                Email = customerViewModel.Email,
                CompanyName = customerViewModel.CompanyName,
                MicrosoftId = Guid.NewGuid().ToString(),
                UserName = customerViewModel.Email,
                BillingLanguage = billingLanguage,
                BillingCulture = billingCulture,
                DomainName = domainName,
                DomainPrefix = customerViewModel.DomainPrefix
            };

            CustomerRegistrationRepository customerRegistrationRepository = new CustomerRegistrationRepository(ApplicationDomain.Instance);
            CustomerViewModel customerRegistrationInfo = await customerRegistrationRepository.AddAsync(customerRegistrationInfoToPersist).ConfigureAwait(false);

            return customerRegistrationInfo.MicrosoftId;
        }

        /// <summary>
        /// Gets the subscriptions managed by customers and partners
        /// </summary>
        /// <returns>returns managed subscriptions view model</returns>
        private async Task<ManagedSubscriptionsViewModel> GetManagedSubscriptions()
        {
            DateTime startTime = DateTime.Now;

            string clientCustomerId = Principal.PartnerCenterCustomerId;

            // responseCulture determines decimals, currency and such
            CultureInfo responseCulture = new CultureInfo(ApplicationDomain.Instance.PortalLocalization.Locale);

            // localeSpecificApiClient allows pulling offer names localized to supported portal locales compatible with Offer API supported locales. 
            IPartner localeSpecificPartnerCenterClient = ApplicationDomain.Instance.PartnerCenterClient.With(RequestContextFactory.Instance.Create(ApplicationDomain.Instance.PortalLocalization.OfferLocale));

            // Get all subscriptions of customer from PC
            ResourceCollection<Subscription> customerAllSubscriptions = await localeSpecificPartnerCenterClient.Customers.ById(clientCustomerId).Subscriptions.GetAsync().ConfigureAwait(false);

            IEnumerable<CustomerSubscriptionEntity> customerSubscriptions = await ApplicationDomain.Instance.CustomerSubscriptionsRepository.RetrieveAsync(clientCustomerId).ConfigureAwait(false);
            IEnumerable<PartnerOffer> allPartnerOffers = await ApplicationDomain.Instance.OffersRepository.RetrieveAsync().ConfigureAwait(false);
            IEnumerable<MicrosoftOffer> currentMicrosoftOffers = await ApplicationDomain.Instance.OffersRepository.RetrieveMicrosoftOffersAsync().ConfigureAwait(false);

            List<SubscriptionViewModel> customerSubscriptionsView = new List<SubscriptionViewModel>();

            // iterate through and build the list of customer's subscriptions.
            foreach (CustomerSubscriptionEntity subscription in customerSubscriptions)
            {
                PartnerOffer partnerOfferItem = allPartnerOffers.FirstOrDefault(offer => offer.Id == subscription.PartnerOfferId);
                string subscriptionTitle = partnerOfferItem.Title;
                string portalOfferId = partnerOfferItem.Id;
                decimal portalOfferPrice = partnerOfferItem.Price;

                DateTime subscriptionExpiryDate = subscription.ExpiryDate.ToUniversalTime();
                int remainingDays = (subscriptionExpiryDate.Date - DateTime.UtcNow.Date).Days;
                bool isRenewable = remainingDays <= 30;                                         // IsRenewable is true if subscription is going to expire in 30 days.
                bool isEditable = DateTime.UtcNow.Date <= subscriptionExpiryDate.Date;          // IsEditable is true if today is lesser or equal to subscription expiry date.

                // Temporarily mark this partnerOffer item as inactive and dont allow store front customer to manage this subscription. 
                MicrosoftOffer alignedMicrosoftOffer = currentMicrosoftOffers.FirstOrDefault(offer => offer.Offer.Id == partnerOfferItem.MicrosoftOfferId);

                if (alignedMicrosoftOffer == null)
                {
                    // The offer is inactive (marked for deletion) then dont allow renewals or editing on this subscription tied to this offer. 
                    partnerOfferItem.IsInactive = true;
                    isRenewable = false;
                    isEditable = false;
                }

                // Compute the pro rated price per seat for this subcription & return for client side processing during updates. 
                decimal proratedPerSeatPrice = Math.Round(CommerceOperations.CalculateProratedSeatCharge(subscription.ExpiryDate, portalOfferPrice), responseCulture.NumberFormat.CurrencyDecimalDigits);

                SubscriptionViewModel subscriptionItem = new SubscriptionViewModel()
                {
                    SubscriptionId = subscription.SubscriptionId,
                    FriendlyName = subscriptionTitle,
                    PortalOfferId = portalOfferId,
                    PortalOfferPrice = portalOfferPrice.ToString("C", responseCulture),
                    IsRenewable = isRenewable,
                    IsEditable = isEditable,
                    SubscriptionExpiryDate = subscriptionExpiryDate.Date.ToString("d", responseCulture),
                    SubscriptionProRatedPrice = proratedPerSeatPrice
                };

                // add this subcription to the customer's subscription list.
                customerSubscriptionsView.Add(subscriptionItem);
            }

            List<CustomerSubscriptionModel> customerManagedSubscriptions = new List<CustomerSubscriptionModel>();
            List<PartnerSubscriptionModel> partnerManagedSubscriptions = new List<PartnerSubscriptionModel>();

            // Divide the subscriptions by customer and partner
            foreach (Subscription customerSubscriptionFromPC in customerAllSubscriptions.Items)
            {
                SubscriptionViewModel subscription = customerSubscriptionsView.FirstOrDefault(sub => sub.SubscriptionId == customerSubscriptionFromPC.Id);

                // Customer managed subscription found
                if (subscription != null)
                {
                    CustomerSubscriptionModel customerSubscription = new CustomerSubscriptionModel()
                    {
                        SubscriptionId = customerSubscriptionFromPC.Id,
                        LicensesTotal = customerSubscriptionFromPC.Quantity.ToString("G", responseCulture),
                        Status = GetStatusType(customerSubscriptionFromPC.Status),
                        CreationDate = customerSubscriptionFromPC.CreationDate.ToString("d", responseCulture),
                        FriendlyName = subscription.FriendlyName,
                        IsRenewable = subscription.IsRenewable,
                        IsEditable = subscription.IsEditable,
                        PortalOfferId = subscription.PortalOfferId,
                        SubscriptionProRatedPrice = subscription.SubscriptionProRatedPrice
                    };

                    customerManagedSubscriptions.Add(customerSubscription);
                }
                else
                {
                    PartnerSubscriptionModel partnerSubscription = new PartnerSubscriptionModel()
                    {
                        Id = customerSubscriptionFromPC.Id,
                        OfferName = customerSubscriptionFromPC.OfferName,
                        Quantity = customerSubscriptionFromPC.Quantity.ToString("G", responseCulture),
                        Status = GetStatusType(customerSubscriptionFromPC.Status),
                        CreationDate = customerSubscriptionFromPC.CreationDate.ToString("d", responseCulture),
                    };

                    partnerManagedSubscriptions.Add(partnerSubscription);
                }
            }

            ManagedSubscriptionsViewModel managedSubscriptions = new ManagedSubscriptionsViewModel()
            {
                CustomerManagedSubscriptions = customerManagedSubscriptions.OrderByDescending(customerManagedSubscription => customerManagedSubscription.CreationDate),
                PartnerManagedSubscriptions = partnerManagedSubscriptions.OrderByDescending(partnerManagedSubscription => partnerManagedSubscription.CreationDate)
            };

            // Capture the request for customer managed subscriptions and partner managed subscriptions for analysis.
            Dictionary<string, string> eventProperties = new Dictionary<string, string> { { "CustomerId", clientCustomerId } };

            // Track the event measurements for analysis.
            Dictionary<string, double> eventMetrics = new Dictionary<string, double> { { "ElapsedMilliseconds", DateTime.Now.Subtract(startTime).TotalMilliseconds }, { "CustomerManagedSubscriptions", customerManagedSubscriptions.Count }, { "PartnerManagedSubscriptions", partnerManagedSubscriptions.Count } };

            ApplicationDomain.Instance.TelemetryService.Provider.TrackEvent("GetManagedSubscriptions", eventProperties, eventMetrics);

            return managedSubscriptions;
        }

        /// <summary>
        /// Retrieves the localized status type string. 
        /// </summary>
        /// <param name="statusType">The subscription status type.</param>
        /// <returns>Localized Operation Type string.</returns>
        private static string GetStatusType(SubscriptionStatus statusType)
        {
            switch (statusType)
            {
                case SubscriptionStatus.Active:
                    return Resources.SubscriptionStatusTypeActive;
                case SubscriptionStatus.Deleted:
                    return Resources.SubscriptionStatusTypeDeleted;
                case SubscriptionStatus.None:
                    return Resources.SubscriptionStatusTypeNone;
                case SubscriptionStatus.Suspended:
                    return Resources.SubscriptionStatusTypeSuspended;
                default:
                    return string.Empty;
            }
        }
    }
}