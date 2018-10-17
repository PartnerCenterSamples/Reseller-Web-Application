// -----------------------------------------------------------------------
// <copyright file="RoleModel.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Storefront.Models
{
    /// <summary>
    /// Represents a role in the application utilized to grant permissions. 
    /// </summary>
    public class RoleModel
    {
        /// <summary>
        /// Gets or sets the description for the role.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the name associated with the role.
        /// </summary>
        public string DisplayName { get; set; }
    }
}