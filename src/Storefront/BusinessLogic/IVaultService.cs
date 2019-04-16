// -----------------------------------------------------------------------
// <copyright file="IVaultService.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Storefront.BusinessLogic
{
    using System.Security;
    using System.Threading.Tasks;

    /// <summary>
    /// Represents a secure mechanism for retrieving sensitive information. 
    /// </summary>
    public interface IVaultService
    {
        /// <summary>
        /// Gets the specified entity from the vault. 
        /// </summary>
        /// <param name="identifier">Identifier of the entity to be retrieved.</param>
        /// <returns>The value retrieved from the vault.</returns>
        Task<SecureString> GetAsync(string identifier);
    }
}