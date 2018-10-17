// -----------------------------------------------------------------------
// <copyright file="OrderNormalizer.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Storefront.BusinessLogic.Commerce
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Exceptions;
    using Models;

    /// <summary>
    /// Implements the normalizers for orders. 
    /// </summary>
    public class OrderNormalizer : DomainObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OrderNormalizer"/> class.
        /// </summary>
        /// <param name="applicationDomain">An application domain instance.</param>
        /// <param name="order">The order which will be normalized.</param>
        public OrderNormalizer(ApplicationDomain applicationDomain, OrderViewModel order) : base(applicationDomain)
        {
            order.AssertNotNull(nameof(order));

            Order = order;
        }

        /// <summary>
        /// Gets the Order instance.
        /// </summary>
        public OrderViewModel Order { get; private set; }

        /// <summary>
        /// Normalizes an order to renew a subscription.
        /// </summary>        
        /// <returns>Normalized order.</returns>
        public async Task<OrderViewModel> NormalizeRenewSubscriptionOrderAsync()
        {
            OrderViewModel order = Order;
            order.CustomerId.AssertNotEmpty(nameof(order.CustomerId));
            if (order.OperationType != CommerceOperationType.Renewal)
            {
                throw new PartnerDomainException(ErrorCode.InvalidInput, Resources.InvalidOperationForOrderMessage).AddDetail("Field", "OperationType");
            }

            // create result order object prefilling it with operation type & customer id.
            OrderViewModel orderResult = new OrderViewModel()
            {
                CustomerId = order.CustomerId,
                OrderId = order.OrderId,
                OperationType = order.OperationType
            };

            order.Subscriptions.AssertNotNull(nameof(order.Subscriptions));
            List<OrderSubscriptionItemViewModel> orderSubscriptions = order.Subscriptions.ToList();
            if (!(orderSubscriptions.Count == 1))
            {
                throw new PartnerDomainException(ErrorCode.InvalidInput, Resources.MoreThanOneSubscriptionUpdateErrorMessage);
            }

            string subscriptionId = orderSubscriptions.First().SubscriptionId;
            subscriptionId.AssertNotEmpty(nameof(subscriptionId)); // is Required for the commerce operation.             

            // grab the customer subscription from our store
            CustomerSubscriptionEntity subscriptionToAugment = await GetSubscriptionAsync(subscriptionId, order.CustomerId).ConfigureAwait(false);

            // retrieve the partner offer this subscription relates to, we need to know the current price
            PartnerOffer partnerOffer = await ApplicationDomain.Instance.OffersRepository.RetrieveAsync(subscriptionToAugment.PartnerOfferId).ConfigureAwait(false);

            if (partnerOffer.IsInactive)
            {
                // renewing deleted offers is prohibited
                throw new PartnerDomainException(ErrorCode.PurchaseDeletedOfferNotAllowed).AddDetail("Id", partnerOffer.Id);
            }

            // retrieve the subscription from Partner Center
            Subscriptions.ISubscription subscriptionOperations = ApplicationDomain.Instance.PartnerCenterClient.Customers.ById(order.CustomerId).Subscriptions.ById(subscriptionId);
            PartnerCenter.Models.Subscriptions.Subscription partnerCenterSubscription = await subscriptionOperations.GetAsync().ConfigureAwait(false);

            List<OrderSubscriptionItemViewModel> resultOrderSubscriptions = new List<OrderSubscriptionItemViewModel>
            {
                new OrderSubscriptionItemViewModel()
                {
                    OfferId = subscriptionId,
                    SubscriptionId = subscriptionId,
                    PartnerOfferId = subscriptionToAugment.PartnerOfferId,
                    SubscriptionExpiryDate = subscriptionToAugment.ExpiryDate,
                    Quantity = partnerCenterSubscription.Quantity,
                    SeatPrice = partnerOffer.Price,
                    SubscriptionName = partnerOffer.Title
                }
            };

            orderResult.Subscriptions = resultOrderSubscriptions;
            return await Task.FromResult(orderResult).ConfigureAwait(false);
        }

        /// <summary>
        /// Normalizes an order to purchase net new subscriptions.
        /// </summary>        
        /// <returns>Normalized order.</returns>
        public async Task<OrderViewModel> NormalizePurchaseSubscriptionOrderAsync()
        {
            OrderViewModel order = Order;
            order.CustomerId.AssertNotEmpty(nameof(order.CustomerId));
            if (order.OperationType != CommerceOperationType.NewPurchase)
            {
                throw new PartnerDomainException(ErrorCode.InvalidInput, Resources.InvalidOperationForOrderMessage).AddDetail("Field", "OperationType");
            }

            // create result order object prefilling it with operation type & customer id.
            OrderViewModel orderResult = new OrderViewModel()
            {
                CustomerId = order.CustomerId,
                OrderId = order.OrderId,
                OperationType = order.OperationType
            };

            order.Subscriptions.AssertNotNull(nameof(order.Subscriptions));
            List<OrderSubscriptionItemViewModel> orderSubscriptions = order.Subscriptions.ToList();
            if (orderSubscriptions.Count < 1)
            {
                throw new Exception(Resources.NotEnoughItemsInOrderErrorMessage);
            }

            // retrieve all the partner offers to match against them
            IEnumerable<PartnerOffer> allPartnerOffers = await ApplicationDomain.Instance.OffersRepository.RetrieveAsync().ConfigureAwait(false);

            List<OrderSubscriptionItemViewModel> resultOrderSubscriptions = new List<OrderSubscriptionItemViewModel>();
            foreach (OrderSubscriptionItemViewModel lineItem in orderSubscriptions)
            {
                PartnerOffer offerToPurchase = allPartnerOffers.Where(offer => offer.Id == lineItem.SubscriptionId).FirstOrDefault();

                if (offerToPurchase == null)
                {
                    // oops, this offer Id is unknown to us
                    throw new PartnerDomainException(ErrorCode.PartnerOfferNotFound).AddDetail("Id", lineItem.SubscriptionId);
                }
                else if (offerToPurchase.IsInactive)
                {
                    // purchasing deleted offers is prohibited
                    throw new PartnerDomainException(ErrorCode.PurchaseDeletedOfferNotAllowed).AddDetail("Id", offerToPurchase.Id);
                }

                // populate details for each order subscription item to purchase. 
                resultOrderSubscriptions.Add(new OrderSubscriptionItemViewModel()
                {
                    OfferId = offerToPurchase.Id,
                    SubscriptionId = offerToPurchase.Id,
                    Quantity = lineItem.Quantity,
                    SeatPrice = offerToPurchase.Price,
                    SubscriptionName = offerToPurchase.Title
                });
            }

            orderResult.Subscriptions = resultOrderSubscriptions;
            return await Task.FromResult(orderResult).ConfigureAwait(false);
        }

        /// <summary>
        /// Normalizes an order to add seats to a subscription.
        /// </summary>        
        /// <returns>Normalized order.</returns>
        public async Task<OrderViewModel> NormalizePurchaseAdditionalSeatsOrderAsync()
        {
            OrderViewModel order = this.Order;
            order.CustomerId.AssertNotEmpty(nameof(order.CustomerId));
            if (order.OperationType != CommerceOperationType.AdditionalSeatsPurchase)
            {
                throw new PartnerDomainException(ErrorCode.InvalidInput, Resources.InvalidOperationForOrderMessage).AddDetail("Field", "OperationType");
            }

            // create result order object prefilling it with operation type & customer id.
            OrderViewModel orderResult = new OrderViewModel()
            {
                CustomerId = order.CustomerId,
                OrderId = order.OrderId,
                OperationType = order.OperationType
            };

            order.Subscriptions.AssertNotNull(nameof(order.Subscriptions));
            List<OrderSubscriptionItemViewModel> orderSubscriptions = order.Subscriptions.ToList();
            if (!(orderSubscriptions.Count == 1))
            {
                throw new PartnerDomainException(ErrorCode.InvalidInput).AddDetail("ErrorMessage", Resources.MoreThanOneSubscriptionUpdateErrorMessage);
            }

            string subscriptionId = orderSubscriptions.First().SubscriptionId;
            int seatsToPurchase = orderSubscriptions.First().Quantity;
            subscriptionId.AssertNotEmpty(nameof(subscriptionId)); // is Required for the commerce operation.             
            seatsToPurchase.AssertPositive("seatsToPurchase");

            // grab the customer subscription from our store
            CustomerSubscriptionEntity subscriptionToAugment = await GetSubscriptionAsync(subscriptionId, order.CustomerId).ConfigureAwait(false);

            // retrieve the partner offer this subscription relates to, we need to know the current price
            PartnerOffer partnerOffer = await ApplicationDomain.Instance.OffersRepository.RetrieveAsync(subscriptionToAugment.PartnerOfferId).ConfigureAwait(false);

            if (partnerOffer.IsInactive)
            {
                // renewing deleted offers is prohibited
                throw new PartnerDomainException(ErrorCode.PurchaseDeletedOfferNotAllowed).AddDetail("Id", partnerOffer.Id);
            }

            // retrieve the subscription from Partner Center
            Subscriptions.ISubscription subscriptionOperations = ApplicationDomain.Instance.PartnerCenterClient.Customers.ById(order.CustomerId).Subscriptions.ById(subscriptionId);
            PartnerCenter.Models.Subscriptions.Subscription partnerCenterSubscription = await subscriptionOperations.GetAsync().ConfigureAwait(false);

            // if subscription expiry date.Date is less than today's UTC date then subcription has expired. 
            if (subscriptionToAugment.ExpiryDate.Date < DateTime.UtcNow.Date)
            {
                // this subscription has already expired, don't permit adding seats until the subscription is renewed
                throw new PartnerDomainException(ErrorCode.SubscriptionExpired);
            }

            decimal proratedSeatCharge = Math.Round(CommerceOperations.CalculateProratedSeatCharge(subscriptionToAugment.ExpiryDate, partnerOffer.Price), Resources.Culture.NumberFormat.CurrencyDecimalDigits);
            decimal totalCharge = Math.Round(proratedSeatCharge * seatsToPurchase, Resources.Culture.NumberFormat.CurrencyDecimalDigits);

            List<OrderSubscriptionItemViewModel> resultOrderSubscriptions = new List<OrderSubscriptionItemViewModel>
            {
                new OrderSubscriptionItemViewModel()
                {
                    OfferId = subscriptionId,
                    SubscriptionId = subscriptionId,
                    PartnerOfferId = subscriptionToAugment.PartnerOfferId,
                    SubscriptionExpiryDate = subscriptionToAugment.ExpiryDate,
                    Quantity = seatsToPurchase,
                    SeatPrice = proratedSeatCharge,
                    SubscriptionName = partnerOffer.Title
                }
            };

            orderResult.Subscriptions = resultOrderSubscriptions;
            return await Task.FromResult(orderResult).ConfigureAwait(false);
        }

        /// <summary>
        /// Retrieves a customer subscription from persistence.
        /// </summary>
        /// <param name="subscriptionId">The subscription ID.</param>
        /// <param name="customerId">The customer ID.</param>
        /// <returns>The matching subscription.</returns>
        private async Task<CustomerSubscriptionEntity> GetSubscriptionAsync(string subscriptionId, string customerId)
        {
            // grab the customer subscription from our store
            IEnumerable<CustomerSubscriptionEntity> customerSubscriptions = await ApplicationDomain.Instance.CustomerSubscriptionsRepository.RetrieveAsync(customerId).ConfigureAwait(false);
            CustomerSubscriptionEntity subscriptionToAugment = customerSubscriptions.Where(subscription => subscription.SubscriptionId == subscriptionId).FirstOrDefault();

            if (subscriptionToAugment == null)
            {
                throw new PartnerDomainException(ErrorCode.SubscriptionNotFound);
            }

            return subscriptionToAugment;
        }
    }
}