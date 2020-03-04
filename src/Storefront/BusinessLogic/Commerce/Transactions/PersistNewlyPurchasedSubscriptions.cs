// -----------------------------------------------------------------------
// <copyright file="PersistNewlyPurchasedSubscriptions.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Storefront.BusinessLogic.Commerce.Transactions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Infrastructure;
    using Models;
    using PartnerCenter.Models.Orders;

    /// <summary>
    /// A transaction which records all the resulting subscriptions of a Partner Center order into persistence. The customer subscriptions
    /// and purchases tables will store the new subscriptions and their purchase history.
    /// </summary>
    public class PersistNewlyPurchasedSubscriptions :
        IBusinessTransactionWithInput<Tuple<Order, IEnumerable<PurchaseLineItemWithOffer>>>,
        IBusinessTransactionWithOutput<IEnumerable<TransactionResultLineItem>>
    {
        /// <summary>
        /// An aggregate transaction which add all the subscriptions from an order to persistence.
        /// </summary>
        private IBusinessTransaction bulkSubscriptionPersistenceTransaction = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="PersistNewlyPurchasedSubscriptions"/> class.
        /// </summary>
        /// <param name="customerId">The ID of the customer who performed the purchases.</param>
        /// <param name="subscriptionsRepository">The customer subscriptions repository used to persist the subscriptions.</param>
        /// <param name="purchasesRepository">The customer purchases repository used to persist the purchases.</param>
        /// <param name="acquireInputsFunction">The function used to obtain the order and the list of purchase line items associated with their partner offers.</param>
        public PersistNewlyPurchasedSubscriptions(
            string customerId,
            CustomerSubscriptionsRepository subscriptionsRepository,
            CustomerPurchasesRepository purchasesRepository,
            Func<Tuple<Order, IEnumerable<PurchaseLineItemWithOffer>>> acquireInputsFunction)
        {
            customerId.AssertNotEmpty(nameof(customerId));
            subscriptionsRepository.AssertNotNull(nameof(subscriptionsRepository));
            purchasesRepository.AssertNotNull(nameof(purchasesRepository));
            acquireInputsFunction.AssertNotNull(nameof(acquireInputsFunction));

            CustomerId = customerId;
            CustomerSubscriptionsRepository = subscriptionsRepository;
            CustomerPurchasesRepository = purchasesRepository;
            AcquireInput = acquireInputsFunction;
        }

        /// <summary>
        /// Gets the ID of the customer who owns the transaction.
        /// </summary>
        public string CustomerId { get; private set; }

        /// <summary>
        /// Gets the customer subscriptions repository used to persist the subscriptions.
        /// </summary>
        public CustomerSubscriptionsRepository CustomerSubscriptionsRepository { get; private set; }

        /// <summary>
        /// Gets the customer purchases repository used to persist the purchases.
        /// </summary>
        public CustomerPurchasesRepository CustomerPurchasesRepository { get; private set; }

        /// <summary>
        /// Gets the function used to obtain the order and the list of purchase line items associated with their partner offers.
        /// </summary>
        public Func<Tuple<Order, IEnumerable<PurchaseLineItemWithOffer>>> AcquireInput { get; private set; }

        /// <summary>
        /// Gets the result from running this transaction.
        /// </summary>
        public IEnumerable<TransactionResultLineItem> Result { get; private set; }

        /// <summary>
        /// Records all the resulting subscriptions as well as their initial purchase history into persistence.
        /// </summary>
        /// <returns>A task.</returns>
        public async Task ExecuteAsync()
        {
            Tuple<Order, IEnumerable<PurchaseLineItemWithOffer>> inputs = AcquireInput.Invoke();
            Order partnerCenterPurchaseOrder = inputs.Item1;
            IEnumerable<PurchaseLineItemWithOffer> purchaseLineItems = inputs.Item2;

            ICollection<TransactionResultLineItem> transactionResultLineItems = new List<TransactionResultLineItem>();
            ICollection<IBusinessTransaction> persistenceTransactions = new List<IBusinessTransaction>();

            DateTime rightNow = DateTime.UtcNow;
            DateTime expirationDate = rightNow;

            BrandingConfiguration portalBranding = await ApplicationDomain.Instance.PortalBranding.RetrieveAsync().ConfigureAwait(false);

            switch (portalBranding.BillingCycle)
            {
                case BillingCycleType.Annual:
                    expirationDate = expirationDate.AddYears(1);
                    break;
                case BillingCycleType.Monthly:
                    expirationDate = expirationDate.AddMonths(1);
                    break;
                default:
                    throw new NotImplementedException($"Billing cycle {portalBranding.BillingCycle} is not implemented");
            }

            foreach (OrderLineItem orderLineItem in partnerCenterPurchaseOrder.LineItems)
            {
                PartnerOffer matchingPartnerOffer = purchaseLineItems.ElementAt(orderLineItem.LineItemNumber).PartnerOffer;

                // add a record new customer subscription transaction for the current line item
                persistenceTransactions.Add(new RecordNewCustomerSubscription(
                    CustomerSubscriptionsRepository,
                    new CustomerSubscriptionEntity(CustomerId, orderLineItem.SubscriptionId, matchingPartnerOffer.Id, expirationDate)));

                // add a record purchase history for the current line item
                persistenceTransactions.Add(new RecordPurchase(
                    CustomerPurchasesRepository,
                    new CustomerPurchaseEntity(CommerceOperationType.NewPurchase, Guid.NewGuid().ToString(), CustomerId, orderLineItem.SubscriptionId, orderLineItem.Quantity, matchingPartnerOffer.Price, rightNow)));

                // build the transaction result line item
                transactionResultLineItems.Add(new TransactionResultLineItem(
                    orderLineItem.SubscriptionId,
                    matchingPartnerOffer.Id,
                    orderLineItem.Quantity,
                    matchingPartnerOffer.Price,
                    matchingPartnerOffer.Price * orderLineItem.Quantity));
            }

            // bundle up all the transactions together
            bulkSubscriptionPersistenceTransaction = new SequentialAggregateTransaction(persistenceTransactions);

            // execute it!
            await bulkSubscriptionPersistenceTransaction.ExecuteAsync().ConfigureAwait(false);

            // store the reuslting transaction line items
            Result = transactionResultLineItems;
        }

        /// <summary>
        /// Rollback all the inserts.
        /// </summary>
        /// <returns>A task.</returns>
        public async Task RollbackAsync()
        {
            if (bulkSubscriptionPersistenceTransaction != null)
            {
                await bulkSubscriptionPersistenceTransaction.RollbackAsync().ConfigureAwait(false);
                bulkSubscriptionPersistenceTransaction = null;
            }
        }
    }
}