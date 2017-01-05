// -----------------------------------------------------------------------
// <copyright file="CustomerAccountController.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.CustomerPortal.Controllers
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Http;
    using BusinessLogic;
    using BusinessLogic.Exceptions;
    using Filters;
    using Filters.WebApi;
    using Models;
    using Newtonsoft.Json;
    using PartnerCenter.Models;
    using PartnerCenter.Models.Customers;
    using PartnerCenter.Models.Invoices;
    using PartnerCenter.Models.Subscriptions;
    using RequestContext;

    /// <summary>
    /// Customer Account API Controller.
    /// </summary>
    [RoutePrefix("api/CustomerAccounts")]
    public class CustomerAccountController : BaseController 
    {
        /// <summary>
        /// Gets the logged in customer account details. 
        /// </summary>
        /// <returns>The Customer account details from PC Customer Profile.</returns>
        [Route("")]
        [@Authorize(UserRole = UserRole.Customer)]
        [HttpGet]
        public async Task<CustomerViewModel> GetCustomerAccount()
        {
            string clientCustomerId = this.Principal.PartnerCenterCustomerId;

            CultureInfo responseCulture = new CultureInfo(ApplicationDomain.Instance.PortalLocalization.Locale);
            var localeSpecificPartnerCenterClient = ApplicationDomain.Instance.PartnerCenterClient.With(RequestContextFactory.Instance.Create(responseCulture.Name));
            var customerAllSubscriptions = await localeSpecificPartnerCenterClient.Customers.ById(clientCustomerId).Subscriptions.GetAsync();

            List<CustomerLicensesModel> allSubscriptionsOfCustomer = new List<CustomerLicensesModel>();
            foreach (var item in customerAllSubscriptions.Items)
            {
                if (item.BillingType == BillingType.License)
                {                   
                    allSubscriptionsOfCustomer.Add(new CustomerLicensesModel()
                    {
                        Id = item.Id,
                        OfferName = item.OfferName,
                        Quantity = item.Quantity.ToString("G", responseCulture),
                        Status = this.GetStatusType(item.Status), 
                        CreationDate = item.CreationDate.ToString("d", responseCulture)
                    });
                }                    
            }

            CultureInfo responseCulture = new CultureInfo(ApplicationDomain.Instance.PortalLocalization.Locale);
            var localeSpecificPartnerCenterClient = ApplicationDomain.Instance.PartnerCenterClient.With(RequestContextFactory.Instance.Create(responseCulture.Name));
            var customerAllSubscriptions = await localeSpecificPartnerCenterClient.Customers.ById(clientCustomerId).Subscriptions.GetAsync();

            List<CustomerLicensesModel> allSubscriptionsOfCustomer = (from item in customerAllSubscriptions.Items
                where item.BillingType == BillingType.License
                select new CustomerLicensesModel()
                {
                    Id = item.Id,
                    OfferName = item.OfferName,
                    Quantity = item.Quantity.ToString("G", responseCulture),
                    Status = this.GetStatusType(item.Status),
                    CreationDate = item.CreationDate.ToString("d", responseCulture)
                }).ToList();

            return new CustomerViewModel
            {
                Licenses = allSubscriptionsOfCustomer.OrderBy(items => items.OfferName)
            };
        }

        /// <summary>
        /// Registers a new customer and returns customer registration data.
        /// </summary>
        /// <param name="customerViewModel">The customer's registration information.</param>
        /// <returns>A registered customer object.</returns>
        [Route("")]
        [HttpPost]
        [@Authorize(UserRole = UserRole.None)]
        public async Task<CustomerViewModel> Register([FromBody] CustomerViewModel customerViewModel)
        {
            if (!ModelState.IsValid)
            {
                var errorList = (from item in ModelState.Values
                                 from error in item.Errors
                                 select error.ErrorMessage).ToList();                
                string errorMessage = JsonConvert.SerializeObject(errorList);
                throw new PartnerDomainException(ErrorCode.InvalidInput).AddDetail("ErrorMessage", errorMessage);
            }

            Customer newCustomer = null;
            
            // TODO :: Loc. may need special handling for national clouds deployments (China).
            string domainName = string.Format(CultureInfo.InvariantCulture, "{0}.onmicrosoft.com", customerViewModel.DomainPrefix);  

            // check domain available.
            bool isDomainTaken = await ApplicationDomain.Instance.PartnerCenterClient.Domains.ByDomain(domainName).ExistsAsync();
            if (isDomainTaken)
            {                                    
                throw new PartnerDomainException(ErrorCode.DomainNotAvailable).AddDetail("DomainPrefix", domainName);
            }

            // get the locale, we default to the first locale used in a country for now.
            var customerCountryValidationRules = await ApplicationDomain.Instance.PartnerCenterClient.CountryValidationRules.ByCountry(customerViewModel.Country).GetAsync();
            string billingCulture = customerCountryValidationRules.SupportedCulturesList.FirstOrDefault();      // default billing culture is the first supported culture for the customer's selected country. 
            string billingLanguage = customerCountryValidationRules.SupportedLanguagesList.FirstOrDefault();    // default billing culture is the first supported language for the customer's selected country. 

            newCustomer = new Customer
            {
                CompanyProfile = new CustomerCompanyProfile
                {
                    Domain = domainName,
                },
                BillingProfile = new CustomerBillingProfile
                {
                    Culture = billingCulture,
                    Language = billingLanguage,
                    Email = customerViewModel.Email,
                    CompanyName = customerViewModel.CompanyName,

                    DefaultAddress = new Address
                    {
                        FirstName = customerViewModel.FirstName,
                        LastName = customerViewModel.LastName,
                        AddressLine1 = customerViewModel.AddressLine1,
                        AddressLine2 = customerViewModel.AddressLine2,
                        City = customerViewModel.City,
                        State = customerViewModel.State,
                        Country = customerViewModel.Country,
                        PostalCode = customerViewModel.ZipCode,
                        PhoneNumber = customerViewModel.Phone,
                    }
                }
            };

            newCustomer = await ApplicationDomain.Instance.PartnerCenterClient.Customers.CreateAsync(newCustomer);

            if (HttpContext.Current.Request.IsAuthenticated)
                {
                    // there is a signed in user, add the user to the customers repository
                    await ApplicationDomain.Instance.CustomersRepository.RegisterAsync(this.Principal.TenantId, newCustomer.Id);
                }
            
            return new CustomerViewModel
            {
                AddressLine1 = newCustomer.BillingProfile.DefaultAddress.AddressLine1,
                AddressLine2 = newCustomer.BillingProfile.DefaultAddress.AddressLine2,
                City = newCustomer.BillingProfile.DefaultAddress.City,
                State = newCustomer.BillingProfile.DefaultAddress.State,
                ZipCode = newCustomer.BillingProfile.DefaultAddress.PostalCode,
                Country = newCustomer.BillingProfile.DefaultAddress.Country,
                Phone = newCustomer.BillingProfile.DefaultAddress.PhoneNumber,
                Language = newCustomer.BillingProfile.Language,
                FirstName = newCustomer.BillingProfile.DefaultAddress.FirstName,
                LastName = newCustomer.BillingProfile.DefaultAddress.LastName,
                Email = newCustomer.BillingProfile.Email,
                CompanyName = newCustomer.BillingProfile.CompanyName,
                MicrosoftId = newCustomer.CompanyProfile.TenantId,
                UserName = customerViewModel.Email,                
                Password = newCustomer.UserCredentials.Password,
                AdminUserAccount = newCustomer.UserCredentials.UserName + "@" + newCustomer.CompanyProfile.Domain
            };
        }

        /// <summary>
        /// Retrieves the localized status type string. 
        /// </summary>
        /// <param name="statusType">The subscription status type.</param>
        /// <returns>Localized Operation Type string.</returns>
        private string GetStatusType(SubscriptionStatus statusType)
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