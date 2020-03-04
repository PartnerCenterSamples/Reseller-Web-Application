// -----------------------------------------------------------------------
// <copyright file="PartnerOffer.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Storefront.Models
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents partner offers.
    /// </summary>
    public class PartnerOffer
    {
        /// <summary>
        /// Gets or sets the unique ID of the partner offer.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the Microsoft offer ID associated with the partner offer.
        /// </summary>
        public string MicrosoftOfferId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the offer is active or has been inactivated by the partner.
        /// </summary>
        public bool IsInactive { get; set; }

        /// <summary>
        /// Gets or sets the partner offer title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the partner offer subtitle.
        /// </summary>
        public string Subtitle { get; set; }

        /// <summary>
        /// Gets or sets a detailed list of features the offer provides to its customers.
        /// </summary>
        public IEnumerable<string> Features { get; set; }

        /// <summary>
        /// Gets or sets a summary list of the offer.
        /// </summary>
        public IEnumerable<string> Summary { get; set; }

        /// <summary>
        /// Gets or sets the offer's monthly price as per the license.
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Gets or sets the offer's thumbnail image.
        /// </summary>
        public Uri Thumbnail { get; set; }

        /// <summary>
        /// Gets or sets the offer's display index.
        /// </summary>
        public int DisplayIndex { get; set; }
    }
}