// -----------------------------------------------------------------------
// <copyright file="Constant.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Storefront.BusinessLogic.Commerce.PaymentGateways.PayUMoney
{
    /// <summary>
    /// Constant class
    /// </summary>
    public static class Constant
    {
        /// <summary>
        /// PaymentResponseUrl url.
        /// </summary>
        public const string PaymentResponseUrl = "https://www.payumoney.com/payment/op/getPaymentResponse?merchantKey={0}&merchantTransactionIds={1}";

        /// <summary>
        /// PaymentStatusUrl url.
        /// </summary>
        public const string PaymentStatusUrl = "https://www.payumoney.com/payment/payment/chkMerchantTxnStatus?merchantKey={0}&merchantTransactionIds={1}";

        /// <summary>
        /// PaymentRefundUrl url.
        /// </summary>
        public const string PaymentRefundUrl = "https://www.payumoney.com/treasury/merchant/refundPayment?merchantKey={0}&paymentId={1}&refundAmount={2}";

        /// <summary>
        /// MoneyWithPayU url.
        /// </summary>
        public const string MoneyWithPayU = "Money with Payumoney";

        /// <summary>
        /// Test url.
        /// </summary>
        public static readonly string TESTPAYUURL = "https://test.payu.in/_payment";

        /// <summary>
        /// Live url.
        /// </summary>
        public static readonly string LIVEPAYUURL = "https://secure.payu.in/_payment";

        /// <summary>
        /// Hash sequence.
        /// </summary>
        public static readonly string HASHSEQUENCE = "key|txnid|amount|productinfo|firstname|email|udf1|udf2|udf3|udf4|udf5|udf6|udf7|udf8|udf9|udf10";

        /// <summary>
        /// Maintains the payment id for the payment gateway.
        /// </summary>
        public static readonly string PAYUPAISASERVICEPROVIDER = "payu_paisa";
    }
}