// -----------------------------------------------------------------------
// <copyright file="CommerceOperations.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.CustomerPortal.BusinessLogic.Commerce
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Infrastructure;
    using Models;
    using PartnerCenter.Models.Orders;
    using Transactions;

    /// <summary>
    /// Implements the portal commerce transactions.
    /// </summary>
    public class CommerceOperations : DomainObject, ICommerceOperations
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommerceOperations"/> class.
        /// </summary>
        /// <param name="applicationDomain">An application domain instance.</param>
        /// <param name="customerId">The customer ID who owns the transaction.</param>
        /// <param name="paymentGateway">A payment gateway to use for processing payments resulting from the transaction.</param>
        public CommerceOperations(ApplicationDomain applicationDomain, string customerId, IPaymentGateway paymentGateway) : base(applicationDomain)
        {
            customerId.AssertNotEmpty(nameof(customerId));
            paymentGateway.AssertNotNull(nameof(paymentGateway));

            this.CustomerId = customerId;
            this.PaymentGateway = paymentGateway;
        }

        /// <summary>
        /// Gets the customer ID who owns the transaction.
        /// </summary>
        public string CustomerId { get; private set; }

        /// <summary>
        /// Gets the payment gateway used to process payments.
        /// </summary>
        public IPaymentGateway PaymentGateway { get; private set; }

        /// <summary>
        /// Calculates the amount to charge for buying an extra additional seat for the remainder of a subscription's lease.
        /// </summary>
        /// <param name="expiryDate">The subscription's expiry date.</param>
        /// <param name="yearlyRatePerSeat">The subscription's yearly price per seat.</param>
        /// <returns>The prorated amount to charge for the new extra seat.</returns>
        public static decimal CalculateProratedSeatCharge(DateTime expiryDate, decimal yearlyRatePerSeat)
        { 
            DateTime rightNow = DateTime.UtcNow;
            expiryDate = expiryDate.ToUniversalTime();

            decimal dailyChargePerSeat = yearlyRatePerSeat / 365m;
            
            // round up the remaining days in case there was a fraction and ensure it does not exceed 365 days
            decimal remainingDaysTillExpiry = Math.Ceiling(Convert.ToDecimal((expiryDate - rightNow).TotalDays));
            remainingDaysTillExpiry = Math.Min(remainingDaysTillExpiry, 365);

            return remainingDaysTillExpiry * dailyChargePerSeat;
        }

        /// <summary>
        /// Purchases one or more partner offers.
        /// </summary>
        /// <param name="order">The order to execute.</param>
        /// <returns>A transaction result which summarizes its outcome.</returns>
        public async Task<TransactionResult> PurchaseAsync(OrderViewModel order)
        {
            // use the normalizer to validate the order. 
            OrderNormalizer orderNormalizer = new OrderNormalizer(this.ApplicationDomain, order);
            order = await orderNormalizer.NormalizePurchaseSubscriptionOrderAsync().ConfigureAwait(false);

            // build the purchase line items. 
            List<PurchaseLineItem> purchaseLineItems = new List<PurchaseLineItem>();
            foreach (var orderItem in order.Subscriptions)
            {
                string offerId = orderItem.OfferId;
                int quantity = orderItem.Quantity;

                purchaseLineItems.Add(new PurchaseLineItem(offerId, quantity));
            }
            
            // associate line items in order to partner offers. 
            var lineItemsWithOffers = await this.AssociateWithPartnerOffersAsync(purchaseLineItems).ConfigureAwait(false);
            ICollection<IBusinessTransaction> subTransactions = new List<IBusinessTransaction>();            

            // prepare payment authorization
            var paymentAuthorization = new AuthorizePayment(this.PaymentGateway);
            subTransactions.Add(paymentAuthorization);

            // build the Partner Center order and pass it to the place order transaction
            Order partnerCenterPurchaseOrder = this.BuildPartnerCenterOrder(lineItemsWithOffers);

            var placeOrder = new PlaceOrder(
                this.ApplicationDomain.PartnerCenterClient.Customers.ById(this.CustomerId),
                partnerCenterPurchaseOrder);
            subTransactions.Add(placeOrder);

            // configure a transaction to save the new resulting subscriptions and purchases into persistence
            var persistSubscriptionsAndPurchases = new PersistNewlyPurchasedSubscriptions(
                this.CustomerId,
                this.ApplicationDomain.CustomerSubscriptionsRepository,
                this.ApplicationDomain.CustomerPurchasesRepository,
                () => new Tuple<Order, IEnumerable<PurchaseLineItemWithOffer>>(placeOrder.Result, lineItemsWithOffers));

            subTransactions.Add(persistSubscriptionsAndPurchases);

            // configure a capture payment transaction and let it read the auth code from the payment authorization output
            var capturePayment = new CapturePayment(this.PaymentGateway, () => paymentAuthorization.Result);
            subTransactions.Add(capturePayment);
            
            // build an aggregated transaction from the previous steps and execute it as a whole
            await RunAggregatedTransaction(subTransactions);
            
            return new TransactionResult(persistSubscriptionsAndPurchases.Result, DateTime.UtcNow);
        }

        /// <summary>
        /// Purchases additional seats for an existing subscription the customer has already bought.
        /// </summary>
        /// <param name="order">The order to execute.</param>
        /// <returns>A transaction result which summarizes its outcome.</returns>
        public async Task<TransactionResult> PurchaseAdditionalSeatsAsync(OrderViewModel order)
        {
            // use the normalizer to validate the order.
            OrderNormalizer orderNormalizer = new OrderNormalizer(this.ApplicationDomain, order);
            order = await orderNormalizer.NormalizePurchaseAdditionalSeatsOrderAsync();

            List<OrderSubscriptionItemViewModel> orderSubscriptions = order.Subscriptions.ToList();
            string subscriptionId = orderSubscriptions.First().SubscriptionId;
            int seatsToPurchase = orderSubscriptions.First().Quantity;
            decimal proratedSeatCharge = orderSubscriptions.First().SeatPrice;
            string partnerOfferId = orderSubscriptions.First().PartnerOfferId;

            // we will add up the transactions here
            ICollection<IBusinessTransaction> subTransactions = new List<IBusinessTransaction>();

            // configure a transaction to charge the payment gateway with the prorated rate
            var paymentAuthorization = new AuthorizePayment(this.PaymentGateway);
            subTransactions.Add(paymentAuthorization);
            
            // configure a purchase additional seats transaction with the requested seats to purchase
            subTransactions.Add(new PurchaseExtraSeats(
                this.ApplicationDomain.PartnerCenterClient.Customers.ById(this.CustomerId).Subscriptions.ById(subscriptionId),
                seatsToPurchase));

            DateTime rightNow = DateTime.UtcNow;
            
            // record the purchase in our purchase store
            subTransactions.Add(new RecordPurchase(
                this.ApplicationDomain.CustomerPurchasesRepository,
                new CustomerPurchaseEntity(CommerceOperationType.AdditionalSeatsPurchase, Guid.NewGuid().ToString(), this.CustomerId, subscriptionId, seatsToPurchase, proratedSeatCharge, rightNow)));

            // add a capture payment to the transaction pipeline
            subTransactions.Add(new CapturePayment(this.PaymentGateway, () => paymentAuthorization.Result));
            
            // build an aggregated transaction from the previous steps and execute it as a whole
            await CommerceOperations.RunAggregatedTransaction(subTransactions).ConfigureAwait(false);
            
            var additionalSeatsPurchaseResult = new TransactionResultLineItem(
                subscriptionId, 
                partnerOfferId,
                seatsToPurchase,
                proratedSeatCharge,
                seatsToPurchase * proratedSeatCharge);

            return new TransactionResult(                
                new TransactionResultLineItem[] { additionalSeatsPurchaseResult },
                rightNow);
        }

        /// <summary>
        /// Renews an existing subscription for a customer.
        /// </summary>
        /// <param name="order">The order to execute.</param>
        /// <returns>A transaction result which summarizes its outcome.</returns>
        public async Task<TransactionResult> RenewSubscriptionAsync(OrderViewModel order)
        {
            // use the normalizer to validate the order.
            OrderNormalizer orderNormalizer = new OrderNormalizer(this.ApplicationDomain, order);
            order = await orderNormalizer.NormalizeRenewSubscriptionOrderAsync();

            List<OrderSubscriptionItemViewModel> orderSubscriptions = order.Subscriptions.ToList();
            string subscriptionId = orderSubscriptions.First().SubscriptionId;
            string partnerOfferId = orderSubscriptions.First().PartnerOfferId;
            decimal partnerOfferPrice = orderSubscriptions.First().SeatPrice;
            DateTime subscriptionExpiryDate = orderSubscriptions.First().SubscriptionExpiryDate;
            int quantity = orderSubscriptions.First().Quantity;            
            decimal totalCharge = Math.Round(quantity * partnerOfferPrice, Resources.Culture.NumberFormat.CurrencyDecimalDigits);            

            // retrieve the subscription from Partner Center
            var subscriptionOperations = this.ApplicationDomain.PartnerCenterClient.Customers.ById(this.CustomerId).Subscriptions.ById(subscriptionId);
            var partnerCenterSubscription = await subscriptionOperations.GetAsync();

            // we will add up the transactions here
            ICollection<IBusinessTransaction> subTransactions = new List<IBusinessTransaction>();

            // configure a transaction to charge the payment gateway with the prorated rate
            var paymentAuthorization = new AuthorizePayment(this.PaymentGateway);
            subTransactions.Add(paymentAuthorization);

            // add a renew subscription transaction to the pipeline
            subTransactions.Add(new RenewSubscription(
                subscriptionOperations,
                partnerCenterSubscription));

            DateTime rightNow = DateTime.UtcNow;

            // record the renewal in our purchase store
            subTransactions.Add(new RecordPurchase(
                this.ApplicationDomain.CustomerPurchasesRepository, 
                new CustomerPurchaseEntity(CommerceOperationType.Renewal, Guid.NewGuid().ToString(), this.CustomerId, subscriptionId, partnerCenterSubscription.Quantity, partnerOfferPrice, rightNow)));
            
            // extend the expiry date by one year
            subTransactions.Add(new UpdatePersistedSubscription(
                this.ApplicationDomain.CustomerSubscriptionsRepository,
                new CustomerSubscriptionEntity(this.CustomerId, subscriptionId, partnerOfferId, subscriptionExpiryDate.AddYears(1))));           

            // add a capture payment to the transaction pipeline
            subTransactions.Add(new CapturePayment(this.PaymentGateway, () => paymentAuthorization.Result));

            // run the pipeline
            await CommerceOperations.RunAggregatedTransaction(subTransactions);

            var renewSubscriptionResult = new TransactionResultLineItem(
                subscriptionId,
                partnerOfferId,
                partnerCenterSubscription.Quantity,
                partnerOfferPrice,
                totalCharge);

            return new TransactionResult(                
                new TransactionResultLineItem[] { renewSubscriptionResult },
                rightNow);
        }

        /// <summary>
        /// Runs a given list of transactions as a whole.
        /// </summary>
        /// <param name="subTransactions">A collection of transactions to run.</param>
        /// <returns>A task.</returns>
        private static async Task RunAggregatedTransaction(IEnumerable<IBusinessTransaction> subTransactions)
        {
            // build an aggregated transaction from the given transactions
            var aggregateTransaction = new SequentialAggregateTransaction(subTransactions);

            try
            {
                // execute it
                await aggregateTransaction.ExecuteAsync();
            }
            catch (Exception transactionFailure)
            {
                if (transactionFailure.IsFatal())
                {
                    throw;
                }

                // roll back the whole transaction
                await aggregateTransaction.RollbackAsync();

                // report the error
                throw;
            }
        }

        /// <summary>
        /// Binds each purchase line item with the partner offer it is requesting.
        /// </summary>
        /// <param name="purchaseLineItems">A collection of purchase line items.</param>
        /// <returns>The requested association.</returns>
        private async Task<IEnumerable<PurchaseLineItemWithOffer>> AssociateWithPartnerOffersAsync(IEnumerable<PurchaseLineItem> purchaseLineItems)
        {
            // retrieve all the partner offers to match against them
            IEnumerable<PartnerOffer> allPartnerOffers = await this.ApplicationDomain.OffersRepository.RetrieveAsync();

            ICollection<PurchaseLineItemWithOffer> lineItemToOfferAssociations = new List<PurchaseLineItemWithOffer>();

            foreach (var lineItem in purchaseLineItems)
            {
                if (lineItem == null)
                {
                    throw new ArgumentException("a line item is null");
                }

                PartnerOffer offerToPurchase = allPartnerOffers.Where(offer => offer.Id == lineItem.PartnerOfferId).FirstOrDefault();

                // associate the line item with the partner offer
                lineItemToOfferAssociations.Add(new PurchaseLineItemWithOffer(lineItem, offerToPurchase));
            }

            return lineItemToOfferAssociations;
        }

        /// <summary>
        /// Builds a Microsoft Partner Center order from a list of purchase line items.
        /// </summary>
        /// <param name="purchaseLineItems">The purchase line items.</param>
        /// <returns>The Partner Center Order.</returns>
        private Order BuildPartnerCenterOrder(IEnumerable<PurchaseLineItemWithOffer> purchaseLineItems)
        {
            int lineItemNumber = 0;
            ICollection<OrderLineItem> partnerCenterOrderLineItems = new List<OrderLineItem>();

            // build the Partner Center order line items
            foreach (var lineItem in purchaseLineItems)
            {
                // add the line items to the partner center order and calculate the price to charge
                partnerCenterOrderLineItems.Add(new OrderLineItem()
                {
                    OfferId = lineItem.PartnerOffer.MicrosoftOfferId,
                    Quantity = lineItem.PurchaseLineItem.Quantity,
                    LineItemNumber = lineItemNumber++
                });
            }

            // bundle the order line items into a partner center order
            return new Order()
            {
                ReferenceCustomerId = this.CustomerId,
                LineItems = partnerCenterOrderLineItems
            };
        }
    }
}