// -----------------------------------------------------------------------
// <copyright file="OrderController.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.CustomerPortal.Controllers
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
    using Filters.WebApi;
    using Models;
    using Newtonsoft.Json;    

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
        /// <returns>The payment url from PayPal.</returns>
        [@Authorize(UserRole = UserRole.Customer)]
        [HttpPost]
        [Route("Prepare")]
        public async Task<string> PrepareOrderForAuthenticatedCustomer([FromBody]OrderViewModel orderDetails)
        {           
            var startTime = DateTime.Now;

            if (!ModelState.IsValid)
            {
                var errorList = (from item in ModelState.Values
                                 from error in item.Errors
                                 select error.ErrorMessage).ToList();
                string errorMessage = JsonConvert.SerializeObject(errorList);
                throw new PartnerDomainException(ErrorCode.InvalidInput).AddDetail("ErrorMessage", errorMessage);
            }

            orderDetails.CustomerId = this.Principal.PartnerCenterCustomerId;
            string operationDescription = string.Empty;

            // Validate & Normalize the order information.
            OrderNormalizer orderNormalizer = new OrderNormalizer(ApplicationDomain.Instance, orderDetails);
            switch (orderDetails.OperationType)
            {
                case CommerceOperationType.AdditionalSeatsPurchase:
                    operationDescription = Resources.AddSeatsOperationCaption;
                    orderDetails = await orderNormalizer.NormalizePurchaseAdditionalSeatsOrderAsync();
                    break;
                case CommerceOperationType.NewPurchase:
                    operationDescription = Resources.NewPurchaseOperationCaption;
                    orderDetails = await orderNormalizer.NormalizePurchaseSubscriptionOrderAsync();
                    break;
                case CommerceOperationType.Renewal:
                    operationDescription = Resources.RenewOperationCaption;
                    orderDetails = await orderNormalizer.NormalizeRenewSubscriptionOrderAsync();
                    break;
            }
            
            // prepare the redirect url so that client can redirect to PayPal.             
            string redirectUrl = string.Format(CultureInfo.InvariantCulture, "{0}/#ProcessOrder?ret=true", Request.RequestUri.GetLeftPart(UriPartial.Authority));

            // execute to paypal and get paypal action URI. 
            PayPalGateway paymentGateway = new PayPalGateway(ApplicationDomain.Instance, operationDescription);

            // Capture the request for the customer summary for analysis.
            var eventProperties = new Dictionary<string, string> { { "CustomerId", orderDetails.CustomerId }, { "OperationType", orderDetails.OperationType.ToString() } };

            // Track the event measurements for analysis.
            var eventMetrics = new Dictionary<string, double> { { "ElapsedMilliseconds", DateTime.Now.Subtract(startTime).TotalMilliseconds } };

            ApplicationDomain.Instance.TelemetryService.Provider.TrackEvent("api/order/prepare", eventProperties, eventMetrics);

            return await paymentGateway.GeneratePaymentUriAsync(redirectUrl, orderDetails);
        }

        /// <summary>
        /// Processes the order raised for an authenticated customer. 
        /// </summary>
        /// <param name="paymentId">Payment Id.</param>
        /// <param name="payerId">Payer Id.</param>
        /// <returns>Commerce transaction result.</returns>        
        [@Authorize(UserRole = UserRole.Customer)]
        [HttpGet]
        [Route("Process")]
        public async Task<TransactionResult> ProcessOrderForAuthenticatedCustomer(string paymentId, string payerId)
        {
            var startTime = DateTime.Now;

            // extract order information and create paypal payload.
            string clientCustomerId = this.Principal.PartnerCenterCustomerId;

            paymentId.AssertNotEmpty(nameof(paymentId));
            payerId.AssertNotEmpty(nameof(payerId));
            PayPalGateway paymentGateway = new PayPalGateway(ApplicationDomain.Instance, payerId, paymentId);

            // use payment gateway to extract order information. 
            OrderViewModel orderToProcess = await paymentGateway.GetOrderDetailsFromPaymentAsync();
            CommerceOperations commerceOperation = new CommerceOperations(ApplicationDomain.Instance, clientCustomerId, paymentGateway);
                        
            TransactionResult transactionResult = null;
            switch (orderToProcess.OperationType)
            {
                case CommerceOperationType.Renewal:                    
                    transactionResult = await commerceOperation.RenewSubscriptionAsync(orderToProcess);
                    break;
                case CommerceOperationType.AdditionalSeatsPurchase:
                    transactionResult = await commerceOperation.PurchaseAdditionalSeatsAsync(orderToProcess);
                    break;
                case CommerceOperationType.NewPurchase:                    
                    transactionResult = await commerceOperation.PurchaseAsync(orderToProcess);
                    break;
            }

            // Capture the request for the customer summary for analysis.
            var eventProperties = new Dictionary<string, string> { { "CustomerId", orderToProcess.CustomerId }, { "OperationType", orderToProcess.OperationType.ToString() }, { "PayerId", payerId }, { "PaymentId", paymentId } };

            // Track the event measurements for analysis.
            var eventMetrics = new Dictionary<string, double> { { "ElapsedMilliseconds", DateTime.Now.Subtract(startTime).TotalMilliseconds } };

            ApplicationDomain.Instance.TelemetryService.Provider.TrackEvent("api/order/process", eventProperties, eventMetrics);
            
            return await Task.FromResult(transactionResult);
        }

        /// <summary>
        /// Prepare an order for an unauthenticated customer. Supports only purchase of new subscriptions.
        /// </summary>
        /// <param name="orderDetails">A list of subscriptions to update.</param>
        /// <returns>The payment url from PayPal.</returns>
        [@Authorize(UserRole = UserRole.None)]
        [HttpPost]
        [Route("NewCustomerPrepareOrder")]
        public async Task<string> PrepareOrderForUnAuthenticatedCustomer([FromBody]OrderViewModel orderDetails)
        {
            var startTime = DateTime.Now;

            if (!ModelState.IsValid)
            {
                var errorList = (from item in ModelState.Values
                                 from error in item.Errors
                                 select error.ErrorMessage).ToList();
                string errorMessage = JsonConvert.SerializeObject(errorList);
                throw new PartnerDomainException(ErrorCode.InvalidInput).AddDetail("ErrorMessage", errorMessage);
            }

            // Validate & Normalize the order information.
            OrderNormalizer orderNormalizer = new OrderNormalizer(ApplicationDomain.Instance, orderDetails);            
            orderDetails = await orderNormalizer.NormalizePurchaseSubscriptionOrderAsync();

            // prepare the redirect url so that client can redirect to PayPal.             
            string redirectUrl = string.Format(CultureInfo.InvariantCulture, "{0}/#ProcessOrder?ret=true&customerId={1}", Request.RequestUri.GetLeftPart(UriPartial.Authority), orderDetails.CustomerId);

            // execute to paypal and get paypal action URI. 
            PayPalGateway paymentGateway = new PayPalGateway(ApplicationDomain.Instance, Resources.NewPurchaseOperationCaption);

            // Track the event measurements for analysis.
            var eventMetrics = new Dictionary<string, double> { { "ElapsedMilliseconds", DateTime.Now.Subtract(startTime).TotalMilliseconds } };

            ApplicationDomain.Instance.TelemetryService.Provider.TrackEvent("api/order/NewCustomerPrepareOrder", null, eventMetrics);

            return await paymentGateway.GeneratePaymentUriAsync(redirectUrl, orderDetails);
        }

        /// <summary>
        /// Processes the order raised for an unauthenticated customer. 
        /// </summary>
        /// <param name="customerId">Customer Id generated by register customer call.</param>
        /// <param name="paymentId">Payment Id.</param>
        /// <param name="payerId">Payer Id.</param>
        /// <returns>Subscription Summary.</returns>        
        [@Authorize(UserRole = UserRole.None)]
        [HttpGet]
        [Route("NewCustomerProcessOrder")]
        public async Task<SubscriptionsSummary> ProcessOrderForUnAuthenticatedCustomer(string customerId, string paymentId, string payerId)
        {
            var startTime = DateTime.Now;

            customerId.AssertNotEmpty(nameof(customerId));

            paymentId.AssertNotEmpty(nameof(paymentId));
            payerId.AssertNotEmpty(nameof(payerId));
            PayPalGateway paymentGateway = new PayPalGateway(ApplicationDomain.Instance, payerId, paymentId);

            // use payment gateway to extract order information. 
            OrderViewModel orderToProcess = await paymentGateway.GetOrderDetailsFromPaymentAsync();
            CommerceOperations commerceOperation = new CommerceOperations(ApplicationDomain.Instance, customerId, paymentGateway);
            await commerceOperation.PurchaseAsync(orderToProcess);

            // Capture the request for the customer summary for analysis.
            var eventProperties = new Dictionary<string, string> { { "CustomerId", orderToProcess.CustomerId }, { "PayerId", payerId }, { "PaymentId", paymentId } };

            // Track the event measurements for analysis.
            var eventMetrics = new Dictionary<string, double> { { "ElapsedMilliseconds", DateTime.Now.Subtract(startTime).TotalMilliseconds } };

            ApplicationDomain.Instance.TelemetryService.Provider.TrackEvent("api/order/NewCustomerProcessOrder", eventProperties, eventMetrics);

            return await this.GetSubscriptionSummaryAsync(customerId);
        }

        /// <summary>
        /// Retrieves a summary of all subscriptions and their respective order histories. 
        /// </summary>        
        /// <returns>The Subscription summary used by the client used for rendering purposes.</returns>
        [HttpGet]
        [@Authorize(UserRole = UserRole.Customer)]
        [Route("summary")]
        public async Task<SubscriptionsSummary> SubscriptionSummary()
        {
            return await this.GetSubscriptionSummaryAsync(this.Principal.PartnerCenterCustomerId);
        }

        /// <summary>
        /// Gets the summary of subscriptions for a portal customer. 
        /// </summary>
        /// <param name="customerId">The customer Id.</param>
        /// <returns>Subscription Summary.</returns>
        private async Task<SubscriptionsSummary> GetSubscriptionSummaryAsync(string customerId)
        {
            var startTime = DateTime.Now;
            var customerSubscriptionsTask = ApplicationDomain.Instance.CustomerSubscriptionsRepository.RetrieveAsync(customerId);
            var customerSubscriptionsHistoryTask = ApplicationDomain.Instance.CustomerPurchasesRepository.RetrieveAsync(customerId);
            var allPartnerOffersTask = ApplicationDomain.Instance.OffersRepository.RetrieveAsync();
            var currentMicrosoftOffersTask = ApplicationDomain.Instance.OffersRepository.RetrieveMicrosoftOffersAsync();
            await Task.WhenAll(customerSubscriptionsTask, customerSubscriptionsHistoryTask, allPartnerOffersTask, currentMicrosoftOffersTask);

            var customerSubscriptionsHistory = customerSubscriptionsHistoryTask.Result;

            // retrieve all the partner offers to match against them
            IEnumerable<PartnerOffer> allPartnerOffers = allPartnerOffersTask.Result;

            // retrive all the microsoft offers to match against. 
            IEnumerable<MicrosoftOffer> currentMicrosoftOffers = currentMicrosoftOffersTask.Result;

            // start building the summary.                 
            decimal summaryTotal = 0;

            // format all responses to client using portal locale. 
            CultureInfo responseCulture = new CultureInfo(ApplicationDomain.Instance.PortalLocalization.Locale);
            List<SubscriptionViewModel> customerSubscriptionsView = new List<SubscriptionViewModel>();

            // iterate through and build the list of customer's subscriptions. 
            foreach (var subscription in customerSubscriptionsTask.Result)
            {
                decimal subscriptionTotal = 0;
                int licenseTotal = 0;
                List<SubscriptionHistory> historyItems = new List<SubscriptionHistory>();

                // collect the list of history items for this subcription.  
                var subscriptionHistoryList = customerSubscriptionsHistory
                    .Where(historyItem => historyItem.SubscriptionId == subscription.SubscriptionId)
                    .OrderBy(historyItem => historyItem.TransactionDate);

                // iterate through and build the SubsriptionHistory for this subscription. 
                foreach (var historyItem in subscriptionHistoryList)
                {
                    decimal orderTotal = Math.Round(historyItem.SeatPrice * historyItem.SeatsBought, responseCulture.NumberFormat.CurrencyDecimalDigits);
                    historyItems.Add(new SubscriptionHistory()
                    {
                        OrderTotal = orderTotal.ToString("C", responseCulture),                                 // Currency format.
                        PricePerSeat = historyItem.SeatPrice.ToString("C", responseCulture),                    // Currency format. 
                        SeatsBought = historyItem.SeatsBought.ToString("G", responseCulture),                   // General format.  
                        OrderDate = historyItem.TransactionDate.ToLocalTime().ToString("d", responseCulture),   // Short date format. 
                        OperationType = this.GetOperationType(historyItem.PurchaseType)                         // Localized Operation type string. 
                    });

                    // Increment the subscription total. 
                    licenseTotal += historyItem.SeatsBought;

                    // Increment the subscription total. 
                    subscriptionTotal += orderTotal;
                }

                var partnerOfferItem = allPartnerOffers.FirstOrDefault(offer => offer.Id == subscription.PartnerOfferId);
                string subscriptionTitle = partnerOfferItem.Title;
                string portalOfferId = partnerOfferItem.Id;
                decimal portalOfferPrice = partnerOfferItem.Price;

                DateTime subscriptionExpiryDate = subscription.ExpiryDate.ToUniversalTime();
                int remainingDays = (subscriptionExpiryDate.Date - DateTime.UtcNow.Date).Days;
                bool isRenewable = remainingDays <= 30;
                bool isEditable = DateTime.UtcNow.Date <= subscriptionExpiryDate.Date;

                // TODO :: Handle Microsoft offer being pulled back due to EOL. 

                // Temporarily mark this partnerOffer item as inactive and dont allow store front customer to manage this subscription. 
                var alignedMicrosoftOffer = currentMicrosoftOffers.FirstOrDefault(offer => offer.Offer.Id == partnerOfferItem.MicrosoftOfferId);
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
            var eventProperties = new Dictionary<string, string> { { "CustomerId", customerId } };

            // Track the event measurements for analysis.
            var eventMetrics = new Dictionary<string, double> { { "ElapsedMilliseconds", DateTime.Now.Subtract(startTime).TotalMilliseconds }, { "NumberOfSubscriptions", customerSubscriptionsView.Count } };

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
        private string GetOperationType(CommerceOperationType operationType)
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
    }
}
