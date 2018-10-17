// -----------------------------------------------------------------------
// <copyright file="AdminConsoleViewModel.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Storefront.Models
{
    /// <summary>
    /// The admin console view model.
    /// </summary>
    public class AdminConsoleViewModel
    {
        /// <summary>
        /// Gets or sets a value indicating whether the partner offers have been configured or not.
        /// </summary>
        public bool IsOffersConfigured { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the website's branding has been configured or not.
        /// </summary>
        public bool IsBrandingConfigured { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the website's payment options have been configured or not.
        /// </summary>
        public bool IsPaymentConfigured { get; set; }
    }    
}