// -----------------------------------------------------------------------
// <copyright file="PreApprovalGateway.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace Microsoft.Store.PartnerCenter.CustomerPortal.BusinessLogic.Commerce.PaymentGateways
{
    using System.Globalization;
    using System.Threading.Tasks;
    using Models;

    /// <summary>
    /// Payment gateway which allows for pre approved orders in the storefront. 
    /// </summary>
    public class PreApprovalGateway : DomainObject, IPaymentGateway
    {
        /// <summary>
        /// The order id for an individual order;
        /// </summary>
        private string orderId;

        /// <summary>
        /// The customer id for an individual order;
        /// </summary>
        private string customerId;

        /// <summary>
        /// Initializes a new instance of the <see cref="PreApprovalGateway" /> class.
        /// </summary>
        /// <param name="applicationDomain">The ApplicationDomain.</param>        
        /// <param name="description">The Payment description.</param>
        public PreApprovalGateway(ApplicationDomain applicationDomain, string description) : base(applicationDomain)
        {
        }

        /// <summary>
        /// Stub to finalizes an authorized payment in the gateway.
        /// </summary>
        /// <param name="authorizationCode">The authorization code for the payment to capture.</param>
        /// <returns>A task.</returns>
        public async Task CaptureAsync(string authorizationCode)
        {
            // clean up the order item. 
            await ApplicationDomain.Instance.CustomerOrdersRepository.DeleteAsync(this.orderId, this.customerId);
        }

        /// <summary>
        /// Stub to execute a payment.
        /// </summary>
        /// <returns>Capture string id.</returns>
        public async Task<string> ExecutePaymentAsync()
        {
            return await Task.FromResult("Pre-approvedTransaction");
        }

        /// <summary>
        /// Stub to generate payment url. 
        /// </summary>
        /// <param name="returnUrl">App return url.</param>
        /// <param name="order">Order information.</param>
        /// <returns>Returns the process order page with success flags setup.</returns>
        public async Task<string> GeneratePaymentUriAsync(string returnUrl, OrderViewModel order)
        {
            // will essentially return the returnUrl as is with additional decorations. 
            // persist the order. 
            OrderViewModel orderDetails = await ApplicationDomain.Instance.CustomerOrdersRepository.AddAsync(order);

            // for future cleanup.
            this.orderId = orderDetails.OrderId;
            this.customerId = orderDetails.CustomerId;

            string appReturnUrl = returnUrl + string.Format(CultureInfo.InvariantCulture, "&oid={0}&payment=success&PayerID=PayId&paymentId=PreApproved", orderDetails.OrderId);
            return await Task.FromResult(appReturnUrl);
        }

        /// <summary>
        /// Retrieves the order details maintained for the payment gateway.  
        /// </summary>
        /// <param name="payerId">The Payer Id.</param>
        /// <param name="paymentId">The Payment Id.</param>
        /// <param name="orderId">The Order Id.</param>
        /// <param name="customerId">The Customer Id.</param>
        /// <returns>The order associated with this payment transaction.</returns>
        public async Task<OrderViewModel> GetOrderDetailsFromPaymentAsync(string payerId, string paymentId, string orderId, string customerId)
        {
            // This gateway implementation ignores payerId, paymentId. 
            orderId.AssertNotEmpty(nameof(orderId));
            customerId.AssertNotEmpty(nameof(customerId));

            // for future cleanup. 
            this.orderId = orderId;
            this.customerId = customerId;

            // use order repository to extract details.             
            return await ApplicationDomain.Instance.CustomerOrdersRepository.RetrieveAsync(orderId, customerId);
        }

        /// <summary>
        /// Stub to Void payment.
        /// </summary>
        /// <param name="authorizationCode">The authorization code for the payment to void.</param>
        /// <returns>a Task</returns>
        public async Task VoidAsync(string authorizationCode)
        {
            // clean up the order item. 
            await ApplicationDomain.Instance.CustomerOrdersRepository.DeleteAsync(this.orderId, this.customerId);
        }
    }
}