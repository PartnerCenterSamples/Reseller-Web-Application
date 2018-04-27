// -----------------------------------------------------------------------
// <copyright file="IPaymentGateway.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.CustomerPortal.BusinessLogic.Commerce
{
    using System.Threading.Tasks;
    using Models;

    /// <summary>
    /// The payment gateway contract. Implement this interface to provide payment capabilities.
    /// </summary>
    public interface IPaymentGateway
    {
        /// <summary>
        /// Executes a payment. 
        /// </summary>
        /// <returns>An Authorization code.</returns>
        Task<string> ExecutePaymentAsync();

        /// <summary>
        /// Finalizes an authorized payment.
        /// </summary>
        /// <param name="authorizationCode">The authorization code for the payment to capture.</param>
        /// <returns>A task.</returns>
        Task CaptureAsync(string authorizationCode);

        /// <summary>
        /// Voids an authorized payment.
        /// </summary>
        /// <param name="authorizationCode">The authorization code for the payment to void.</param>
        /// <returns>a Task</returns>
        Task VoidAsync(string authorizationCode);

        /// <summary>
        /// Generates the Payment gateway Url where actual payment collection is done.
        /// </summary>
        /// <param name="returnUrl">Application return Url.</param>
        /// <param name="order">Order information.</param>
        /// <returns>The payment gateway url.</returns>
        Task<string> GeneratePaymentUriAsync(string returnUrl, OrderViewModel order);

        /// <summary>
        /// Retrieves the order details maintained for the payment gateway.  
        /// </summary>
        /// <param name="payerId">The Payer Id.</param>
        /// <param name="paymentId">The Payment Id.</param>
        /// <param name="orderId">The Order Id.</param>
        /// <param name="customerId">The Customer Id.</param>
        /// <returns>The order associated with this payment transaction.</returns>
        Task<OrderViewModel> GetOrderDetailsFromPaymentAsync(string payerId, string paymentId, string orderId, string customerId);

        /// <summary>
        /// validate payment configuration.
        /// </summary>
        /// <param name="paymentConfiguration">The paymentConfiguration contains all the payment configuration data.</param>
        void ValidateConfiguration(PaymentConfiguration paymentConfiguration);

        /// <summary>
        /// creates web experience profile.
        /// </summary>
        /// <param name="paymentConfig">The payment configuration</param>
        /// <param name="brandConfig">The brand configuration</param>
        /// <param name="countryIso2Code">The country code</param>
        /// <returns>returns web profile id</returns>
        string CreateWebExperienceProfile(PaymentConfiguration paymentConfig, BrandingConfiguration brandConfig, string countryIso2Code);
    }
}