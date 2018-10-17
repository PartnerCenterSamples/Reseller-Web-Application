// -----------------------------------------------------------------------
// <copyright file="PaymentConfiguration.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Storefront.Models
{
    /// <summary>
    /// Holds payment configuration properties.
    /// </summary>
    public class PaymentConfiguration
    {
        /// <summary>
        /// Gets or sets the payment client Id.
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// Gets or sets the payment client secret.
        /// </summary>
        public string ClientSecret { get; set; }

        /// <summary>
        /// Gets or sets the payment account type.
        /// </summary>
        public string AccountType { get; set; }

        /// <summary>
        /// Gets or sets the Web Experience Profile Id.
        /// </summary>
        public string WebExperienceProfileId { get; set; }
    }
}