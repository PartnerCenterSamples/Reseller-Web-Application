// -----------------------------------------------------------------------
// <copyright file="IGraphClient.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Storefront.BusinessLogic
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Models;

    /// <summary>
    /// Represents an object that interacts with Microsoft Azure AD Graph API.
    /// </summary>
    public interface IGraphClient
    {
        /// <summary>
        /// Gets a list of roles assigned to the specified object identifier.
        /// </summary>
        /// <param name="objectId">Object identifier for the object to be checked.</param>
        /// <returns>A list of roles that that are associated with the specified object identifier.</returns>
        Task<List<RoleModel>> GetDirectoryRolesAsync(string objectId);
    }
}