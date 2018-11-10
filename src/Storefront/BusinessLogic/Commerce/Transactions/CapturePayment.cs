// -----------------------------------------------------------------------
// <copyright file="CapturePayment.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Storefront.BusinessLogic.Commerce.Transactions
{
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using Infrastructure;

    /// <summary>
    /// Captures a payment.
    /// </summary>
    public class CapturePayment : IBusinessTransactionWithInput<string>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CapturePayment"/> class.
        /// </summary>
        /// <param name="paymentGateway">The payment gateway to use for capturing payments.</param>
        /// <param name="authorizationCode">The authorization code to capture.</param>
        public CapturePayment(IPaymentGateway paymentGateway, string authorizationCode)
        {
            paymentGateway.AssertNotNull(nameof(paymentGateway));
            authorizationCode.AssertNotEmpty(nameof(paymentGateway));

            PaymentGateway = paymentGateway;
            AuthorizationCode = authorizationCode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CapturePayment"/> class.
        /// </summary>
        /// <param name="paymentGateway">The payment gateway to use for capturing payments.</param>
        /// <param name="acquireAuthorizationCallFunction">The function to call to obtain the authorization code.</param>
        public CapturePayment(IPaymentGateway paymentGateway, Func<string> acquireAuthorizationCallFunction)
        {
            paymentGateway.AssertNotNull(nameof(paymentGateway));
            acquireAuthorizationCallFunction.AssertNotNull(nameof(acquireAuthorizationCallFunction));

            PaymentGateway = paymentGateway;
            AcquireInput = acquireAuthorizationCallFunction;
        }

        /// <summary>
        /// Gets the function that is called to retrieve the authorization code.
        /// </summary>
        public Func<string> AcquireInput { get; private set; }

        /// <summary>
        /// Gets the authorization code used for capturing payments.
        /// </summary>
        public string AuthorizationCode { get; private set; }

        /// <summary>
        /// Gets the payment gateway used to capture payments.
        /// </summary>
        public IPaymentGateway PaymentGateway { get; private set; }

        /// <summary>
        /// Captures the payment.
        /// </summary>
        /// <returns>A task.</returns>
        public async Task ExecuteAsync()
        {
            if (string.IsNullOrEmpty(AuthorizationCode))
            {
                AuthorizationCode = AcquireInput.Invoke();
            }

            await PaymentGateway.CaptureAsync(AuthorizationCode).ConfigureAwait(false);
        }

        /// <summary>
        /// Rolls back the payment capture.
        /// </summary>
        /// <returns>A task.</returns>
        public async Task RollbackAsync()
        {
            // no known way to rollback a captured payment, just log the fact
            Trace.TraceInformation("CapturePayment.RollbackAsync executed. Authorization code: {0}", AuthorizationCode);

            // TODO: Notify the system integrity recovery component
            await Task.CompletedTask.ConfigureAwait(false);
        }
    }
}