// -----------------------------------------------------------------------
// <copyright file="ApiCalls.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.CustomerPortal.BusinessLogic.Commerce.PaymentGateways.PayUMoney
{
    using System.Collections.Specialized;
    using System.Net.Http;
    using System.Threading.Tasks;
    using BusinessLogic;
    using Models;

    /// <summary>
    /// class definition
    /// </summary>
    public class ApiCalls
    {
        /// <summary>
        /// Http client object to call web API
        /// </summary>
        private static HttpClient client = new HttpClient();

        /// <summary>
        /// Get payment response. 
        /// </summary>
        /// <param name="paymentId">The PaymentId.</param>
        /// <returns>returns PayUMoneyPaymentResponse.</returns>
        public static async Task<PaymentResponse> GetPaymentDetails(string paymentId)
        {
            PaymentConfiguration payconfig = await GetPaymentConfigAsync();
            NameValueCollection header = new NameValueCollection();
            header.Add("Authorization", payconfig.WebExperienceProfileId);
            PaymentResponse response = await PostAsync<PaymentResponse>(header, string.Format(Constant.PaymentResponseUrl, payconfig.ClientId, paymentId));
            return await Task.FromResult(response);
        }

        /// <summary>
        /// Get Payment status. 
        /// </summary>
        /// <param name="paymentId">The PaymentId.</param>
        /// <returns>returns transaction response.</returns>
        public static async Task<TransactionStatusResponse> GetPaymentStatus(string paymentId)
        {
            PaymentConfiguration payconfig = await GetPaymentConfigAsync();
            NameValueCollection header = new NameValueCollection();
            header.Add("Authorization", payconfig.WebExperienceProfileId);
            TransactionStatusResponse response = await PostAsync<TransactionStatusResponse>(header, string.Format(Constant.PaymentStatusUrl, payconfig.ClientId, paymentId));
            return await Task.FromResult(response);
        }

        /// <summary>
        /// Initiate Refund. 
        /// </summary>
        /// <param name="paymentId">The PaymentId.</param>
        /// <param name="amount">The Amount.</param>
        /// <returns>returns PayUMoneyRefundResponse.</returns>
        public static async Task<RefundResponse> RefundPayment(string paymentId, string amount)
        {
            PaymentConfiguration payconfig = await GetPaymentConfigAsync();
            NameValueCollection header = new NameValueCollection();
            header.Add("Authorization", payconfig.WebExperienceProfileId);
            RefundResponse response = await PostAsync<RefundResponse>(header, string.Format(Constant.PaymentRefundUrl, payconfig.ClientId, paymentId, amount));
            return await Task.FromResult(response);
        }

        /// <summary>
        /// Get Payment configuration. 
        /// </summary>
        /// <returns>return payment configuration</returns>
        private static async Task<PaymentConfiguration> GetPaymentConfigAsync()
        {
            PaymentConfiguration paymentConfig = await ApplicationDomain.Instance.PaymentConfigurationRepository.RetrieveAsync();

            return paymentConfig;
        }

        /// <summary>
        /// Make Post API call on give path with given header
        /// </summary>
        /// <typeparam name="T">Class Name</typeparam>
        /// <param name="header">The Header</param>
        /// <param name="path">Post URL path</param>
        /// <returns>Returns response</returns>
        private static async Task<T> PostAsync<T>(NameValueCollection header, string path)
        {
            HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Post, path);
            message.Headers.Add("Accept", "application/json");
            message.Headers.TryAddWithoutValidation("Authorization", header.Get("Authorization"));
            T data = default(T);
            HttpResponseMessage response = await client.SendAsync(message);
            if (response.IsSuccessStatusCode)
            {
                data = await response.Content.ReadAsAsync<T>();
            }

            return data;
        }
    }
}