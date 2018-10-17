// -----------------------------------------------------------------------
// <copyright file="PayUGateway.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Storefront.BusinessLogic.Commerce.PaymentGateways
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text;
    using System.Threading.Tasks;
    using Exceptions;
    using Models;
    using PartnerCenter.Models.Customers;
    using PayUMoney;

    /// <summary>
    /// PayUMoney payment gateway implementation.
    /// </summary>
    public class PayUGateway : DomainObject, IPaymentGateway
    {
        /// <summary>
        /// Maintains the description for this payment. 
        /// </summary>
        private readonly string paymentDescription;

        /// <summary>
        /// Maintains the payer id for the payment gateway. 
        /// </summary>
        private string payerId;

        /// <summary>
        /// Maintains the payment id for the payment gateway.
        /// </summary>
        private string paymentId;

        /// <summary>
        /// Initializes a new instance of the <see cref="PayUGateway" /> class. 
        /// </summary>
        /// <param name="applicationDomain">The ApplicationDomain</param>        
        /// <param name="description">The description which will be added to the Payment Card authorization call.</param>
        public PayUGateway(ApplicationDomain applicationDomain, string description) : base(applicationDomain)
        {
            description.AssertNotEmpty(nameof(description));
            paymentDescription = description;

            payerId = string.Empty;
            paymentId = string.Empty;
        }

        /// <summary>
        /// Validates payment configuration. 
        /// </summary>
        /// <param name="paymentConfig">The Payment configuration.</param>
        public void ValidateConfiguration(PaymentConfiguration paymentConfig)
        {
            ////Payu does not provide payment profile validation api.
        }

        /// <summary>
        /// Creates Web Experience profile using portal branding and payment configuration. 
        /// </summary>
        /// <param name="paymentConfig">The Payment configuration.</param>
        /// <param name="brandConfig">The branding configuration.</param>
        /// <param name="countryIso2Code">The locale code used by the web experience profile. Example-US.</param>
        /// <returns>The created web experience profile id.</returns>
        public string CreateWebExperienceProfile(PaymentConfiguration paymentConfig, BrandingConfiguration brandConfig, string countryIso2Code)
        {
            ////Payu does not provide the concept of webprofile
            ////stored authorization in WebExperienceProfileId
            return paymentConfig.WebExperienceProfileId;
        }

        /// <summary>
        /// Creates a payment transaction and returns the PayUMoney generated payment URL. 
        /// </summary>
        /// <param name="returnUrl">The redirect url for PayUMoney callback to web store portal.</param>                
        /// <param name="order">The order details for which payment needs to be made.</param>        
        /// <returns>Payment URL from PayUMoney.</returns>
        public async Task<string> GeneratePaymentUriAsync(string returnUrl, OrderViewModel order)
        {
            returnUrl.AssertNotEmpty(nameof(returnUrl));
            order.AssertNotNull(nameof(order));
            RemotePost myremotepost = await PrepareRemotePost(order, returnUrl).ConfigureAwait(false);
            return myremotepost.Post();
        }

        /// <summary>
        /// Executes a PayU payment.
        /// </summary>
        /// <returns>Capture string id.</returns>
        public async Task<string> ExecutePaymentAsync()
        {
            try
            {
                TransactionStatusResponse paymentResponse = await ApiCalls.GetPaymentStatus(paymentId).ConfigureAwait(false);
                if (paymentResponse != null && paymentResponse.Result.Count > 0 && paymentResponse.Result[0].Status.Equals(Constant.MoneyWithPayU, StringComparison.InvariantCultureIgnoreCase))
                {
                    return paymentResponse.Result[0].Amount.ToString(CultureInfo.InvariantCulture);
                }
            }
            catch (Exception ex)
            {
                this.ParsePayUException(ex);
            }

            return await Task.FromResult(string.Empty).ConfigureAwait(false);
        }

        /// <summary>
        /// Finalizes an authorized payment with PayU.
        /// </summary>
        /// <param name="authorizationCode">The authorization code for the payment to capture.</param>
        /// <returns>A task.</returns>
        public async Task CaptureAsync(string authorizationCode)
        {
            ////PayU api not provided
            await Task.FromResult(string.Empty).ConfigureAwait(false);
        }

        /// <summary>
        /// Voids an authorized payment with PayUMoney.
        /// </summary>
        /// <param name="authorizationCode">The authorization code for the payment to void.</param>
        /// <returns>a Task</returns>
        public async Task VoidAsync(string authorizationCode)
        {
            authorizationCode.AssertNotEmpty(nameof(authorizationCode));

            // given the authorizationId string... Lookup the authorization to void it. 
            try
            {
                RefundResponse refundResponse = await ApiCalls.RefundPayment(payerId, authorizationCode).ConfigureAwait(false);
                if (refundResponse.Status != 0 || !refundResponse.Message.Equals("Refund Initiated", StringComparison.InvariantCulture))
                {
                    throw new Exception("Error in refund");
                }
            }
            catch (Exception ex)
            {
                this.ParsePayUException(ex);
            }
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
            // this payment gateway ignores orderId & customerId. 
            payerId.AssertNotEmpty(nameof(payerId));
            paymentId.AssertNotEmpty(nameof(paymentId));

            this.payerId = payerId;
            this.paymentId = paymentId;

            return await GetOrderDetails().ConfigureAwait(false);
        }

        /// <summary>
        /// Generate hash. 
        /// </summary>
        /// <param name="text">hash string.</param>
        /// <returns>return string</returns>
        private string GenerateHash512(string text)
        {
            byte[] message = Encoding.UTF8.GetBytes(text);

            UnicodeEncoding ue = new UnicodeEncoding();
            byte[] hashValue;
            System.Security.Cryptography.SHA512Managed hashString = new System.Security.Cryptography.SHA512Managed();
            string hex = string.Empty;
            hashValue = hashString.ComputeHash(message);
            foreach (byte x in hashValue)
            {
                hex += string.Format(CultureInfo.InvariantCulture, "{0:x2}", x);
            }

            return hex;
        }

        /// <summary>
        /// Generate transaction id. 
        /// </summary>
        /// <returns>return string</returns>
        private string GenerateTransactionId()
        {
            Random rnd = new Random();
            string strHash = GenerateHash512(rnd.ToString() + DateTime.Now);
            string txnid1 = strHash.ToString(CultureInfo.InvariantCulture).Substring(0, 20);
            return txnid1;
        }

        /// <summary>
        /// Retrieves the order details maintained for the payment gateway.  
        /// </summary>
        /// <returns>return order data.</returns>
        private async Task<OrderViewModel> GetOrderDetails()
        {
            OrderViewModel orderFromPayment = null;
            try
            {
                PaymentResponse paymentResponse = await ApiCalls.GetPaymentDetails(paymentId).ConfigureAwait(false);
                if (paymentResponse != null && paymentResponse.Result.Count > 0)
                {
                    orderFromPayment = await GetOrderDetails(paymentResponse.Result[0].PostBackParam.Udf1, paymentResponse.Result[0].PostBackParam.ProductInformation, paymentResponse.Result[0].PostBackParam.Udf2).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                ParsePayUException(ex);
            }

            return await Task.FromResult(orderFromPayment).ConfigureAwait(false);
        }

        /// <summary>
        /// get payment url.
        /// </summary>
        /// <param name="mode">mode of payment gateway.</param>
        /// <returns>return string.</returns>
        private string GetPaymentUrl(string mode)
        {
            ////two modes are possible sandbox and live
            if (mode.Equals("sandbox", StringComparison.InvariantCultureIgnoreCase))
            {
                return Constant.TESTPAYUURL;
            }

            return Constant.LIVEPAYUURL;
        }

        /// <summary>
        /// prepares Remote post by populate all the necessary fields to generate PayUMoney post request. 
        /// </summary>
        /// <param name="order">order details.</param>                
        /// <param name="returnUrl">return url.</param>        
        /// <returns>return remote post.</returns>
        private async Task<RemotePost> PrepareRemotePost(OrderViewModel order, string returnUrl)
        {
            string fname = string.Empty;
            string phone = string.Empty;
            string email = string.Empty;

            CustomerRegistrationRepository customerRegistrationRepository = new CustomerRegistrationRepository(ApplicationDomain.Instance);
            CustomerViewModel customerRegistrationInfo = await customerRegistrationRepository.RetrieveAsync(order.CustomerId).ConfigureAwait(false);
            if (customerRegistrationInfo == null)
            {
                Customer customer = await ApplicationDomain.Instance.PartnerCenterClient.Customers.ById(order.CustomerId).GetAsync().ConfigureAwait(false);
                fname = customer.BillingProfile.DefaultAddress.FirstName;
                phone = customer.BillingProfile.DefaultAddress.PhoneNumber;
                email = customer.BillingProfile.Email;
            }
            else
            {
                fname = customerRegistrationInfo.FirstName;
                phone = customerRegistrationInfo.Phone;
                email = customerRegistrationInfo.Email;
            }

            decimal paymentTotal = 0;
            StringBuilder productSubs = new StringBuilder();
            StringBuilder prodQuants = new StringBuilder();
            foreach (OrderSubscriptionItemViewModel subscriptionItem in order.Subscriptions)
            {
                productSubs.Append(":").Append(subscriptionItem.SubscriptionId);
                prodQuants.Append(":").Append(subscriptionItem.Quantity.ToString(CultureInfo.InvariantCulture));
                paymentTotal += Math.Round(subscriptionItem.Quantity * subscriptionItem.SeatPrice, Resources.Culture.NumberFormat.CurrencyDecimalDigits);
            }

            productSubs.Remove(0, 1);
            prodQuants.Remove(0, 1);
            System.Collections.Specialized.NameValueCollection inputs = new System.Collections.Specialized.NameValueCollection();
            PaymentConfiguration payconfig = await GetAPaymentConfigAsync().ConfigureAwait(false);
            inputs.Add("key", payconfig.ClientId);
            inputs.Add("txnid", GenerateTransactionId());
            inputs.Add("amount", paymentTotal.ToString(CultureInfo.InvariantCulture));
            inputs.Add("productinfo", productSubs.ToString());
            inputs.Add("firstname", fname);
            inputs.Add("phone", phone);
            inputs.Add("email", email);
            inputs.Add("udf1", order.OperationType.ToString());
            inputs.Add("udf2", prodQuants.ToString());
            inputs.Add("surl", returnUrl + "&payment=success&PayerId=" + inputs.Get("txnid"));
            inputs.Add("furl", returnUrl + "&payment=failure&PayerId=" + inputs.Get("txnid"));
            inputs.Add("service_provider", Constant.PAYUPAISASERVICEPROVIDER);
            string hashString = inputs.Get("key") + "|" + inputs.Get("txnid") + "|" + inputs.Get("amount") + "|" + inputs.Get("productInfo") + "|" + inputs.Get("firstName") + "|" + inputs.Get("email") + "|" + inputs.Get("udf1") + "|" + inputs.Get("udf2") + "|||||||||" + payconfig.ClientSecret; // payconfig.ClientSecret;
            string hash = this.GenerateHash512(hashString);
            inputs.Add("hash", hash);

            RemotePost myremotepost = new RemotePost();
            myremotepost.SetUrl(this.GetPaymentUrl(payconfig.AccountType));
            myremotepost.SetInputs(inputs);
            return myremotepost;
        }

        /// <summary>
        /// Throws PartnerDomainException by parsing PayUMoney exception. 
        /// </summary>
        /// <param name="ex">Exceptions from PayUMoney API call.</param>        
        private void ParsePayUException(Exception ex)
        {
            throw new PartnerDomainException(ErrorCode.PaymentGatewayFailure).AddDetail("ErrorMessage", ex.Message);
        }

        /// <summary>
        /// Retrieves the Order from a payment transaction.
        /// </summary>
        /// <param name="operation">operation data.</param>
        /// <param name="prod">product data.</param>
        /// <param name="quant">quantity data.</param>
        /// <returns>The Order for which payment was made.</returns>
        private async Task<OrderViewModel> GetOrderDetails(string operation, string prod, string quant)
        {
            OrderViewModel orderFromPayment = null;
            try
            {
                orderFromPayment = new OrderViewModel();
                List<OrderSubscriptionItemViewModel> orderSubscriptions = new List<OrderSubscriptionItemViewModel>();

                orderFromPayment.OperationType = (CommerceOperationType)Enum.Parse(typeof(CommerceOperationType), operation, true);
                string[] prodList = prod.Split(':');
                string[] quantList = quant.Split(':');

                for (int i = 0; i < prodList.Length; i++)
                {
                    orderSubscriptions.Add(new OrderSubscriptionItemViewModel()
                    {
                        SubscriptionId = prodList[i],
                        OfferId = prodList[i],
                        Quantity = Convert.ToInt32(quantList[i], CultureInfo.InvariantCulture)
                    });
                }

                orderFromPayment.Subscriptions = orderSubscriptions;
            }
            catch (Exception ex)
            {
                ParsePayUException(ex);
            }

            return await Task.FromResult(orderFromPayment).ConfigureAwait(false);
        }

        /// <summary>
        /// Throws PartnerDomainException by parsing PayUMoney exception. 
        /// </summary>
        /// <returns>return payment configuration</returns>
        private async Task<PaymentConfiguration> GetAPaymentConfigAsync()
        {
            // Before getAPIContext ... set up PayUMoney configuration. This is an expensive call which can benefit from caching. 
            PaymentConfiguration paymentConfig = await ApplicationDomain.Instance.PaymentConfigurationRepository.RetrieveAsync().ConfigureAwait(false);

            return paymentConfig;
        }

        /// <summary>
        /// Remote post class.
        /// </summary>
        private class RemotePost
        {
            /// <summary>
            /// Maintains Url. 
            /// </summary>
            private string url = string.Empty;

            /// <summary>
            /// Maintains Method. 
            /// </summary>
            private readonly string method = "post";

            /// <summary>
            /// Maintains form name. 
            /// </summary>
            private readonly string formName = "form1";

            /// <summary>
            /// Maintains input collection. 
            /// </summary>
            private System.Collections.Specialized.NameValueCollection inputs = new System.Collections.Specialized.NameValueCollection();

            /// <summary>
            /// Retrieves the API Context for PayUMoney. 
            /// </summary>
            /// <param name="u">url string.</param>
            public void SetUrl(string u)
            {
                this.url = u;
            }

            /// <summary>
            /// Retrieves the API Context for PayUMoney. 
            /// </summary>
            /// <param name="name">name string.</param>
            /// <param name="value">value string.</param>
            public void Add(string name, string value)
            {
                this.inputs.Add(name, value);
            }

            /// <summary>
            /// Retrieves the API Context for PayUMoney. 
            /// </summary>
            /// <param name="inputs">collection of values.</param>
            public void SetInputs(System.Collections.Specialized.NameValueCollection inputs)
            {
                this.inputs = inputs;
            }

            /// <summary>
            /// prepare form string.
            /// </summary>
            /// <returns>return form string</returns>
            public string Post()
            {
                System.Web.HttpContext.Current.Response.Clear();
                StringBuilder responseForm = new StringBuilder();
                responseForm.Append(string.Format(CultureInfo.InvariantCulture, "<form name=\"{0}\" method=\"{1}\" action=\"{2}\" >", this.formName, this.method, this.url));
                for (int i = 0; i < this.inputs.Keys.Count; i++)
                {
                    responseForm.Append(string.Format(CultureInfo.InvariantCulture, "<input name=\"{0}\" type=\"hidden\" value=\"{1}\">", this.inputs.Keys[i], this.inputs[this.inputs.Keys[i]]));
                }

                responseForm.Append("</form>");
                responseForm.Append(string.Format(CultureInfo.InvariantCulture, "<script language='javascript'>document.{0}.submit();</script>", this.formName));
                return responseForm.ToString();
            }
        }
    }
}