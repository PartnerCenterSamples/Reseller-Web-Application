// -----------------------------------------------------------------------
// <copyright file="OrderController.cs" company="Microsoft">
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
    using BusinessLogic.Commerce.PaymentGateways;
    using BusinessLogic.Exceptions;
    using Filters;
    using Microsoft.Store.PartnerCenter.Models.Agreements;
    using Models;
    using Newtonsoft.Json;
    using PartnerCenter.Models;
    using PartnerCenter.Models.Customers;

    /// <summary>
    /// Manages customer orders.
    /// </summary>    
    [RoutePrefix("api/Order")]
    public class OrderController : BaseController
    {
        /// <summary>
        /// Updates a customer's subscriptions.
        /// </summary>
        /// <param name="orderDetails">A list of subscriptions to update.</param>
        /// <returns>The payment url from payment gateway.</returns>
        [Filters.WebApi.PortalAuthorize(UserRole = UserRole.Customer)]
        [HttpPost]
        [Route("Prepare")]
        public async Task<string> PrepareOrderForAuthenticatedCustomer([FromBody]OrderViewModel orderDetails)
        {
            DateTime startTime = DateTime.Now;

            if (!ModelState.IsValid)
            {
                List<string> errorList = (from item in ModelState.Values
                                          from error in item.Errors
                                          select error.ErrorMessage).ToList();
                string errorMessage = JsonConvert.SerializeObject(errorList);
                throw new PartnerDomainException(ErrorCode.InvalidInput).AddDetail("ErrorMessage", errorMessage);
            }

            orderDetails.CustomerId = Principal.PartnerCenterCustomerId;
            orderDetails.OrderId = Guid.NewGuid().ToString();
            string operationDescription = string.Empty;

            // Validate & Normalize the order information.
            OrderNormalizer orderNormalizer = new OrderNormalizer(ApplicationDomain.Instance, orderDetails);
            switch (orderDetails.OperationType)
            {
                case CommerceOperationType.AdditionalSeatsPurchase:
                    operationDescription = Resources.AddSeatsOperationCaption;
                    orderDetails = await orderNormalizer.NormalizePurchaseAdditionalSeatsOrderAsync().ConfigureAwait(false);
                    break;
                case CommerceOperationType.NewPurchase:
                    operationDescription = Resources.NewPurchaseOperationCaption;
                    orderDetails = await orderNormalizer.NormalizePurchaseSubscriptionOrderAsync().ConfigureAwait(false);
                    break;
                case CommerceOperationType.Renewal:
                    operationDescription = Resources.RenewOperationCaption;
                    orderDetails = await orderNormalizer.NormalizeRenewSubscriptionOrderAsync().ConfigureAwait(false);
                    break;
            }

            // prepare the redirect url so that client can redirect to payment gateway.             
            string redirectUrl = string.Format(CultureInfo.InvariantCulture, "{0}/#ProcessOrder?ret=true", Request.RequestUri.GetLeftPart(UriPartial.Authority));

            // Create the right payment gateway to use for customer oriented payment transactions. 
            IPaymentGateway paymentGateway = await CreatePaymentGateway(operationDescription, orderDetails.CustomerId).ConfigureAwait(false);

            // execute and get payment gateway action URI.           
            string generatedUri = await paymentGateway.GeneratePaymentUriAsync(redirectUrl, orderDetails).ConfigureAwait(false);

            // Capture the request for the customer summary for analysis.
            Dictionary<string, string> eventProperties = new Dictionary<string, string> { { "CustomerId", orderDetails.CustomerId }, { "OperationType", orderDetails.OperationType.ToString() } };

            // Track the event measurements for analysis.
            Dictionary<string, double> eventMetrics = new Dictionary<string, double> { { "ElapsedMilliseconds", DateTime.Now.Subtract(startTime).TotalMilliseconds } };

            ApplicationDomain.Instance.TelemetryService.Provider.TrackEvent("api/order/prepare", eventProperties, eventMetrics);

            return generatedUri;
        }

        /// <summary>
        /// Processes the order raised for an authenticated customer. 
        /// </summary>
        /// <param name="paymentId">Payment Id.</param>
        /// <param name="payerId">Payer Id.</param>
        /// <param name="orderId">Order Id.</param>
        /// <returns>Commerce transaction result.</returns>        
        [Filters.WebApi.PortalAuthorize(UserRole = UserRole.Customer)]
        [HttpGet]
        [Route("Process")]
        public async Task<TransactionResult> ProcessOrderForAuthenticatedCustomer(string paymentId, string payerId, string orderId)
        {
            DateTime startTime = DateTime.Now;

            // extract order information and create payment payload.
            string clientCustomerId = Principal.PartnerCenterCustomerId;

            paymentId.AssertNotEmpty(nameof(paymentId));
            payerId.AssertNotEmpty(nameof(payerId));
            orderId.AssertNotEmpty(nameof(orderId));

            // Create the right payment gateway to use for customer oriented payment transactions. 
            IPaymentGateway paymentGateway = await CreatePaymentGateway("ProcessingOrder", clientCustomerId).ConfigureAwait(false);

            // use payment gateway to extract order information.             
            OrderViewModel orderToProcess = await paymentGateway.GetOrderDetailsFromPaymentAsync(payerId, paymentId, orderId, clientCustomerId).ConfigureAwait(false);
            orderToProcess.CustomerId = clientCustomerId;
            CommerceOperations commerceOperation = new CommerceOperations(ApplicationDomain.Instance, clientCustomerId, paymentGateway);

            TransactionResult transactionResult = null;
            switch (orderToProcess.OperationType)
            {
                case CommerceOperationType.Renewal:
                    transactionResult = await commerceOperation.RenewSubscriptionAsync(orderToProcess).ConfigureAwait(false);
                    break;
                case CommerceOperationType.AdditionalSeatsPurchase:
                    transactionResult = await commerceOperation.PurchaseAdditionalSeatsAsync(orderToProcess).ConfigureAwait(false);
                    break;
                case CommerceOperationType.NewPurchase:
                    transactionResult = await commerceOperation.PurchaseAsync(orderToProcess).ConfigureAwait(false);
                    break;
            }

            // Capture the request for the customer summary for analysis.
            Dictionary<string, string> eventProperties = new Dictionary<string, string> { { "CustomerId", orderToProcess.CustomerId }, { "OperationType", orderToProcess.OperationType.ToString() }, { "PayerId", payerId }, { "PaymentId", paymentId } };

            // Track the event measurements for analysis.
            Dictionary<string, double> eventMetrics = new Dictionary<string, double> { { "ElapsedMilliseconds", DateTime.Now.Subtract(startTime).TotalMilliseconds } };

            ApplicationDomain.Instance.TelemetryService.Provider.TrackEvent("api/order/process", eventProperties, eventMetrics);

            return transactionResult;
        }

        /// <summary>
        /// Prepare an order for an unauthenticated customer. Supports only purchase of new subscriptions.
        /// </summary>
        /// <param name="orderDetails">A list of subscriptions to update.</param>
        /// <returns>The payment url from PayPal.</returns>
        [Filters.WebApi.PortalAuthorize(UserRole = UserRole.None)]
        [HttpPost]
        [Route("NewCustomerPrepareOrder")]
        public async Task<string> PrepareOrderForUnAuthenticatedCustomer([FromBody]OrderViewModel orderDetails)
        {
            DateTime startTime = DateTime.Now;

            if (!ModelState.IsValid)
            {
                List<string> errorList = (from item in ModelState.Values
                                          from error in item.Errors
                                          select error.ErrorMessage).ToList();
                string errorMessage = JsonConvert.SerializeObject(errorList);
                throw new PartnerDomainException(ErrorCode.InvalidInput).AddDetail("ErrorMessage", errorMessage);
            }

            // Validate & Normalize the order information.
            OrderNormalizer orderNormalizer = new OrderNormalizer(ApplicationDomain.Instance, orderDetails);
            orderDetails = await orderNormalizer.NormalizePurchaseSubscriptionOrderAsync().ConfigureAwait(false);

            // prepare the redirect url so that client can redirect to PayPal.             
            string redirectUrl = string.Format(CultureInfo.InvariantCulture, "{0}/#ProcessOrder?ret=true&customerId={1}", Request.RequestUri.GetLeftPart(UriPartial.Authority), orderDetails.CustomerId);

            // execute to paypal and get paypal action URI. 
            IPaymentGateway paymentGateway = PaymentGatewayConfig.GetPaymentGatewayInstance(ApplicationDomain.Instance, Resources.NewPurchaseOperationCaption);
            string generatedUri = await paymentGateway.GeneratePaymentUriAsync(redirectUrl, orderDetails).ConfigureAwait(false);

            // Track the event measurements for analysis.
            Dictionary<string, double> eventMetrics = new Dictionary<string, double> { { "ElapsedMilliseconds", DateTime.Now.Subtract(startTime).TotalMilliseconds } };

            ApplicationDomain.Instance.TelemetryService.Provider.TrackEvent("api/order/NewCustomerPrepareOrder", null, eventMetrics);

            return generatedUri;
        }

        /// <summary>
        /// Processes the order raised for an unauthenticated customer. 
        /// </summary>
        /// <param name="customerId">Customer Id generated by register customer call.</param>
        /// <param name="paymentId">Payment Id.</param>
        /// <param name="payerId">Payer Id.</param>
        /// <returns>Subscription Summary.</returns>        
        [Filters.WebApi.PortalAuthorize(UserRole = UserRole.None)]
        [HttpGet]
        [Route("NewCustomerProcessOrder")]
        public async Task<SubscriptionsSummary> ProcessOrderForUnAuthenticatedCustomer(string customerId, string paymentId, string payerId)
        {
            DateTime startTime = DateTime.Now;

            customerId.AssertNotEmpty(nameof(customerId));
            paymentId.AssertNotEmpty(nameof(paymentId));
            payerId.AssertNotEmpty(nameof(payerId));

            BrandingConfiguration branding = await ApplicationDomain.Instance.PortalBranding.RetrieveAsync().ConfigureAwait(false);

            CustomerViewModel customerRegistrationInfoPersisted = await ApplicationDomain.Instance.CustomerRegistrationRepository.RetrieveAsync(customerId).ConfigureAwait(false);

            Customer newCustomer = new Customer()
            {
                CompanyProfile = new CustomerCompanyProfile()
                {
                    Domain = customerRegistrationInfoPersisted.DomainName,
                },
                BillingProfile = new CustomerBillingProfile()
                {
                    Culture = customerRegistrationInfoPersisted.BillingCulture,
                    Language = customerRegistrationInfoPersisted.BillingLanguage,
                    Email = customerRegistrationInfoPersisted.Email,
                    CompanyName = customerRegistrationInfoPersisted.CompanyName,

                    DefaultAddress = new Address()
                    {
                        FirstName = customerRegistrationInfoPersisted.FirstName,
                        LastName = customerRegistrationInfoPersisted.LastName,
                        AddressLine1 = customerRegistrationInfoPersisted.AddressLine1,
                        AddressLine2 = customerRegistrationInfoPersisted.AddressLine2,
                        City = customerRegistrationInfoPersisted.City,
                        State = customerRegistrationInfoPersisted.State,
                        Country = customerRegistrationInfoPersisted.Country,
                        PostalCode = customerRegistrationInfoPersisted.ZipCode,
                        PhoneNumber = customerRegistrationInfoPersisted.Phone,
                    }
                }
            };

            // Register customer
            newCustomer = await ApplicationDomain.Instance.PartnerCenterClient.Customers.CreateAsync(newCustomer).ConfigureAwait(false);

            ResourceCollection<AgreementMetaData> agreements = await ApplicationDomain.Instance.PartnerCenterClient.AgreementDetails.GetAsync().ConfigureAwait(false);

            // Obtain reference to the Microsoft Cloud Agreement.
            AgreementMetaData microsoftCloudAgreement = agreements.Items.FirstOrDefault(agr => agr.AgreementType == AgreementType.MicrosoftCloudAgreement);

            // Attest that the customer has accepted the Microsoft Cloud Agreement (MCA).
            await ApplicationDomain.Instance.PartnerCenterClient.Customers[newCustomer.Id].Agreements.CreateAsync(
                new Agreement
                {
                    DateAgreed = DateTime.UtcNow,
                    PrimaryContact = new PartnerCenter.Models.Agreements.Contact
                    {
                        Email = customerRegistrationInfoPersisted.Email,
                        FirstName = customerRegistrationInfoPersisted.FirstName,
                        LastName = customerRegistrationInfoPersisted.LastName,
                        PhoneNumber = customerRegistrationInfoPersisted.Phone
                    },
                    TemplateId = microsoftCloudAgreement.TemplateId,
                    Type = AgreementType.MicrosoftCloudAgreement,
                    UserId = branding.AgreementUserId
                });

            string newCustomerId = newCustomer.CompanyProfile.TenantId;

            CustomerViewModel customerViewModel = new CustomerViewModel()
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
                UserName = newCustomer.BillingProfile.Email,
                Password = newCustomer.UserCredentials.Password,
                AdminUserAccount = newCustomer.UserCredentials.UserName + "@" + newCustomer.CompanyProfile.Domain
            };

            IPaymentGateway paymentGateway = PaymentGatewayConfig.GetPaymentGatewayInstance(ApplicationDomain.Instance, "ProcessingOrder");
            OrderViewModel orderToProcess = await paymentGateway.GetOrderDetailsFromPaymentAsync(payerId, paymentId, string.Empty, string.Empty).ConfigureAwait(false);

            // Assign the actual customer Id
            orderToProcess.CustomerId = newCustomerId;

            CommerceOperations commerceOperation = new CommerceOperations(ApplicationDomain.Instance, newCustomerId, paymentGateway);
            await commerceOperation.PurchaseAsync(orderToProcess).ConfigureAwait(false);
            SubscriptionsSummary summaryResult = await GetSubscriptionSummaryAsync(newCustomerId).ConfigureAwait(false);

            // Remove the persisted customer registration info.
            await ApplicationDomain.Instance.CustomerRegistrationRepository.DeleteAsync(customerId).ConfigureAwait(false);

            // Capture the request for the customer summary for analysis.
            Dictionary<string, string> eventProperties = new Dictionary<string, string> { { "CustomerId", orderToProcess.CustomerId } };

            // Track the event measurements for analysis.
            Dictionary<string, double> eventMetrics = new Dictionary<string, double> { { "ElapsedMilliseconds", DateTime.Now.Subtract(startTime).TotalMilliseconds } };

            ApplicationDomain.Instance.TelemetryService.Provider.TrackEvent("api/order/NewCustomerProcessOrder", eventProperties, eventMetrics);

            summaryResult.CustomerViewModel = customerViewModel;

            return summaryResult;
        }

        /// <summary>
        /// Retrieves a summary of all subscriptions and their respective order histories. 
        /// </summary>        
        /// <returns>The Subscription summary used by the client used for rendering purposes.</returns>
        [HttpGet]
        [Filters.WebApi.PortalAuthorize(UserRole = UserRole.Customer)]
        [Route("summary")]
        public async Task<SubscriptionsSummary> SubscriptionSummary()
        {
            return await GetSubscriptionSummaryAsync(Principal.PartnerCenterCustomerId).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets the summary of subscriptions for a portal customer. 
        /// </summary>
        /// <param name="customerId">The customer Id.</param>
        /// <returns>Subscription Summary.</returns>
        private async Task<SubscriptionsSummary> GetSubscriptionSummaryAsync(string customerId)
        {
            DateTime startTime = DateTime.Now;
            IEnumerable<CustomerSubscriptionEntity> customerSubscriptions = await ApplicationDomain.Instance.CustomerSubscriptionsRepository.RetrieveAsync(customerId).ConfigureAwait(false);
            IEnumerable<CustomerPurchaseEntity> customerSubscriptionsHistory = await ApplicationDomain.Instance.CustomerPurchasesRepository.RetrieveAsync(customerId).ConfigureAwait(false);
            IEnumerable<PartnerOffer> allPartnerOffers = await ApplicationDomain.Instance.OffersRepository.RetrieveAsync().ConfigureAwait(false);
            IEnumerable<MicrosoftOffer> currentMicrosoftOffers = await ApplicationDomain.Instance.OffersRepository.RetrieveMicrosoftOffersAsync().ConfigureAwait(false);

            // start building the summary.                 
            decimal summaryTotal = 0;

            // format all responses to client using portal locale. 
            CultureInfo responseCulture = new CultureInfo(ApplicationDomain.Instance.PortalLocalization.Locale);
            List<SubscriptionViewModel> customerSubscriptionsView = new List<SubscriptionViewModel>();

            // iterate through and build the list of customer's subscriptions. 
            foreach (CustomerSubscriptionEntity subscription in customerSubscriptions)
            {
                decimal subscriptionTotal = 0;
                int licenseTotal = 0;
                List<SubscriptionHistory> historyItems = new List<SubscriptionHistory>();

                // collect the list of history items for this subcription.  
                IOrderedEnumerable<CustomerPurchaseEntity> subscriptionHistoryList = customerSubscriptionsHistory
                    .Where(historyItem => historyItem.SubscriptionId == subscription.SubscriptionId)
                    .OrderBy(historyItem => historyItem.TransactionDate);

                // iterate through and build the SubsriptionHistory for this subscription. 
                foreach (CustomerPurchaseEntity historyItem in subscriptionHistoryList)
                {
                    decimal orderTotal = Math.Round(historyItem.SeatPrice * historyItem.SeatsBought, responseCulture.NumberFormat.CurrencyDecimalDigits);
                    historyItems.Add(new SubscriptionHistory()
                    {
                        OrderTotal = orderTotal.ToString("C", responseCulture),                                 // Currency format.
                        PricePerSeat = historyItem.SeatPrice.ToString("C", responseCulture),                    // Currency format. 
                        SeatsBought = historyItem.SeatsBought.ToString("G", responseCulture),                   // General format.  
                        OrderDate = historyItem.TransactionDate.ToLocalTime().ToString("d", responseCulture),   // Short date format. 
                        OperationType = GetOperationType(historyItem.PurchaseType)                         // Localized Operation type string. 
                    });

                    // Increment the subscription total. 
                    licenseTotal += historyItem.SeatsBought;

                    // Increment the subscription total. 
                    subscriptionTotal += orderTotal;
                }

                PartnerOffer partnerOfferItem = allPartnerOffers.FirstOrDefault(offer => offer.Id == subscription.PartnerOfferId);
                string subscriptionTitle = partnerOfferItem.Title;
                string portalOfferId = partnerOfferItem.Id;
                decimal portalOfferPrice = partnerOfferItem.Price;

                DateTime subscriptionExpiryDate = subscription.ExpiryDate.ToUniversalTime();
                int remainingDays = (subscriptionExpiryDate.Date - DateTime.UtcNow.Date).Days;
                bool isRenewable = remainingDays <= 30;
                bool isEditable = DateTime.UtcNow.Date <= subscriptionExpiryDate.Date;

                // TODO :: Handle Microsoft offer being pulled back due to EOL. 

                // Temporarily mark this partnerOffer item as inactive and dont allow store front customer to manage this subscription. 
                MicrosoftOffer alignedMicrosoftOffer = currentMicrosoftOffers.FirstOrDefault(offer => offer.Offer.Id == partnerOfferItem.MicrosoftOfferId);
                if (alignedMicrosoftOffer == null)
                {
                    partnerOfferItem.IsInactive = true;
                }

                if (partnerOfferItem.IsInactive)
                {
                    // in case the offer is inactive (marked for deletion) then dont allow renewals or editing on this subscription tied to this offer. 
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
                    IsRenewable = isRenewable,                                                              // IsRenewable is true if subscription is going to expire in 30 days.                         
                    IsEditable = isEditable,                                                                // IsEditable is true if today is lesser or equal to subscription expiry date.                                                
                    LicensesTotal = licenseTotal.ToString("G", responseCulture),                            // General format. 
                    SubscriptionTotal = subscriptionTotal.ToString("C", responseCulture),                   // Currency format.
                    SubscriptionExpiryDate = subscriptionExpiryDate.Date.ToString("d", responseCulture),    // Short date format. 
                    SubscriptionOrderHistory = historyItems,
                    SubscriptionProRatedPrice = proratedPerSeatPrice
                };

                // add this subcription to the customer's subscription list.
                customerSubscriptionsView.Add(subscriptionItem);

                // Increment the summary total. 
                summaryTotal += subscriptionTotal;
            }

            // Capture the request for the customer summary for analysis.
            Dictionary<string, string> eventProperties = new Dictionary<string, string> { { "CustomerId", customerId } };

            // Track the event measurements for analysis.
            Dictionary<string, double> eventMetrics = new Dictionary<string, double> { { "ElapsedMilliseconds", DateTime.Now.Subtract(startTime).TotalMilliseconds }, { "NumberOfSubscriptions", customerSubscriptionsView.Count } };

            ApplicationDomain.Instance.TelemetryService.Provider.TrackEvent("GetSubscriptionSummaryAsync", eventProperties, eventMetrics);

            // Sort List of subscriptions based on portal offer name. 
            return new SubscriptionsSummary()
            {
                Subscriptions = customerSubscriptionsView.OrderBy(subscriptionItem => subscriptionItem.FriendlyName),
                SummaryTotal = summaryTotal.ToString("C", responseCulture)      // Currency format.
            };
        }

        /// <summary>
        /// Retrieves the localized operation type string. 
        /// </summary>
        /// <param name="operationType">The Commerce operation type.</param>
        /// <returns>Localized Operation Type string.</returns>
        private static string GetOperationType(CommerceOperationType operationType)
        {
            switch (operationType)
            {
                case CommerceOperationType.AdditionalSeatsPurchase:
                    return Resources.CommerceOperationTypeAddSeats;
                case CommerceOperationType.NewPurchase:
                    return Resources.CommerceOperationTypeAddSubscription;
                case CommerceOperationType.Renewal:
                    return Resources.CommerceOperationTypeRenewSubscription;
                default:
                    return string.Empty;
            }
        }

        /// <summary>
        /// Factory method to create the right payment gateway for this customer. 
        /// </summary>
        /// <param name="operationDescription">The payment operation description.</param>
        /// <param name="customerId">The customer who is transacting.</param>
        /// <returns>The payment gateway instance.</returns>
        private static async Task<IPaymentGateway> CreatePaymentGateway(string operationDescription, string customerId)
        {
            operationDescription.AssertNotEmpty(nameof(operationDescription));
            customerId.AssertNotEmpty(nameof(customerId));

            bool isCustomerPreApproved = false;
            isCustomerPreApproved = await ApplicationDomain.Instance.PreApprovedCustomersRepository.IsCustomerPreApprovedAsync(customerId).ConfigureAwait(false);

            // if customer is preapproved then use PreApprovedGateway else use PayPalGateway. 
            if (isCustomerPreApproved)
            {
                return new PreApprovalGateway(ApplicationDomain.Instance, operationDescription);
            }
            else
            {
                return PaymentGatewayConfig.GetPaymentGatewayInstance(ApplicationDomain.Instance, operationDescription);
            }
        }
    }
}