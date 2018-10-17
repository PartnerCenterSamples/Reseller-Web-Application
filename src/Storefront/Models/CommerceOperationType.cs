// -----------------------------------------------------------------------
// <copyright file="CommerceOperationType.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Storefront.Models
{
    /// <summary>
    /// Enumerates the types of commerce operations done by the portal.
    /// </summary>
    public enum CommerceOperationType
    {
        /// <summary>
        /// A brand new purchase.
        /// </summary>
        NewPurchase,

        /// <summary>
        /// Purchase of additional seats for an existing subscription.
        /// </summary>
        AdditionalSeatsPurchase,

        /// <summary>
        /// Existing subscription renewal.
        /// </summary>
        Renewal
    }
}