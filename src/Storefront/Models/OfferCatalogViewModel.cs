// -----------------------------------------------------------------------
// <copyright file="OfferCatalogViewModel.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Storefront.Models
{
    using System.Collections.Generic;

    /// <summary>
    /// The offer catalog view model.
    /// </summary>
    public class OfferCatalogViewModel
    {
        /// <summary>
        /// Gets or sets the partner offers.
        /// </summary>
        public IEnumerable<PartnerOffer> Offers { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the portal has been configured or not.
        /// </summary>
        public bool IsPortalConfigured { get; set; }
    }
}