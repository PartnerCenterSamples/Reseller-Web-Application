// -----------------------------------------------------------------------
// <copyright file="UserRole.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Storefront.Filters
{
    /// <summary>
    /// Defines different user roles.
    /// </summary>
    public enum UserRole
    {
        /// <summary>
        /// A customer of the partner.
        /// </summary>
        Customer,

        /// <summary>
        /// A partner user.
        /// </summary>
        Partner,

        /// <summary>
        /// A customer or a partner.
        /// </summary>
        Any,

        /// <summary>
        /// An unauthenticated user.
        /// </summary>
        None
    }
}