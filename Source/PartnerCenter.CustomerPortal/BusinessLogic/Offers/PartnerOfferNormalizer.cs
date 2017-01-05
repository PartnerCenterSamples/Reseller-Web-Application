// -----------------------------------------------------------------------
// <copyright file="PartnerOfferNormalizer.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.CustomerPortal.BusinessLogic.Offers
{
    using System;
    using System.Collections.Generic;
    using Exceptions;
    using Models;

    /// <summary>
    /// Applies business rules to a partner offer.
    /// </summary>
    public class PartnerOfferNormalizer
    {
        /// <summary>
        /// Applies business rules to a partner offer.
        /// </summary>
        /// <param name="partnerOffer">The partner offer to normalize.</param>
        public void Normalize(PartnerOffer partnerOffer)
        {
            partnerOffer.AssertNotNull(nameof(partnerOffer));

            // ensure the Microsoft offer ID and other required properties are set
            Guid offerId;

            if (!Guid.TryParse(partnerOffer.Id, out offerId))
            {
                throw new PartnerDomainException(ErrorCode.InvalidInput, Resources.IdMustBeAValidGUID).AddDetail("Field", "Id");
            }

            if (string.IsNullOrWhiteSpace(partnerOffer.MicrosoftOfferId))
            {
                throw new PartnerDomainException(ErrorCode.InvalidInput, Resources.MicrosoftOfferIdMustBeSet).AddDetail("Field", "MicrosoftOfferId");
            }

            partnerOffer.Title.AssertNotEmpty("Offer title");
            
            if (partnerOffer.Price <= 0)
            {
                throw new PartnerDomainException(ErrorCode.InvalidInput, Resources.OfferPriceShouldBeMoreThanZero).AddDetail("Field", "Price");
            }

            // flatten the offer price based on locale decimal settings. 
            partnerOffer.Price = Math.Round(partnerOffer.Price, Resources.Culture.NumberFormat.CurrencyDecimalDigits, MidpointRounding.AwayFromZero);             

            partnerOffer.Features = PartnerOfferNormalizer.CleanupEmptyEntries(partnerOffer.Features);
            partnerOffer.Summary = PartnerOfferNormalizer.CleanupEmptyEntries(partnerOffer.Summary);
        }

        /// <summary>
        /// Removes empty elements from a given enumerable.
        /// </summary>
        /// <param name="enumerable">The enumerable to clean up.</param>
        /// <returns>The cleaned up enumerable.</returns>
        private static IEnumerable<string> CleanupEmptyEntries(IEnumerable<string> enumerable)
        {
            if (enumerable == null)
            {
                return null;
            }

            ICollection<string> filteredList = new List<string>();

            foreach (var element in enumerable)
            {
                if (!string.IsNullOrWhiteSpace(element))
                {
                    filteredList.Add(element);
                }
            }

            return filteredList;
        }
    }
}