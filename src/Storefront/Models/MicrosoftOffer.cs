// -----------------------------------------------------------------------
// <copyright file="MicrosoftOffer.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Storefront.Models
{
    using System;
    using PartnerCenter.Models.Offers;

    /// <summary>
    /// Represents a Microsoft offer.
    /// </summary>
    public class MicrosoftOffer
    {
        /// <summary>
        /// Gets or sets the Microsoft offer details.
        /// </summary>
        public Offer Offer { get; set; }

        /// <summary>
        /// Gets or sets the offer's thumbnail URI.
        /// </summary>
        public Uri ThumbnailUri { get; set; }
    }
}